using System.Text.Json.Serialization;

namespace Coalesce.Configuration;

public sealed class AppOptions
{
    public const string DefaultOutputFilePath = "coalesce.md";

    [JsonPropertyName("output")]
    public string OutputFilePath { get; set; } = DefaultOutputFilePath;

    [JsonPropertyName("sources")]
    public List<string> SourceDirectoryPaths { get; set; } = [];

    [JsonIgnore]
    public List<string> ValidSourceDirectoryPaths { get; } = [];

    [JsonIgnore]
    public string ConfigFilePath { get; set; } = string.Empty;

    [JsonPropertyName("include_extensions")]
    public List<string> IncludeExtensions { get; set; } = [];

    [JsonPropertyName("exclude_extensions")]
    public List<string> ExcludeExtensions { get; set; } = [];

    [JsonPropertyName("exclude_dirs")]
    public List<string> ExcludeDirectoryNames { get; set; } = [];

    [JsonPropertyName("exclude_files")]
    public List<string> ExcludeFileNames { get; set; } = [];
}
