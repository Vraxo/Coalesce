using Coalesce.Utils;

namespace Coalesce.Configuration;

public static class ConfigurationProvider
{
    public const string DefaultConfigFileName = ".coalesce";

    public static AppOptions? Build(FileInfo? configFileOption = null)
    {
        if (configFileOption is not null)
        {
            if (!configFileOption.Exists)
            {
                Log.Error($"Configuration file not found: {configFileOption.FullName}");
                return null;
            }

            Log.Info($"Loading configuration from: {configFileOption.FullName}");
            return LoadFromFile(configFileOption);
        }

        FileInfo defaultConfig = new(Path.Combine(Environment.CurrentDirectory, DefaultConfigFileName));
        if (defaultConfig.Exists)
        {
            Log.Info($"Loading configuration from: {defaultConfig.FullName}");
            return LoadFromFile(defaultConfig);
        }

        Log.Info($"No '{DefaultConfigFileName}' file found. Using default configuration.");
        return new AppOptions
        {
            OutputFilePath = AppOptions.DefaultOutputFilePath,
            SourceDirectoryPaths = ["."]
        };
    }

    public static AppOptions Parse(string content, FileInfo? configFile = null)
    {
        AppOptions options = new()
        {
            OutputFilePath = DeriveOutputFileName(configFile),
            ConfigFilePath = configFile?.FullName ?? string.Empty
        };

        using StringReader reader = new(content);
        string? rawLine;
        while ((rawLine = reader.ReadLine()) is not null)
        {
            string line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith('#'))
            {
                continue;
            }

            bool isInclude = false;
            if (line.StartsWith('!') || line.StartsWith('+'))
            {
                isInclude = true;
                line = line[1..].Trim();
            }
            else if (line.StartsWith('-'))
            {
                line = line[1..].Trim();
            }

            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            ApplyRule(options, line, isInclude);
        }

        if (options.SourceDirectoryPaths.Count == 0)
        {
            options.SourceDirectoryPaths.Add(".");
        }

        return options;
    }

    public static string DeriveOutputFileName(FileInfo? configFile)
    {
        if (configFile is null)
        {
            return AppOptions.DefaultOutputFilePath;
        }

        string stem = Path.GetFileNameWithoutExtension(configFile.Name).TrimStart('.');
        if (string.IsNullOrWhiteSpace(stem) ||
            stem.Equals("coalesce", StringComparison.OrdinalIgnoreCase))
        {
            return AppOptions.DefaultOutputFilePath;
        }

        return $"{stem}.md";
    }

    private static AppOptions? LoadFromFile(FileInfo configFile)
    {
        try
        {
            string content = File.ReadAllText(configFile.FullName);
            return Parse(content, configFile);
        }
        catch (Exception ex)
        {
            Log.Error($"Error loading configuration file '{configFile.FullName}': {ex.Message}");
            return null;
        }
    }

    private static void ApplyRule(AppOptions options, string pattern, bool isInclude)
    {
        // 1. Extension rule: '*.ext' (or '!.ext' / '+.ext' when including)
        if (TryExtractExtension(pattern, isInclude, out string extension))
        {
            if (isInclude)
            {
                options.IncludeExtensions.Add(extension);
            }
            else
            {
                options.ExcludeExtensions.Add(extension);
            }

            return;
        }

        // 2. Include directory / source path rule: '!src/' or '!Coalesce'
        if (isInclude)
        {
            string sourcePath = pattern.TrimEnd('/', '\\');
            if (string.IsNullOrEmpty(sourcePath))
            {
                sourcePath = ".";
            }

            options.SourceDirectoryPaths.Add(sourcePath);
            return;
        }

        // 3. Exclude rule: trailing slash means directory-only; bare name matches directory or file
        if (pattern.EndsWith('/') || pattern.EndsWith('\\'))
        {
            string dirName = pattern.TrimEnd('/', '\\');
            if (!string.IsNullOrEmpty(dirName))
            {
                options.ExcludeDirectoryNames.Add(dirName);
            }

            return;
        }

        options.ExcludeDirectoryNames.Add(pattern);
        options.ExcludeFileNames.Add(pattern);
    }

    private static bool TryExtractExtension(string pattern, bool isInclude, out string extension)
    {
        extension = string.Empty;

        if (pattern.StartsWith("*.", StringComparison.Ordinal) &&
            pattern.Length > 2 &&
            !pattern.AsSpan(2).Contains('/') &&
            !pattern.AsSpan(2).Contains('\\') &&
            !pattern.AsSpan(2).Contains('*'))
        {
            extension = pattern[1..];
            return true;
        }

        if (isInclude &&
            pattern.StartsWith('.') &&
            pattern.Length > 1 &&
            !pattern.Contains('/') &&
            !pattern.Contains('\\') &&
            !pattern.Contains('*'))
        {
            extension = pattern;
            return true;
        }

        return false;
    }
}
