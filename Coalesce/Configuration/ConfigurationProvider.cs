using Coalesce.Utils;
using System.Reflection;
using Tomlyn;

namespace Coalesce.Configuration;

public class ConfigurationProvider
{
    public static AppOptions? Build(string? outputArg, List<string> sourceArgs, List<string> excludeDirOptions, List<string> excludeFileOptions, List<string> includeExtOptions, List<string> excludeExtOptions, List<string> pathOnlyExtOptions, FileInfo? configFileOption)
    {
        AppOptions? options = LoadOptions(configFileOption);
        if (options == null)
        {
            return null;
        }

        ApplyCommandLineOverrides(options, outputArg, sourceArgs, excludeDirOptions, excludeFileOptions, includeExtOptions, excludeExtOptions, pathOnlyExtOptions);

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
            Logger.WriteVerbose($"Overriding 'outputFilePath' with CLI argument: '{outputArg}'");
            options.OutputFilePath = outputArg!;
        }

        if (sourceArgs.Count > 0)
        {
            Logger.WriteVerbose("Overriding 'sourceDirectoryPaths' with CLI arguments.");
            options.SourceDirectoryPaths = sourceArgs;
        }

        if (includeExtOptions.Count > 0)
        {
            Logger.WriteVerbose("Replacing 'includeExtensions' with CLI arguments.");
            options.IncludeExtensions = includeExtOptions;
        }

        AddCliOptionsToList(options.ExcludeDirectoryNames, excludeDirOptions, "excludeDirectoryNames");
        AddCliOptionsToList(options.ExcludeFileNames, excludeFileOptions, "excludeFileNames");
        AddCliOptionsToList(options.ExcludeExtensions, excludeExtOptions, "excludeExtensions");
        AddCliOptionsToList(options.PathOnlyExtensions, pathOnlyExtOptions, "pathOnlyExtensions");
    }

    private static void AddCliOptionsToList(List<string> targetList, List<string> cliOptions, string optionName)
    {
        if (cliOptions.Count == 0)
        {
            return;
        }

        Logger.WriteVerbose($"Adding to '{optionName}' from CLI arguments.");
        foreach (string option in cliOptions)
        {
            if (!targetList.Contains(option, StringComparer.OrdinalIgnoreCase))
            {
                targetList.Add(option);
            }
        }
    }

    private static bool Validate(AppOptions options)
    {
        bool hasError = false;

        if (string.IsNullOrEmpty(options.OutputFilePath))
        {
            Logger.WriteError("Missing output file path. Please specify it as an argument or in your config file.");
            hasError = true;
        }

        if (options.SourceDirectoryPaths.Count == 0)
        {
            Logger.WriteError("Missing source directories. Please provide at least one source directory as an argument or in your config file.");
            hasError = true;
        }

        if (hasError)
        {
            Logger.WriteError("Example: coalesce coalesce.md ./src");
            Logger.WriteSuggestion("\nRun 'coalesce --help' for a list of commands and options.");
            return false;
        }

        return true;
    }

    private static AppOptions? LoadOptions(FileInfo? configFileOption)
    {
        string? resolvedConfigPath = null;

        if (configFileOption != null)
        {
            if (!configFileOption.Exists)
            {
                Logger.WriteWarning($"Configuration file specified via --config not found: {configFileOption.FullName}");
                return LoadDefaultOptionsFromEmbedded();
            }

            resolvedConfigPath = configFileOption.FullName;
        }
        else
        {
            string defaultConfigPath = Path.Combine(Environment.CurrentDirectory, "coalesce.toml");

            if (File.Exists(defaultConfigPath))
            {
                resolvedConfigPath = defaultConfigPath;
            }
        }

        if (resolvedConfigPath != null)
        {
            Logger.WriteInfo($"Loading configuration from: {resolvedConfigPath}");
            return LoadOptionsFromTomlFile(resolvedConfigPath);
        }

        Logger.WriteVerbose("No 'coalesce.toml' found. Using built-in default configuration.");
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
            Logger.WriteError($"Error loading or parsing config file '{configPath}': {ex.Message}");
            return null;
        }
    }

    private static AppOptions? LoadDefaultOptionsFromEmbedded()
    {
        try
        {
            string tomlContent = GetEmbeddedResource("Coalesce.Resources.DefaultConfig.toml");
            return DeserializeToml(tomlContent);
        }
        catch (Exception ex)
        {
            Logger.WriteError($"FATAL: Could not load the built-in default configuration. {ex.Message}");
            return null;
        }
    }

    private static string GetEmbeddedResource(string resourceName)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        using Stream? stream = assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            throw new FileNotFoundException($"Could not find embedded resource '{resourceName}'.", resourceName);
        }

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    private static AppOptions? DeserializeToml(string tomlContent)
    {
        if (string.IsNullOrWhiteSpace(tomlContent))
        {
            return new AppOptions();
        }

        try
        {
            AppOptions? deserialized = TomlSerializer.Deserialize<AppOptions>(tomlContent);
            return deserialized ?? new AppOptions();
        }
        catch (Exception ex)
        {
            throw new FormatException($"The configuration file is malformed. {ex.Message}", ex);
        }
    }
}