using Coalesce.Utils;
using Spectre.Console.Cli;

namespace Coalesce.Cli;

public sealed class UninstallPathCommand : Command<CommandSettings>
{
    protected override int Execute(CommandContext context, CommandSettings settings, CancellationToken cancellationToken)
    {
        PathManager.Uninstall();
        return 0;
    }
}