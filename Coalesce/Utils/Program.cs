using Coalesce.Cli;
using System.CommandLine;

namespace Coalesce.Utils;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        RootCommand rootCommand = CommandLineBuilder.Build();
        return await rootCommand.InvokeAsync(args);
    }
}