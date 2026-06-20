using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

public class InitSettings : CommandSettings
{
    [CommandOption("--preset <PRESET>")]
    [Description("Initializes 'coalesce.yaml' from a built-in or custom preset template.")]
    public string? Preset { get; init; }
}