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
            config.AddCommand<InstallCommand>("install");
            config.AddCommand<UninstallCommand>("uninstall");
        });

        return await app.RunAsync(args);
    }
}