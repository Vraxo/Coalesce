using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

public sealed class InitSettings : CommandSettings
{
    [CommandOption("--preset <PRESET>")]
    [Description("Initializes 'coalesce.toml' from a built-in preset template (e.g., 'dotnet', 'node').")]
    public string? Preset { get; init; }
}