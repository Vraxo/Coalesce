using Coalesce.Configuration;
using Coalesce.Utils;

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
        // 1. Skip if it is the output file itself
        if (string.Equals(filePath, _options.OutputFilePath, StringComparison.OrdinalIgnoreCase))
        {
            Logger.WriteVerbose($"Skipping '{filePath}' because it is the output file.");
            return true;
        }

        // 2. Skip if it is an excluded file name
        string fileName = Path.GetFileName(filePath);
        if (_options.ExcludeFileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
        {
            Logger.WriteVerbose($"Skipping '{fileName}' due to 'excludeFileNames' rule.");
            return true;
        }

        // 3. Skip if it's in an excluded directory
        string relativePathFromSource = Path.GetRelativePath(currentSourceDirectoryRoot, filePath);
        string[] pathSegments = relativePathFromSource.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (pathSegments.Length > 1)
        {
            IEnumerable<string> directorySegments = pathSegments.Take(pathSegments.Length - 1);
            foreach (string? segment in directorySegments)
            {
                if (_options.ExcludeDirectoryNames.Contains(segment, StringComparer.OrdinalIgnoreCase))
                {
                    Logger.WriteVerbose($"Skipping '{filePath}' because it's in an excluded directory ('{segment}').");
                    return true;
                }
            }
        }

        // 4. Skip based on extension
        string fileExtension = Path.GetExtension(filePath);

        if (_options.ExcludeExtensions.Count > 0 && _options.ExcludeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
        {
            Logger.WriteVerbose($"Skipping '{fileName}' due to 'excludeExtensions' rule for '{fileExtension}'.");
            return true;
        }

        // If IncludeExtensions is used as a whitelist, we need to check against it.
        // PathOnlyExtensions also acts as an inclusion list.
        if (_options.IncludeExtensions.Count > 0)
        {
            bool isIncluded = _options.IncludeExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);
            bool isPathOnly = _options.PathOnlyExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase);

            if (!isIncluded && !isPathOnly)
            {
                Logger.WriteVerbose($"Skipping '{fileName}' because its extension ('{fileExtension}') is not in 'includeExtensions' or 'pathOnlyExtensions'.");
                return true; // Skip if not in either inclusion list
            }
        }

        return false;
    }
}