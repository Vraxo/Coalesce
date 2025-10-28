namespace Coalesce.Utils;

public static class Logger
{
    private static bool _quietMode;
    private static bool _verboseMode;
    private static bool _noColor;

    public static void Initialize(bool quiet, bool verbose = false)
    {
        _quietMode = quiet;
        // Verbose is disabled if quiet is enabled. This simplifies checks in other logging methods.
        _verboseMode = verbose && !quiet;
        _noColor = Console.IsOutputRedirected;

        if (verbose && quiet)
        {
            // This warning will still be displayed even in quiet mode, which is desired behavior.
            WriteWarning("Both --quiet and --verbose flags were specified. --quiet takes precedence and verbose logs will be suppressed.");
        }
    }

    public static void WriteError(string message)
    {
        if (_noColor)
        {
            Console.Error.WriteLine($"ERROR: {message}");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine($"ERROR: {message}");
        Console.ResetColor();
    }

    public static void WriteWarning(string message)
    {
        if (_noColor)
        {
            Console.WriteLine($"WARNING: {message}");
            return;
        }

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"WARNING: {message}");
        Console.ResetColor();
    }

    public static void WriteSuccess(string message)
    {
        if (_quietMode)
        {
            return;
        }

        if (_noColor)
        {
            Console.WriteLine(message);
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public static void WriteInfo(string message)
    {
        if (_quietMode)
        {
            return;
        }

        Console.WriteLine(message);
    }

    public static void WriteSuggestion(string message)
    {
        // Not suppressed by quiet mode as it follows an error.
        Console.WriteLine(message);
    }

    public static void WriteVerbose(string message)
    {
        if (!_verboseMode)
        {
            return;
        }

        if (_noColor)
        {
            Console.WriteLine(message);
            return;
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(message);
        Console.ResetColor();
    }
}