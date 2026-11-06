using Coalesce.Configuration;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

[Description("Initialize a '.coalesce' configuration file in the current directory.")]
public sealed class InitCommand : Command<EmptyCommandSettings>
{
    protected override int Execute(CommandContext context, EmptyCommandSettings settings, CancellationToken cancellationToken)
    {
        ConfigurationGenerator.GenerateDefaultConfig();
        return 0;
    }
}
