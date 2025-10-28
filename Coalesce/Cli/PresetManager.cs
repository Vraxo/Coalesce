using Coalesce.Utils;
using System.Reflection;

namespace Coalesce.Cli;

public static class PresetManager
{
    private const string PresetsDirectoryName = "presets";
    private const string PresetsReadmeFileName = "_readme.md";
    private const string PresetsTemplateFileName = "_template.yaml";
    private static readonly string[] BuiltInPresets = ["dotnet", "node"];

    public static void List()
    {
        EnsurePresetsDirectoryExists();

        Logger.WriteInfo("Available presets:");

        var customPresets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string? presetsPath = GetPresetsDirectoryPath();
        if (presetsPath != null && Directory.Exists(presetsPath))
        {
            try
            {
                foreach (var file in Directory.EnumerateFiles(presetsPath, "*.yaml"))
                {
                    string presetName = Path.GetFileNameWithoutExtension(file);
                    if (!presetName.StartsWith('_'))
                    {
                        customPresets.Add(presetName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteWarning($"Could not read custom presets from '{presetsPath}': {ex.Message}");
            }
        }

        var allPresetNames = new SortedSet<string>(customPresets, StringComparer.OrdinalIgnoreCase);
        allPresetNames.UnionWith(BuiltInPresets);

        foreach (var name in allPresetNames)
        {
            bool isCustom = customPresets.Contains(name);
            bool isBuiltIn = BuiltInPresets.Contains(name, StringComparer.OrdinalIgnoreCase);

            if (isCustom && isBuiltIn)
            {
                Logger.WriteInfo($"- {name} (custom, overrides built-in)");
            }
            else if (isCustom)
            {
                Logger.WriteInfo($"- {name} (custom)");
            }
            else // isBuiltIn only
            {
                Logger.WriteInfo($"- {name} (built-in)");
            }
        }
    }

    public static void ShowPath()
    {
        // Ensure the directory exists so the command always returns a valid, accessible path.
        EnsurePresetsDirectoryExists();

        string? presetsPath = GetPresetsDirectoryPath();
        if (presetsPath != null)
        {
            Logger.WriteInfo(presetsPath);
        }
        else
        {
            Logger.WriteError("Could not determine the presets directory path.");
        }
    }

    public static void EnsurePresetsDirectoryExists()
    {
        string? presetsPath = GetPresetsDirectoryPath();
        if (presetsPath == null)
        {
            Logger.WriteWarning("Could not determine application directory. Custom presets will be unavailable.");
            return;
        }

        try
        {
            if (!Directory.Exists(presetsPath))
            {
                Directory.CreateDirectory(presetsPath);
                Logger.WriteVerbose($"Created presets directory: {presetsPath}");
            }

            // Ensure the readme file exists.
            string readmePath = Path.Combine(presetsPath, PresetsReadmeFileName);
            if (!File.Exists(readmePath))
            {
                string readmeContent = GetEmbeddedResource("Coalesce.Resources.Presets._readme.md");
                File.WriteAllText(readmePath, readmeContent);
            }

            // Ensure the template file exists.
            string templatePath = Path.Combine(presetsPath, PresetsTemplateFileName);
            if (!File.Exists(templatePath))
            {
                string templateContent = GetEmbeddedResource("Coalesce.Resources.DefaultConfig.yaml");
                File.WriteAllText(templatePath, templateContent);
            }
        }
        catch (Exception ex)
        {
            Logger.WriteWarning($"Could not create or write to the presets directory '{presetsPath}': {ex.Message}");
        }
    }

    public static string? GetPresetContent(string presetName)
    {
        // 1. Check for a user-defined preset in the file system first.
        string? presetsPath = GetPresetsDirectoryPath();
        if (presetsPath != null)
        {
            string userPresetPath = Path.Combine(presetsPath, $"{presetName}.yaml");
            if (File.Exists(userPresetPath))
            {
                try
                {
                    Logger.WriteVerbose($"Loading user-defined preset from: {userPresetPath}");
                    return File.ReadAllText(userPresetPath);
                }
                catch (Exception ex)
                {
                    Logger.WriteWarning($"Could not read user preset file '{userPresetPath}': {ex.Message}");
                }
            }
        }

        // 2. Fall back to checking for a built-in embedded preset.
        if (BuiltInPresets.Contains(presetName, StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                Logger.WriteVerbose($"Loading built-in preset: {presetName}");
                return GetEmbeddedResource($"Coalesce.Resources.Presets.{presetName}.yaml");
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Could not load built-in preset '{presetName}': {ex.Message}");
                return null;
            }
        }

        return null;
    }

    private static string? GetPresetsDirectoryPath()
    {
        string? appDirectory = GetAppDirectory();
        return appDirectory != null ? Path.Combine(appDirectory, PresetsDirectoryName) : null;
    }

    private static string? GetAppDirectory()
    {
        string? exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath))
        {
            return null;
        }
        return Path.GetDirectoryName(exePath);
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
}