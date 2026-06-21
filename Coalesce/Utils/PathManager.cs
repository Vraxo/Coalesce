using System.Runtime.InteropServices;

namespace Coalesce.Utils;

public static class PathManager
{
    public static void Install()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            InstallOnWindows();
        }
        else
        {
            DisplayInstallInstructionsOnUnix();
        }
    }

    public static void Uninstall()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            UninstallOnWindows();
            return;
        }

        DisplayUninstallInstructionsOnUnix();
    }

    private static string? GetExecutableDirectory()
    {
        string? exePath = Environment.ProcessPath;

        if (string.IsNullOrEmpty(exePath))
        {
            Log.Error("Could not determine the application's path.");
            return null;
        }

        return Path.GetDirectoryName(exePath);
    }

    private static void InstallOnWindows()
    {
        string? appDirectory = GetExecutableDirectory();

        if (appDirectory is null)
        {
            return;
        }

        const EnvironmentVariableTarget scope = EnvironmentVariableTarget.User;
        string currentPath = Environment.GetEnvironmentVariable("PATH", scope) ?? "";

        List<string> paths = [.. currentPath.Split(';', StringSplitOptions.RemoveEmptyEntries)];

        if (paths.Contains(appDirectory, StringComparer.OrdinalIgnoreCase))
        {
            Log.Info($"Coalesce is already in your user PATH: {appDirectory}");
            return;
        }

        try
        {
            paths.Add(appDirectory);
            string newPath = string.Join(';', paths);
            Environment.SetEnvironmentVariable("PATH", newPath, scope);

            Log.Success("Successfully added Coalesce to user PATH.");
            Log.Info("Please restart your terminal for the changes to take effect.");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to update PATH: {ex.Message}");
            Log.Suggestion("You may need to run this command with administrator privileges or update the PATH manually.");
        }
    }

    private static void UninstallOnWindows()
    {
        string? appDirectory = GetExecutableDirectory();

        if (appDirectory is null)
        {
            return;
        }

        const EnvironmentVariableTarget scope = EnvironmentVariableTarget.User;
        string? currentPath = Environment.GetEnvironmentVariable("PATH", scope);

        if (string.IsNullOrEmpty(currentPath))
        {
            Log.Info("User PATH variable is empty. Nothing to do.");
            return;
        }

        List<string> paths = [.. currentPath.Split(';')];
        int initialCount = paths.Count;

        _ = paths.RemoveAll(p => string.Equals(p, appDirectory, StringComparison.OrdinalIgnoreCase));

        if (paths.Count == initialCount)
        {
            Log.Info("Coalesce was not found in your user PATH.");
            return;
        }

        try
        {
            string newPath = string.Join(";", paths);
            Environment.SetEnvironmentVariable("PATH", newPath, scope);

            Log.Success("Successfully removed Coalesce from user PATH.");
            Log.Info("Please restart your terminal for the changes to take effect.");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to update PATH: {ex.Message}");
        }
    }

    private static void DisplayInstallInstructionsOnUnix()
    {
        string? exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath))
        {
            Log.Error("Could not determine the application's path.");
            return;
        }

        string? appDirectory = Path.GetDirectoryName(exePath);
        string exeName = Path.GetFileName(exePath);

        if (string.IsNullOrEmpty(appDirectory) || string.IsNullOrEmpty(exeName))
        {
            Log.Error("Could not determine the application's path details.");
            return;
        }

        Log.Info("Automatic PATH installation is not supported on this OS.");
        Log.Info("Please add the application directory to your PATH manually.");
        Log.Suggestion("\nOption 1: Add to your shell profile (e.g., ~/.bashrc, ~/.zshrc)");
        Log.Info($"  export PATH=\"$PATH:{appDirectory}\"");
        Log.Suggestion("\nOption 2: Create a symbolic link to a directory in your PATH");
        Log.Info($"  sudo ln -s \"{exePath}\" /usr/local/bin/{exeName}");
        Log.Info("\nAfter running one of the commands, restart your terminal.");
    }

    private static void DisplayUninstallInstructionsOnUnix()
    {
        string? exePath = Environment.ProcessPath;
        string exeName = !string.IsNullOrEmpty(exePath)
            ? Path.GetFileName(exePath)
            : "coalesce";

        Log.Info("To uninstall, please reverse the steps you took to install.");
        Log.Suggestion("1. Remove the 'export PATH...' line from your shell profile.");
        Log.Suggestion($"2. Or, delete the symbolic link: sudo rm /usr/local/bin/{exeName}");
    }
}