using Coalesce.Utils;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

[Description("Remove the coalesce executable from the system PATH.")]
public sealed class UninstallCommand : Command<EmptyCommandSettings>
{
    protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        PathManager.Uninstall();
        return 0;
    }
}