using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

[Description("List all available configuration presets.")]
public sealed class PresetListCommand : Command<EmptyCommandSettings>
{
    protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        PresetManager.List();
        return 0;
    }
}