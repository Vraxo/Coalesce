using Coalesce.Configuration;
using Spectre.Console.Cli;

namespace Coalesce.Cli;

public sealed class InitCommand : Command<InitSettings>
{
    protected override int Execute(CommandContext context, InitSettings settings, CancellationToken cancellationToken)
    {
        ConfigurationGenerator.GenerateDefaultConfig(settings.Preset);
        return 0;
    }
}