using Coalesce.Configuration;
using Coalesce.Utils;

namespace Coalesce.Core;

public class DirectoryMerger
{
    private readonly AppOptions _options;
    private readonly bool _dryRun;

    public DirectoryMerger(AppOptions options, bool dryRun = false)
    {
        _options = options;
        _dryRun = dryRun;
    }

    public void Merge()
    {
        if (!PathValidator.TryValidateAndPrepare(_options))
        {
            Logger.WriteSuggestion("\nRun 'coalesce --help' for a list of commands and options.");
            return;
        }

        if (_dryRun)
        {
            ExecuteDryRun();
        }
        else
        {
            ExecuteMerge();
        }
    }

    private void ExecuteDryRun()
    {
        Logger.WriteWarning("--- DRY RUN MODE ---");
        Logger.WriteInfo("The following files would be included in the merge output:");

        SourceFileProvider fileProvider = new(_options);
        int totalFilesProcessed = 0;

        foreach (string sourceDirectory in _options.ValidSourceDirectoryPaths)
        {
            foreach (string filePath in fileProvider.EnumerateEligibleFiles(sourceDirectory))
            {
                Logger.WriteInfo($"- {filePath}");
                totalFilesProcessed++;
            }
        }

        Logger.WriteInfo(string.Empty);
        PrintSummary(totalFilesProcessed, 0);
    }

    private void ExecuteMerge()
    {
        try
        {
            SafelyDeleteFile(_options.OutputFilePath);
            Logger.WriteInfo($"Starting merge process. Output will be saved to: {_options.OutputFilePath}");

            using StreamWriter writer = new(_options.OutputFilePath);
            (int processed, int skipped) = ProcessAllSources(writer);

            Logger.WriteInfo(string.Empty);
            PrintSummary(processed, skipped);
        }
        catch (Exception ex)
        {
            Logger.WriteError($"A critical error occurred during the merge process: {ex.Message}");
        }
    }

    private (int processed, int skipped) ProcessAllSources(StreamWriter writer)
    {
        int totalFilesProcessed = 0;
        int totalFilesSkipped = 0;

        SourceFileProvider fileProvider = new(_options);
        OutputFileGenerator outputGenerator = new(writer);

        foreach (string sourceDirectory in _options.ValidSourceDirectoryPaths)
        {
            Logger.WriteInfo($"\n--- Processing Source: {sourceDirectory} ---");

            foreach (string filePath in fileProvider.EnumerateEligibleFiles(sourceDirectory))
            {
                try
                {
                    outputGenerator.WriteFileEntry(filePath, _options);
                    totalFilesProcessed++;
                }
                catch (IOException ex)
                {
                    Logger.WriteWarning(ex.Message);
                    totalFilesSkipped++;
                }
            }
        }

        return (totalFilesProcessed, totalFilesSkipped);
    }

    private void PrintSummary(int totalFilesProcessed, int totalFilesSkipped)
    {
        if (_dryRun)
        {
            if (totalFilesProcessed > 0)
            {
                Logger.WriteSuccess($"Dry run complete. Found {totalFilesProcessed} files to include.");
            }
            else
            {
                Logger.WriteWarning("Dry run complete. No eligible files were found.");
            }
            return;
        }

        if (totalFilesProcessed > 0)
        {
            int sourceCount = _options.ValidSourceDirectoryPaths.Count;
            Logger.WriteSuccess($"Merging complete. Processed {totalFilesProcessed} files across {sourceCount} source directories.");
        }
        else
        {
            Logger.WriteWarning("No eligible files were found in the provided source directories.");
        }

        if (totalFilesSkipped > 0)
        {
            Logger.WriteInfo($"Skipped {totalFilesSkipped} files (unreadable, excluded, or output file itself).");
        }
    }

    private static void SafelyDeleteFile(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Logger.WriteInfo($"Deleted existing output file: {filePath}");
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Logger.WriteWarning($"Could not delete existing file '{filePath}'. It might be locked or protected. Error: {ex.Message}");
        }
    }
}