using Spectre.Console.Cli;

namespace Coalesce.Cli;

public sealed class PresetListCommand : Command<CommandSettings>
{
    protected override int Execute(CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
    {
        PresetManager.List();
        return 0;
    }
}