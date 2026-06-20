using Coalesce.Utils;
using Spectre.Console.Cli;

namespace Coalesce.Cli;

public class InstallPathCommand : Command<CommandSettings>
{
    protected override int Execute(CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
    {
        PathManager.Install();
        return 0;
    }
}