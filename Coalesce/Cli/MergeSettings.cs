using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

public class MergeSettings : CommandSettings
{
    [CommandArgument(0, "[output-file]")]
    [Description("Path for the merged output file. Required if not in config.")]
    public string? OutputFile { get; init; }

    [CommandArgument(1, "[source-dirs]")]
    [Description("Source directories to scan. Required if not in config.")]
    public string[] SourceDirs { get; init; } = [];

    [CommandOption("--exclude-dir <EXCLUDE_DIR>")]
    [Description("Excludes a directory by name (e.g., 'node_modules'). Can be used multiple times.")]
    public string[] ExcludeDir { get; init; } = [];

    [CommandOption("--exclude-file <EXCLUDE_FILE>")]
    [Description("Excludes a file by name (e.g., 'package-lock.json'). Can be used multiple times.")]
    public string[] ExcludeFile { get; init; } = [];

    [CommandOption("--include-ext <INCLUDE_EXT>")]
    [Description("Includes a file extension (e.g., '.md'). Replaces the config list. Can be used multiple times.")]
    public string[] IncludeExt { get; init; } = [];

    [CommandOption("--exclude-ext <EXCLUDE_EXT>")]
    [Description("Excludes a file extension (e.g., '.log'). Can be used multiple times.")]
    public string[] ExcludeExt { get; init; } = [];

    [CommandOption("--path-only-ext <PATH_ONLY_EXT>")]
    [Description("Includes a file by path only, without its content (e.g., '.dll'). Can be used multiple times.")]
    public string[] PathOnlyExt { get; init; } = [];

    [CommandOption("--config <CONFIG>")]
    [Description("Path to a YAML configuration file. If not provided, 'coalesce.yaml' in the current directory is used if it exists.")]
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