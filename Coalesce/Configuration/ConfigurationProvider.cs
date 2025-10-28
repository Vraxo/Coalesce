using Coalesce.Utils;
using System.IO;
using System.Reflection;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Coalesce.Configuration;

public class ConfigurationProvider
{
    public AppOptions? Build(string? outputArg, List<string> sourceArgs, List<string> excludeDirOptions, List<string> excludeFileOptions, List<string> includeExtOptions, List<string> excludeExtOptions, List<string> pathOnlyExtOptions, FileInfo? configFileOption)
    {
        // 1. Load base options from YAML file or fall back to embedded defaults.
        AppOptions? options = LoadOptions(configFileOption);
        if (options == null)
        {
            return null; // A critical error occurred during loading.
        }

        // 2. Apply command-line arguments as overrides.
        ApplyCommandLineOverrides(options, outputArg, sourceArgs, excludeDirOptions, excludeFileOptions, includeExtOptions, excludeExtOptions, pathOnlyExtOptions);

        // 3. Validate the final, merged options.
        if (!Validate(options))
        {
            return null; // Validation failed, errors were already logged.
        }

        return options;
    }

    private void ApplyCommandLineOverrides(AppOptions options, string? outputArg, List<string> sourceArgs, List<string> excludeDirOptions, List<string> excludeFileOptions, List<string> includeExtOptions, List<string> excludeExtOptions, List<string> pathOnlyExtOptions)
    {
        // Positional arguments for output and source REPLACE the values from the config file.
        if (!string.IsNullOrEmpty(outputArg))
        {
            Logger.WriteVerbose($"Overriding 'outputFilePath' with CLI argument: '{outputArg}'");
            options.OutputFilePath = outputArg;
        }

        if (sourceArgs.Count > 0)
        {
            Logger.WriteVerbose("Overriding 'sourceDirectoryPaths' with CLI arguments.");
            options.SourceDirectoryPaths = sourceArgs;
        }

        // Flag options for excluded directories ADD to the values from the config file.
        if (excludeDirOptions.Count > 0)
        {
            Logger.WriteVerbose("Adding to 'excludeDirectoryNames' from CLI arguments.");
            foreach (string dir in excludeDirOptions)
            {
                if (!options.ExcludeDirectoryNames.Contains(dir, StringComparer.OrdinalIgnoreCase))
                {
                    options.ExcludeDirectoryNames.Add(dir);
                }
            }
        }

        // Flag options for excluded files ADD to the values from the config file.
        if (excludeFileOptions.Count > 0)
        {
            Logger.WriteVerbose("Adding to 'excludeFileNames' from CLI arguments.");
            foreach (string file in excludeFileOptions)
            {
                if (!options.ExcludeFileNames.Contains(file, StringComparer.OrdinalIgnoreCase))
                {
                    options.ExcludeFileNames.Add(file);
                }
            }
        }

        // If --include-ext is used, it REPLACES the default/config list because it acts as a focused whitelist.
        if (includeExtOptions.Count > 0)
        {
            Logger.WriteVerbose("Replacing 'includeExtensions' with CLI arguments.");
            options.IncludeExtensions = includeExtOptions;
        }

        // Exclude and path-only extensions from CLI are ADDED to the lists from the config file.
        if (excludeExtOptions.Count > 0)
        {
            Logger.WriteVerbose("Adding to 'excludeExtensions' from CLI arguments.");
            foreach (string ext in excludeExtOptions)
            {
                if (!options.ExcludeExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                {
                    options.ExcludeExtensions.Add(ext);
                }
            }
        }

        if (pathOnlyExtOptions.Count > 0)
        {
            Logger.WriteVerbose("Adding to 'pathOnlyExtensions' from CLI arguments.");
            foreach (string ext in pathOnlyExtOptions)
            {
                if (!options.PathOnlyExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase))
                {
                    options.PathOnlyExtensions.Add(ext);
                }
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

    private AppOptions? LoadOptions(FileInfo? configFileOption)
    {
        string? resolvedConfigPath = null;

        if (configFileOption != null)
        {
            if (!configFileOption.Exists)
            {
                Logger.WriteWarning($"Configuration file specified via --config not found: {configFileOption.FullName}");
                // Fall back to embedded defaults if specified config is missing
                return LoadDefaultOptionsFromEmbedddedResource();
            }
            resolvedConfigPath = configFileOption.FullName;
        }
        else
        {
            string defaultConfigPath = Path.Combine(Environment.CurrentDirectory, "coalesce.yaml");
            if (File.Exists(defaultConfigPath))
            {
                resolvedConfigPath = defaultConfigPath;
            }
        }

        if (resolvedConfigPath != null)
        {
            Logger.WriteInfo($"Loading configuration from: {resolvedConfigPath}");
            return LoadOptionsFromYamlFile(resolvedConfigPath);
        }

        // No config file found, so use the embedded default configuration.
        Logger.WriteVerbose("No 'coalesce.yaml' found. Using built-in default configuration.");
        return LoadDefaultOptionsFromEmbedddedResource();
    }

    private static AppOptions? LoadOptionsFromYamlFile(string configPath)
    {
        try
        {
            string yamlContent = File.ReadAllText(configPath);
            return DeserializeYaml(yamlContent);
        }
        catch (Exception ex)
        {
            Logger.WriteError($"Error loading or parsing config file '{configPath}': {ex.Message}");
            return null;
        }
    }

    private static AppOptions? LoadDefaultOptionsFromEmbedddedResource()
    {
        try
        {
            string yamlContent = GetEmbeddedResource("Coalesce.Resources.DefaultConfig.yaml");
            return DeserializeYaml(yamlContent);
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

    private static AppOptions? DeserializeYaml(string yamlContent)
    {
        if (string.IsNullOrWhiteSpace(yamlContent))
        {
            return new AppOptions();
        }

        try
        {
            var reader = new StringReader(yamlContent);
            var parser = new Parser(reader);

            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            // Consume the stream start event
            parser.Consume<StreamStart>();

            // An empty file or a file with only comments is valid.
            // In this case, there are no more events after StreamStart except StreamEnd.
            if (parser.Accept(out StreamEnd _))
            {
                return new AppOptions();
            }

            // The deserializer reads a single document.
            var options = deserializer.Deserialize<AppOptions>(parser) ?? new AppOptions();

            // After successfully deserializing one document, we expect the end of the stream.
            // If there is more content (e.g., a second document, or malformed text), this will fail.
            parser.Consume<StreamEnd>();

            return options;
        }
        catch (YamlException ex)
        {
            // Any parsing or deserialization error will be caught here.
            throw new FormatException($"The configuration file is malformed. {ex.Message}", ex);
        }
    }
}