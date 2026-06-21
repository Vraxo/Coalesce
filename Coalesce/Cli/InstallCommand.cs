using Coalesce.Utils;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

[Description("Install the coalesce executable to the system PATH.")]
public sealed class InstallCommand : Command<EmptyCommandSettings>
{
    protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        Log.Info("Installing path...");
        PathManager.Install();
        return 0;
    }
}