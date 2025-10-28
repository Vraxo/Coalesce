using Coalesce.Configuration;

namespace Coalesce.Core;

public class OutputFileGenerator(TextWriter writer)
{
    private static readonly Dictionary<string, string> s_languageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        // Web
        { ".html", "html" },
        { ".css", "css" },
        { ".scss", "scss" },
        { ".js", "javascript" },
        { ".jsx", "javascript" },
        { ".ts", "typescript" },
        { ".tsx", "typescript" },
        { ".json", "json" },
        { ".xml", "xml" },
        { ".md", "markdown" },

        // C# / .NET
        { ".cs", "csharp" },
        { ".csproj", "xml" },
        
        // Other
        { ".py", "python" },
        { ".java", "java" },
        { ".sh", "shell" },
        { ".bat", "batch" },
        { ".ps1", "powershell" },
        { ".yaml", "yaml" },
        { ".yml", "yaml" },
        { ".txt", "text" },
    };

    private static string GetLanguageIdentifier(string fileExtension)
    {
        return s_languageMap.GetValueOrDefault(fileExtension, string.Empty);
    }

    public void WriteFileEntry(string filePath, AppOptions options)
    {
        try
        {
            string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, filePath);
            string fileExtension = Path.GetExtension(filePath);
            string language = GetLanguageIdentifier(fileExtension);

            // Use a Markdown header for the file path. This is more robust for rendering.
            writer.WriteLine($"### `{relativePath}`");
            writer.WriteLine(); // Blank line for spacing after header

            if (options.PathOnlyExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                writer.WriteLine($"<!-- Content of binary file '{Path.GetFileName(filePath)}' not included. -->");
            }
            else
            {
                writer.WriteLine($"```{language}");
                using StreamReader reader = new(filePath);
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    writer.WriteLine(line);
                }
                writer.WriteLine("```");
            }

            // A horizontal rule provides a clear visual separation between file entries in all Markdown viewers.
            writer.WriteLine();
            writer.WriteLine("---");
            writer.WriteLine();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"Could not process file '{filePath}'.", ex);
        }
    }
}