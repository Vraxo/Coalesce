namespace Coalesce.Utils;

public static class Log
{
    private static bool _quiet;
    private static bool _verbose;
    private static bool _noColor;

    public static void Initialize(bool quiet, bool verbose = false)
    {
        _quiet = quiet;
        _verbose = verbose && !quiet;
        _noColor = Console.IsOutputRedirected;

        if (!verbose || !quiet)
        {
            return;
        }

        Warning(
            "Both --quiet and --verbose flags were specified. " +
            "--quiet takes precedence and verbose logs will be suppressed.");
    }

    public static void Error(string message)
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

    public static void Warning(string message)
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

    public static void Success(string message)
    {
        if (_quiet)
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

    public static void Info(string message)
    {
        if (_quiet)
        {
            return;
        }

        Console.WriteLine(message);
    }

    public static void Suggestion(string message)
    {
        // Not suppressed by quiet mode as it follows an error.
        Console.WriteLine(message);
    }

    public static void Verbose(string message)
    {
        if (!_verbose)
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