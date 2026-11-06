using Coalesce.Utils;
using Tomlyn;

namespace Coalesce.Configuration;

public static class ConfigurationProvider
{
    private const string DefaultConfigFileName = "coalesce.toml";

    public static AppOptions? Build(FileInfo? configFileOption = null)
    {
        FileInfo resolvedConfig = configFileOption
            ?? new FileInfo(Path.Combine(Environment.CurrentDirectory, DefaultConfigFileName));

        if (!resolvedConfig.Exists)
        {
            Log.Error($"Configuration file not found: {resolvedConfig.FullName}");

            if (configFileOption is null)
            {
                Log.Suggestion("Run 'coalesce init' to create a default configuration file, or specify one with '--config <CONFIG>'.");
            }

            return null;
        }

        Log.Info($"Loading configuration from: {resolvedConfig.FullName}");
        AppOptions? options = LoadOptionsFromTomlFile(resolvedConfig.FullName);

        if (options is null || !Validate(options))
        {
            return null;
        }

        options.ConfigFilePath = resolvedConfig.FullName;
        return options;
    }

    private static AppOptions? LoadOptionsFromTomlFile(string configPath)
    {
        try
        {
            return DeserializeToml(File.ReadAllText(configPath));
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading or parsing config file '{configPath}': {ex.Message}");
            return null;
        }
    }

    private static AppOptions? DeserializeToml(string tomlContent)
    {
        if (string.IsNullOrWhiteSpace(tomlContent))
        {
            return new AppOptions();
        }

        try
        {
            return TomlSerializer.Deserialize<AppOptions>(tomlContent, CoalesceTomlContext.Default.AppOptions);
        }
        catch (Exception ex)
        {
            throw new FormatException($"The configuration file is malformed. {ex.Message}", ex);
        }
    }

    private static bool Validate(AppOptions options)
    {
        bool hasError = false;

        if (string.IsNullOrWhiteSpace(options.OutputFilePath))
        {
            Log.Error("Missing 'output' in the configuration file.");
            hasError = true;
        }

        if (options.SourceDirectoryPaths.Count == 0)
        {
            Log.Error("Missing 'sources'. Please provide at least one source directory in the configuration file.");
            hasError = true;
        }

        return !hasError;
    }
}
