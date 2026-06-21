using Coalesce.Cli;
using Coalesce.Utils;
using Tomlyn;

namespace Coalesce.Configuration;

public static class ConfigurationProvider
{
    public static AppOptions? Build(MergeSettings settings, FileInfo? configFileOption)
    {
        if (configFileOption is not null)
        {
            return BuildFromConfigFile(configFileOption);
        }

        bool hasCliArgs = !string.IsNullOrEmpty(settings.OutputFile) || settings.SourceDirs.Length > 0;

        if (hasCliArgs)
        {
            return BuildFromCommandLine(settings);
        }

        string defaultConfigPath = Path.Combine(Environment.CurrentDirectory, "coalesce.toml");
        if (File.Exists(defaultConfigPath))
        {
            return BuildFromConfigFile(new(defaultConfigPath));
        }

        return BuildFromCommandLine(settings);
    }

    private static AppOptions? BuildFromConfigFile(FileInfo resolvedConfig)
    {
        if (!resolvedConfig.Exists)
        {
            Log.Warning($"Configuration file not found: {resolvedConfig.FullName}");
            return null;
        }

        Log.Info($"Loading configuration from: {resolvedConfig.FullName}");
        AppOptions? options = LoadOptionsFromTomlFile(resolvedConfig.FullName);
        return options is not null && Validate(options) ? options : null;
    }

    private static AppOptions? BuildFromCommandLine(MergeSettings settings)
    {
        Log.Verbose("No configuration file loaded. Operating in ad-hoc CLI mode.");
        AppOptions cliOptions = new()
        {
            OutputFilePath = settings.OutputFile ?? string.Empty,
            SourceDirectoryPaths = [.. settings.SourceDirs],
            ExcludeDirectoryNames = [.. settings.ExcludeDir],
            ExcludeFileNames = [.. settings.ExcludeFile],
            IncludeExtensions = [.. settings.IncludeExt],
            ExcludeExtensions = [.. settings.ExcludeExt],
            PathOnlyExtensions = [.. settings.PathOnlyExt]
        };

        return Validate(cliOptions) ? cliOptions : null;
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

        if (string.IsNullOrEmpty(options.OutputFilePath))
        {
            Log.Error("Missing output file path. Please specify it as an argument or in your config file.");
            hasError = true;
        }

        if (options.SourceDirectoryPaths.Count == 0)
        {
            Log.Error("Missing source directories. Please provide at least one source directory as an argument or in your config file.");
            hasError = true;
        }

        if (hasError)
        {
            Log.Error("Example: coalesce coalesce.md ./src");
            Log.Suggestion("\nRun 'coalesce --help' for a list of commands and options.");
            return false;
        }

        return true;
    }
}