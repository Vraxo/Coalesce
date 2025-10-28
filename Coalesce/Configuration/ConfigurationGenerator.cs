using Coalesce.Cli;
using Coalesce.Utils;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Coalesce.Configuration;

public static class ConfigurationGenerator
{
    private const string DefaultConfigFileName = "coalesce.yaml";
    private const string BatchFileName = "coalesce-run.bat";
    private const string ShellFileName = "coalesce-run.sh";

    public static void GenerateDefaultConfig(string? presetName)
    {
        string? yamlContent;
        string sourceName;

        // The PresetManager now handles all the logic of finding/loading presets
        // and setting up the presets directory on first use.
        PresetManager.EnsurePresetsDirectoryExists();

        if (string.IsNullOrWhiteSpace(presetName))
        {
            yamlContent = GetEmbeddedResource("Coalesce.Resources.DefaultConfig.yaml");
            sourceName = "default configuration";
        }
        else
        {
            yamlContent = PresetManager.GetPresetContent(presetName);
            sourceName = $"preset '{presetName}'";
        }

        if (yamlContent == null)
        {
            Logger.WriteError($"Preset '{presetName}' not found.");
            Logger.WriteSuggestion("Run 'coalesce preset list' to see all available presets.");
        }
        else
        {
            TryGeneratingYamlConfigFile(yamlContent, sourceName);

            // Generate OS-specific run script.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                TryGeneratingBatchFile();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                TryGeneratingShellScriptFile();
            }
        }

        // The prompt is removed to create a smoother, scriptable CLI experience.
        // The generated .bat script already includes a 'pause' command for the double-click use case.
    }

    private static void TryGeneratingYamlConfigFile(string yamlContent, string sourceName)
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
            File.WriteAllText(filePath, yamlContent);
            Logger.WriteSuccess($"Created configuration file from {sourceName}: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.WriteError($"Failed to generate '{DefaultConfigFileName}': {ex.Message}");
        }
    }

    private static string GetEmbeddedResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"Could not find embedded resource '{resourceName}'. Make sure the file's 'Build Action' is 'Embedded Resource' and the name is correct.", resourceName);
        }

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static void TryGeneratingBatchFile()
    {
        string filePath = Path.Combine(Environment.CurrentDirectory, BatchFileName);

        if (File.Exists(filePath))
        {
            Logger.WriteWarning($"Batch file '{BatchFileName}' already exists. Generation skipped.");
            Logger.WriteInfo(filePath);
            return;
        }

        try
        {
            string batchContent = GetEmbeddedResource("Coalesce.Resources.coalesce-run.bat").Replace("\r\n", "\n").Replace("\n", "\r\n");
            File.WriteAllText(filePath, batchContent);
            Logger.WriteSuccess($"Created Windows run script: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.WriteError($"Failed to generate '{BatchFileName}': {ex.Message}");
        }
    }

    private static void TryGeneratingShellScriptFile()
    {
        string filePath = Path.Combine(Environment.CurrentDirectory, ShellFileName);

        if (File.Exists(filePath))
        {
            Logger.WriteWarning($"Shell script '{ShellFileName}' already exists. Generation skipped.");
            Logger.WriteInfo(filePath);
            return;
        }

        try
        {
            string shellContent = GetEmbeddedResource("Coalesce.Resources.coalesce-run.sh").Replace("\r\n", "\n");
            File.WriteAllText(filePath, shellContent);
            Logger.WriteSuccess($"Created Linux/macOS run script: {filePath}");
            Logger.WriteInfo($"-> To make it executable, run: chmod +x {ShellFileName}");
        }
        catch (Exception ex)
        {
            Logger.WriteError($"Failed to generate '{ShellFileName}': {ex.Message}");
        }
    }
}