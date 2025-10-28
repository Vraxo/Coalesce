using Coalesce.Configuration;
using Coalesce.Utils;

namespace Coalesce.Core;

public static class PathValidator
{
    public static bool TryValidateAndPrepare(AppOptions options)
    {
        if (!TryResolveAndValidateOutput(options))
        {
            return false;
        }

        ResolveValidSourcePaths(options);

        if (options.ValidSourceDirectoryPaths.Count == 0)
        {
            Logger.WriteError("No valid source directories were provided or found.");
            return false;
        }

        return true;
    }

    private static bool TryResolveAndValidateOutput(AppOptions options)
    {
        options.OutputFilePath = Path.GetFullPath(options.OutputFilePath);
        string? outputDirectory = Path.GetDirectoryName(options.OutputFilePath);

        if (string.IsNullOrEmpty(outputDirectory) || !Directory.Exists(outputDirectory))
        {
            Logger.WriteError($"Output directory not found: '{outputDirectory}'. Please ensure the directory exists.");
            return false;
        }
        return true;
    }

    private static void ResolveValidSourcePaths(AppOptions options)
    {
        foreach (string sourcePath in options.SourceDirectoryPaths)
        {
            string fullSourcePath = Path.GetFullPath(sourcePath);

            if (Directory.Exists(fullSourcePath))
            {
                options.ValidSourceDirectoryPaths.Add(fullSourcePath);
                Logger.WriteInfo($"- Added Source Directory: {fullSourcePath}");
            }
            else
            {
                Logger.WriteWarning($"Source directory not found: '{fullSourcePath}'. Skipping.");
            }
        }
    }
}