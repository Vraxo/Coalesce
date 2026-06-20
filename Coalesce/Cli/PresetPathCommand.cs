using Spectre.Console.Cli;

namespace Coalesce.Cli;

public class PresetPathCommand : Command<CommandSettings>
{
    protected override int Execute(CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
    {
        PresetManager.ShowPath();
        return 0;
    }
}