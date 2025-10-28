using Coalesce.Configuration;
using Coalesce.Utils;

namespace Coalesce.Core;

public class SourceFileProvider
{
    private readonly FileFilter _fileFilter;

    public SourceFileProvider(AppOptions options)
    {
        _fileFilter = new(options);
    }

    public IEnumerable<string> EnumerateEligibleFiles(string sourceDirectory)
    {
        try
        {
            return EnumerateEligibleFilesCore(sourceDirectory);
        }
        catch (Exception ex) when (ex is UnauthorizedAccessException or DirectoryNotFoundException)
        {
            Logger.WriteWarning(
                $"Could not access directory '{sourceDirectory}'." +
                $"Skipping. " +
                $"Error: {ex.Message}");

            return [];
        }
    }

    private IEnumerable<string> EnumerateEligibleFilesCore(string sourceDirectory)
    {
        foreach (string filePath in Directory.EnumerateFiles(sourceDirectory, "*.*", SearchOption.AllDirectories))
        {
            if (_fileFilter.ShouldSkip(filePath, sourceDirectory))
            {
                continue;
            }

            yield return filePath;
        }
    }
}