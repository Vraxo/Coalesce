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
        });

        return await app.RunAsync(args);
    }
}
