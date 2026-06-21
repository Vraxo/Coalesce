using Spectre.Console.Cli;

namespace Coalesce.Cli;

public sealed class PresetPathCommand : Command<CommandSettings>
{
    protected override int Execute(CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
    {
        PresetManager.ShowPath();
        return 0;
    }
}