using Coalesce.Cli;
using Coalesce.Utils;
using System.Runtime.InteropServices;

namespace Coalesce.Configuration;

public static class ConfigurationGenerator
{
    private const string DefaultConfigFileName = "coalesce.toml";
    private const string BatchFileName = "coalesce-run.bat";
    private const string ShellFileName = "coalesce-run.sh";

    public static void GenerateDefaultConfig(string? presetName)
    {
        string? tomlContent;
        string sourceName;

        PresetManager.EnsurePresetsDirectoryExists();

        if (string.IsNullOrWhiteSpace(presetName))
        {
            tomlContent = ResourceLoader.Get("coalesce.toml");
            sourceName = "default configuration";
        }
        else
        {
            tomlContent = PresetManager.GetPresetContent(presetName);
            sourceName = $"preset '{presetName}'";
        }

        if (tomlContent is null)
        {
            Logger.WriteError($"Preset '{presetName}' not found.");
            Logger.WriteSuggestion("Run 'coalesce preset list' to see all available presets.");
        }
        else
        {
            TryGeneratingTomlConfigFile(tomlContent, sourceName);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                TryGeneratingBatchFile();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                TryGeneratingShellScriptFile();
            }
        }
    }

    private static void TryGeneratingTomlConfigFile(string tomlContent, string sourceName)
    {
        string filePath = Path.Combine(Environment.CurrentDirectory, DefaultConfigFileName);

        if (File.Exists(filePath))
        {
            Logger.WriteWarning($"Configuration file '{DefaultConfigFileName}' already exists. Generation skipped.");
            Logger.WriteInfo(filePath);
            return;
        }

        try
        {
            File.WriteAllText(filePath, tomlContent);
            Logger.WriteSuccess($"Created configuration file from {sourceName}: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.WriteError($"Failed to generate '{DefaultConfigFileName}': {ex.Message}");
        }
    }

    private static void TryGeneratingBatchFile()
    {
        string content = ResourceLoader.Get("coalesce-run.bat").Replace("\r\n", "\n").Replace("\n", "\r\n");
        TryGeneratingScriptFile(BatchFileName, "Windows run script", content);
    }

    private static void TryGeneratingShellScriptFile()
    {
        string content = ResourceLoader.Get("coalesce-run.sh").Replace("\r\n", "\n");
        string postGenerationInfo = $"-> To make it executable, run: chmod +x {ShellFileName}";
        TryGeneratingScriptFile(ShellFileName, "Linux/macOS run script", content, postGenerationInfo);
    }

    private static void TryGeneratingScriptFile(string fileName, string scriptType, string content, string? postGenerationInfo = null)
    {
        string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

        if (File.Exists(filePath))
        {
            Logger.WriteWarning($"Script file '{fileName}' already exists. Generation skipped.");
            Logger.WriteInfo(filePath);
            return;
        }

        try
        {
            File.WriteAllText(filePath, content);
            Logger.WriteSuccess($"Created {scriptType}: {filePath}");
            if (postGenerationInfo is not null)
            {
                Logger.WriteInfo(postGenerationInfo);
            }
        }
        catch (Exception ex)
        {
            Logger.WriteError($"Failed to generate '{fileName}': {ex.Message}");
        }
    }
}