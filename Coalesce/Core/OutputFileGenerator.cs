namespace Coalesce.Core;

public class OutputFileGenerator(TextWriter writer)
{
    private const int BinaryCheckBufferSize = 8192;

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
        { ".props", "xml" },
        { ".targets", "xml" },
        { ".slnx", "xml" },

        // Other
        { ".py", "python" },
        { ".java", "java" },
        { ".sh", "shell" },
        { ".bat", "batch" },
        { ".ps1", "powershell" },
        { ".toml", "toml" },
        { ".yaml", "yaml" },
        { ".yml", "yaml" },
        { ".txt", "text" },
    };

    private static readonly HashSet<string> s_binaryExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        // Images & Icons
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".webp", ".avif", ".tiff",

        // Fonts
        ".woff", ".woff2", ".ttf", ".otf", ".eot",

        // Executables, Libraries & Symbols
        ".dll", ".exe", ".pdb", ".so", ".dylib", ".bin", ".class", ".jar", ".wasm",

        // Archives & Packages
        ".zip", ".tar", ".gz", ".tgz", ".bz2", ".xz", ".7z", ".rar", ".nupkg", ".snupkg",

        // Documents & Media
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".mp3", ".mp4", ".wav", ".ogg", ".flac", ".webm", ".avi", ".mov"
    };

    private static string GetLanguageIdentifier(string fileExtension)
    {
        return s_languageMap.GetValueOrDefault(fileExtension, string.Empty);
    }

    public void WriteFileEntry(string filePath)
    {
        try
        {
            string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, filePath);
            string fileExtension = Path.GetExtension(filePath);
            string language = GetLanguageIdentifier(fileExtension);

            // Use a Markdown header for the file path. This is more robust for rendering.
            writer.WriteLine($"### `{relativePath}`");
            writer.WriteLine(); // Blank line for spacing after header

            if (IsBinaryFile(filePath, fileExtension))
            {
                writer.WriteLine($"<!-- Content of binary file '{Path.GetFileName(filePath)}' not included. -->");
            }
            else
            {
                writer.WriteLine($"```{language}");
                using StreamReader reader = new(filePath);
                string? line;
                while ((line = reader.ReadLine()) is not null)
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

    private static bool IsBinaryFile(string filePath, string fileExtension)
    {
        if (s_binaryExtensions.Contains(fileExtension))
        {
            return true;
        }

        using FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        byte[] buffer = new byte[BinaryCheckBufferSize];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        return buffer.AsSpan(0, bytesRead).Contains((byte)0);
    }
}
