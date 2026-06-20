using Coalesce.Configuration;
using Coalesce.Core;
using Coalesce.Utils;
using Spectre.Console.Cli;

namespace Coalesce.Cli;

public class MergeCommand : Command<MergeSettings>
{
    protected override int Execute(CommandContext context, MergeSettings settings, CancellationToken cancellationToken)
    {
        Logger.Initialize(settings.Quiet, settings.Verbose);

        ConfigurationProvider configProvider = new();

        FileInfo? configFile = !string.IsNullOrEmpty(settings.Config) ? new FileInfo(settings.Config) : null;

        AppOptions? options = ConfigurationProvider.Build(
            settings.OutputFile,
            [.. settings.SourceDirs],
            [.. settings.ExcludeDir],
            [.. settings.ExcludeFile],
            [.. settings.IncludeExt],
            [.. settings.ExcludeExt],
            [.. settings.PathOnlyExt],
            configFile);

        if (options is not null)
        {
            try
            {
                new DirectoryMerger(options, settings.DryRun).Merge();
                return 0;
            }
            catch (Exception ex)
            {
                Logger.WriteError($"An unexpected error occurred: {ex.Message}");
                return 1;
            }
        }

        return 1;
    }
}