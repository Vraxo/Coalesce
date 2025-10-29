using Coalesce.Configuration;
using Coalesce.Utils;
using System.Linq;

namespace Coalesce.Core;

public class FileFilter
{
    private readonly AppOptions _options;

    public FileFilter(AppOptions options)
    {
        _options = options;
    }

    public bool ShouldSkip(string filePath, string currentSourceDirectoryRoot)
    {
        if (IsOutputFile(filePath)) return true;
        if (IsExcludedFileName(filePath)) return true;
        if (IsInExcludedDirectory(filePath, currentSourceDirectoryRoot)) return true;
        if (IsExcludedByExtension(filePath)) return true;
        if (IsMissingFromInclusionList(filePath)) return true;

        return false;
    }

    private bool IsOutputFile(string filePath)
    {
        if (string.Equals(filePath, _options.OutputFilePath, StringComparison.OrdinalIgnoreCase))
        {
            Logger.WriteVerbose($"Skipping '{filePath}' because it is the output file.");
            return true;
        }
        return false;
    }

    private bool IsExcludedFileName(string filePath)
    {
        string fileName = Path.GetFileName(filePath);
        if (_options.ExcludeFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
        {
            Logger.WriteVerbose($"Skipping '{fileName}' due to 'excludeFileNames' rule.");
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
            if (_options.ExcludeDirectoryNames.Contains(segment, StringComparer.OrdinalIgnoreCase))
            {
                Logger.WriteVerbose($"Skipping '{filePath}' because it's in an excluded directory ('{segment}').");
                return true;
            }
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
            Logger.WriteVerbose($"Skipping '{Path.GetFileName(filePath)}' due to 'excludeExtensions' rule for '{fileExtension}'.");
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
        bool isIncluded = _options.IncludeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
        bool isPathOnly = _options.PathOnlyExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

        if (!isIncluded && !isPathOnly)
        {
            Logger.WriteVerbose($"Skipping '{Path.GetFileName(filePath)}' because its extension ('{fileExtension}') is not in 'includeExtensions' or 'pathOnlyExtensions'.");
            return true;
        }
        return false;
    }
}