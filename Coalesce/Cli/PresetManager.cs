using Coalesce.Utils;

namespace Coalesce.Cli;

public static class PresetManager
{
    private const string PresetsDirectoryName = "presets";
    private const string PresetsReadmeFileName = "_readme.md";
    private const string PresetsTemplateFileName = "_template.toml";

    private static readonly string[] BuiltInPresets = ["dotnet", "node"];

    public static void List()
    {
        EnsurePresetsDirectoryExists();

        Log.Info("Available presets:");

        HashSet<string> customPresets = new(StringComparer.OrdinalIgnoreCase);
        string? presetsPath = GetPresetsDirectoryPath();

        if (presetsPath is not null && Directory.Exists(presetsPath))
        {
            try
            {
                foreach (string file in Directory.EnumerateFiles(presetsPath, "*.toml"))
                {
                    string presetName = Path.GetFileNameWithoutExtension(file);
                    if (presetName.StartsWith('_'))
                    {
                        continue;
                    }
                    customPresets.Add(presetName);
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Could not read custom presets from '{presetsPath}': {ex.Message}");
            }
        }

        SortedSet<string> allPresetNames = new(customPresets, StringComparer.OrdinalIgnoreCase);
        allPresetNames.UnionWith(BuiltInPresets);

        foreach (string name in allPresetNames)
        {
            bool isCustom = customPresets.Contains(name);
            bool isBuiltIn = BuiltInPresets.Contains(name, StringComparer.OrdinalIgnoreCase);

            string status;
            if (isCustom && isBuiltIn)
            {
                status = " (custom, overrides built-in)";
            }
            else
            {
                if (isCustom)
                {
                    status = " (custom)";
                }
                else
                {
                    status = " (built-in)";
                }
            }

            Log.Info($"- {name}{status}");
        }
    }

    public static void ShowPath()
    {
        EnsurePresetsDirectoryExists();

        string? presetsPath = GetPresetsDirectoryPath();
        if (presetsPath is null)
        {
            Log.Error("Could not determine the presets directory path.");
            return;
        }

        Log.Info(presetsPath);
    }

    public static void EnsurePresetsDirectoryExists()
    {
        string? presetsPath = GetPresetsDirectoryPath();
        if (presetsPath is null)
        {
            Log.Warning(
                "Could not determine application directory. " +
                "Custom presets will be unavailable.");
            return;
        }

        try
        {
            if (!Directory.Exists(presetsPath))
            {
                Directory.CreateDirectory(presetsPath);
                Log.Verbose($"Created presets directory: {presetsPath}");
            }

            string readmePath = Path.Combine(presetsPath, PresetsReadmeFileName);
            if (!File.Exists(readmePath))
            {
                string readmeContent = ResourceLoader.Get("Presets._readme.md");
                File.WriteAllText(readmePath, readmeContent);
            }

            string templatePath = Path.Combine(presetsPath, PresetsTemplateFileName);
            if (!File.Exists(templatePath))
            {
                string templateContent = ResourceLoader.Get("coalesce.toml");
                File.WriteAllText(templatePath, templateContent);
            }
        }
        catch (Exception ex)
        {
            Log.Warning($"Could not create or write to the presets directory '{presetsPath}': {ex.Message}");
        }
    }

    public static string? GetPresetContent(string presetName)
    {
        string? presetsPath = GetPresetsDirectoryPath();
        if (presetsPath is not null)
        {
            string userPresetPath = Path.Combine(presetsPath, $"{presetName}.toml");
            if (File.Exists(userPresetPath))
            {
                try
                {
                    Log.Verbose($"Loading user-defined preset from: {userPresetPath}");
                    return File.ReadAllText(userPresetPath);
                }
                catch (Exception ex)
                {
                    Log.Warning($"Could not read user preset file '{userPresetPath}': {ex.Message}");
                }
            }
        }

        if (!BuiltInPresets.Contains(presetName, StringComparer.OrdinalIgnoreCase))
        {
            return null;
        }

        try
        {
            Log.Verbose($"Loading built-in preset: {presetName}");
            return ResourceLoader.Get($"Presets.{presetName.ToLower()}.toml");
        }
        catch (Exception ex)
        {
            Log.Error($"Could not load built-in preset '{presetName}': {ex.Message}");
            return null;
        }
    }

    private static string? GetPresetsDirectoryPath()
    {
        string? exePath = Environment.ProcessPath;

        if (string.IsNullOrEmpty(exePath))
        {
            return null;
        }

        return Path.Combine(Path.GetDirectoryName(exePath)!, PresetsDirectoryName);
    }
}