using Coalesce.Configuration;
using Coalesce.Utils;

namespace Coalesce.Core;

public class FileFilter
{
    private static readonly HashSet<string> s_defaultExcludedDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git"
    };

    private static readonly HashSet<string> s_defaultExcludedFileNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ".DS_Store",
        "Thumbs.db",
        "coalesce.toml"
    };

    private readonly AppOptions _options;

    public FileFilter(AppOptions options)
    {
        _options = options;
    }

    public bool ShouldSkip(string filePath, string currentSourceDirectoryRoot)
    {
        if (IsOutputFile(filePath))
        {
            return true;
        }

        if (IsConfigFile(filePath))
        {
            return true;
        }

        if (IsExcludedFileName(filePath))
        {
            return true;
        }

        if (IsInExcludedDirectory(filePath, currentSourceDirectoryRoot))
        {
            return true;
        }

        if (IsExcludedByExtension(filePath))
        {
            return true;
        }

        return IsMissingFromInclusionList(filePath);
    }

    private bool IsOutputFile(string filePath)
    {
        if (!string.Equals(filePath, _options.OutputFilePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Log.Verbose($"Skipping '{filePath}' because it is the output file.");
        return true;
    }

    private bool IsConfigFile(string filePath)
    {
        if (string.IsNullOrEmpty(_options.ConfigFilePath) ||
            !string.Equals(filePath, _options.ConfigFilePath, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        Log.Verbose($"Skipping '{filePath}' because it is the configuration file.");
        return true;
    }

    private bool IsExcludedFileName(string filePath)
    {
        string fileName = Path.GetFileName(filePath);

        if (s_defaultExcludedFileNames.Contains(fileName) ||
            _options.ExcludeFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
        {
            Log.Verbose($"Skipping '{fileName}' due to excluded file name rule.");
            return true;
        }

        return false;
    }

    private bool IsInExcludedDirectory(string filePath, string currentSourceDirectoryRoot)
    {
        string relativePathFromSource = Path.GetRelativePath(currentSourceDirectoryRoot, filePath);
        string[] pathSegments = relativePathFromSource.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (pathSegments.Length <= 1)
        {
            return false;
        }

        IEnumerable<string> directorySegments = pathSegments.Take(pathSegments.Length - 1);

        foreach (string segment in directorySegments)
        {
            if (!s_defaultExcludedDirectories.Contains(segment) &&
                !_options.ExcludeDirectoryNames.Contains(segment, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            Log.Verbose($"Skipping '{filePath}' because it's in an excluded directory ('{segment}').");
            return true;
        }

        return false;
    }

    private bool IsExcludedByExtension(string filePath)
    {
        if (_options.ExcludeExtensions.Count == 0)
        {
            return false;
        }

        string fileExtension = Path.GetExtension(filePath);

        if (_options.ExcludeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            Log.Verbose($"Skipping '{Path.GetFileName(filePath)}' due to 'exclude_extensions' rule for '{fileExtension}'.");
            return true;
        }

        return false;
    }

    private bool IsMissingFromInclusionList(string filePath)
    {
        if (_options.IncludeExtensions.Count == 0)
        {
            return false;
        }

        string fileExtension = Path.GetExtension(filePath);
        if (!_options.IncludeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            Log.Verbose($"Skipping '{Path.GetFileName(filePath)}' because its extension ('{fileExtension}') is not in 'include_extensions'.");
            return true;
        }

        return false;
    }
}
