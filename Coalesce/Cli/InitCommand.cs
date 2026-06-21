using Coalesce.Configuration;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace Coalesce.Cli;

[Description("Initialize a new configuration file in the current directory.")]
public sealed class InitCommand : Command<InitSettings>
{
    protected override int Execute(CommandContext context, InitSettings settings, CancellationToken cancellationToken)
    {
        ConfigurationGenerator.GenerateDefaultConfig(settings.Preset);
        return 0;
    }
}