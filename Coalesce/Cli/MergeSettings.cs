using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

public sealed class MergeSettings : CommandSettings
{
    [CommandOption("--config <CONFIG>")]
    [Description("Path to a configuration file. Defaults to '.coalesce' in the current directory if present.")]
    public string? Config { get; init; }

    [CommandOption("--dry-run")]
    [Description("Simulates a merge, printing which files would be included without writing the output file.")]
    public bool DryRun { get; init; }

    [CommandOption("--quiet")]
    [Description("Suppresses all informational output. Only warnings and errors will be displayed.")]
    public bool Quiet { get; init; }

    [CommandOption("--verbose")]
    [Description("Enables detailed output, showing why files are skipped and how configuration is applied.")]
    public bool Verbose { get; init; }
}
