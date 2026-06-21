using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

[Description("Show the directory path where presets are stored.")]
public sealed class PresetPathCommand : Command<EmptyCommandSettings>
{
    protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        PresetManager.ShowPath();
        return 0;
    }
}