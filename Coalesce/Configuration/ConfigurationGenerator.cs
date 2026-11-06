using Coalesce.Utils;

namespace Coalesce.Configuration;

public static class ConfigurationGenerator
{
    public static void GenerateDefaultConfig()
    {
        string filePath = Path.Combine(Environment.CurrentDirectory, ConfigurationProvider.DefaultConfigFileName);

        if (File.Exists(filePath))
        {
            Log.Warning($"Configuration file '{ConfigurationProvider.DefaultConfigFileName}' already exists. Generation skipped.");
            Log.Info(filePath);
            return;
        }

        try
        {
            string content = ResourceLoader.Get("default.coalesce");
            File.WriteAllText(filePath, content);
            Log.Success($"Created configuration file: {filePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to generate '{ConfigurationProvider.DefaultConfigFileName}': {ex.Message}");
        }
    }
}
