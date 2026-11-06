using Coalesce.Configuration;
using Coalesce.Core;
using Coalesce.Utils;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

[Description("Merge multiple source directories into a single target file using a TOML configuration file.")]
public sealed class MergeCommand : Command<MergeSettings>
{
    protected override int Execute(CommandContext context, MergeSettings settings, CancellationToken cancellationToken)
    {
        Log.Initialize(settings.Quiet, settings.Verbose);

        FileInfo? configFile = !string.IsNullOrEmpty(settings.Config)
            ? new FileInfo(settings.Config)
            : null;

        AppOptions? options = ConfigurationProvider.Build(configFile);

        if (options is null)
        {
            return 1;
        }

        try
        {
            new DirectoryMerger(options, settings.DryRun).Merge();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Error($"An unexpected error occurred: {ex.Message}");
            return 1;
        }
    }
}
