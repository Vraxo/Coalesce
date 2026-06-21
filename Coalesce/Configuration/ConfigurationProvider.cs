using Coalesce.Utils;
using Tomlyn;

namespace Coalesce.Configuration;

public static class ConfigurationProvider
{
    public static AppOptions? Build(string? outputArg, List<string> sourceArgs, List<string> excludeDirOptions, List<string> excludeFileOptions, List<string> includeExtOptions, List<string> excludeExtOptions, List<string> pathOnlyExtOptions, FileInfo? configFileOption)
    {
        AppOptions? options = LoadOptions(configFileOption);
        if (options is null)
        {
            return null;
        }

        ApplyCommandLineOverrides(
            options,
            outputArg,
            sourceArgs,
            excludeDirOptions,
            excludeFileOptions,
            includeExtOptions,
            excludeExtOptions,
            pathOnlyExtOptions);

        if (!Validate(options))
        {
            return null;
        }

        return options;
    }

    private static void ApplyCommandLineOverrides(AppOptions options, string? outputArg, List<string> sourceArgs, List<string> excludeDirOptions, List<string> excludeFileOptions, List<string> includeExtOptions, List<string> excludeExtOptions, List<string> pathOnlyExtOptions)
    {
        if (!string.IsNullOrEmpty(outputArg))
        {
            Log.Verbose($"Overriding 'OutputFilePath' with CLI argument: '{outputArg}'");
            options.OutputFilePath = outputArg;
        }

        if (sourceArgs.Count > 0)
        {
            Log.Verbose("Overriding 'SourceDirectoryPaths' with CLI arguments.");
            options.SourceDirectoryPaths = sourceArgs;
        }

        if (includeExtOptions.Count > 0)
        {
            Log.Verbose("Overriding 'IncludeExtensions' with CLI arguments.");
            options.IncludeExtensions = includeExtOptions;
        }

        if (excludeDirOptions.Count > 0)
        {
            Log.Verbose("Overriding 'ExcludeDirectoryNames' with CLI arguments.");
            options.ExcludeDirectoryNames = excludeDirOptions;
        }

        if (excludeFileOptions.Count > 0)
        {
            Log.Verbose("Overriding 'ExcludeFileNames' with CLI arguments.");
            options.ExcludeFileNames = excludeFileOptions;
        }

        if (excludeExtOptions.Count > 0)
        {
            Log.Verbose("Overriding 'ExcludeExtensions' with CLI arguments.");
            options.ExcludeExtensions = excludeExtOptions;
        }

        if (pathOnlyExtOptions.Count > 0)
        {
            Log.Verbose("Overriding 'PathOnlyExtensions' with CLI arguments.");
            options.PathOnlyExtensions = pathOnlyExtOptions;
        }
    }

    private static bool Validate(AppOptions options)
    {
        bool hasError = false;

        if (string.IsNullOrEmpty(options.OutputFilePath))
        {
            Log.Error(
                "Missing output file path. " +
                "Please specify it as an argument or in your config file.");

            hasError = true;
        }

        if (options.SourceDirectoryPaths.Count == 0)
        {
            Log.Error("" +
                "Missing source directories. " +
                "Please provide at least one source directory as an argument or in your config file.");

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

    private static AppOptions? LoadOptions(FileInfo? configFileOption)
    {
        string? resolvedConfigPath = null;

        if (configFileOption is null)
        {
            string defaultConfigPath = Path.Combine(Environment.CurrentDirectory, "coalesce.toml");

            if (File.Exists(defaultConfigPath))
            {
                resolvedConfigPath = defaultConfigPath;
            }
        }
        else
        {
            if (!configFileOption.Exists)
            {
                Log.Warning($"Configuration file specified via --config not found: {configFileOption.FullName}");
                return LoadDefaultOptionsFromEmbedded();
            }

            resolvedConfigPath = configFileOption.FullName;
        }

        if (resolvedConfigPath is not null)
        {
            Log.Info($"Loading configuration from: {resolvedConfigPath}");
            return LoadOptionsFromTomlFile(resolvedConfigPath);
        }

        Log.Verbose(
            "No 'coalesce.toml' found. " +
            "Using built-in default configuration.");

        return LoadDefaultOptionsFromEmbedded();
    }

    private static AppOptions? LoadOptionsFromTomlFile(string configPath)
    {
        try
        {
            string tomlContent = File.ReadAllText(configPath);
            return DeserializeToml(tomlContent);
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading or parsing config file '{configPath}': {ex.Message}");
            return null;
        }
    }

    private static AppOptions? LoadDefaultOptionsFromEmbedded()
    {
        try
        {
            string tomlContent = ResourceLoader.Get("coalesce.toml");
            return DeserializeToml(tomlContent);
        }
        catch (Exception ex)
        {
            Log.Error($"FATAL: Could not load the built-in default configuration. {ex.Message}");
            return null;
        }
    }

    private static AppOptions? DeserializeToml(string tomlContent)
    {
        if (string.IsNullOrWhiteSpace(tomlContent))
        {
            return new();
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
}