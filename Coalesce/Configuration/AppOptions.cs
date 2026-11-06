namespace Coalesce.Configuration;

public sealed class AppOptions
{
    public const string DefaultOutputFilePath = "coalesce.md";

    public string OutputFilePath { get; set; } = DefaultOutputFilePath;

    public List<string> SourceDirectoryPaths { get; set; } = [];

    public List<string> ValidSourceDirectoryPaths { get; } = [];

    public string ConfigFilePath { get; set; } = string.Empty;

    public List<string> IncludeExtensions { get; set; } = [];

    public List<string> ExcludeExtensions { get; set; } = [];

    public List<string> ExcludeDirectoryNames { get; set; } = [];

    public List<string> ExcludeFileNames { get; set; } = [];
}
