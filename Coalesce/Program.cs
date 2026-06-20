using Coalesce.Cli;
using Spectre.Console.Cli;

namespace Coalesce;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        CommandApp<MergeCommand> app = new();

        app.Configure(config =>
        {
            config.SetApplicationName("coalesce");

            config.AddCommand<InitCommand>("init");

            config.AddBranch("preset", preset =>
            {
                preset.SetDescription("Manage configuration presets.");
                preset.AddCommand<PresetListCommand>("list");
                preset.AddCommand<PresetPathCommand>("path");
            });

            config.AddCommand<InstallPathCommand>("install-path");
            config.AddCommand<UninstallPathCommand>("uninstall-path");
        });

        return await app.RunAsync(args);
    }
}