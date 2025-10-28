using YamlDotNet.Serialization;

namespace Coalesce.Configuration;

public class AppOptions
{
    public string OutputFilePath { get; set; } = string.Empty;

    public List<string> SourceDirectoryPaths { get; set; } = [];

    [YamlIgnore]
    public List<string> ValidSourceDirectoryPaths { get; } = [];

    public List<string> IncludeExtensions { get; set; } = [];

    public List<string> ExcludeExtensions { get; set; } = [];

    public List<string> ExcludeDirectoryNames { get; set; } = [];

    public List<string> ExcludeFileNames { get; set; } = [];

    public List<string> PathOnlyExtensions { get; set; } = [];
}