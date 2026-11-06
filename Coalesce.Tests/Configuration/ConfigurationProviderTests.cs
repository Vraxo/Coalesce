using AwesomeAssertions;
using Coalesce.Configuration;
using Xunit;

namespace Coalesce.Tests.Configuration;

public class ConfigurationProviderTests : IDisposable
{
    private readonly string _tempConfigFile;
    private readonly string _malformedConfigFile;
    private readonly string _emptyConfigFile;
    private readonly string _missingSourcesConfigFile;
    private readonly string _missingOutputConfigFile;

    public ConfigurationProviderTests()
    {
        // Valid TOML config
        _tempConfigFile = Path.GetTempFileName();
        string tomlContent = """
            OutputFilePath = "from-config.txt"
            SourceDirectoryPaths = [
              "./config-src-1",
              "./config-src-2"
            ]
            ExcludeDirectoryNames = [
              ".git",
              "node_modules"
            ]
            IncludeExtensions = [
              ".toml",
              ".json"
            ]
            ExcludeExtensions = [
              ".tmp"
            ]
            PathOnlyExtensions = [
              ".svg"
            ]
            """;
        File.WriteAllText(_tempConfigFile, tomlContent);

        // Malformed TOML config (unclosed brackets/quotes)
        _malformedConfigFile = Path.GetTempFileName();
        File.WriteAllText(_malformedConfigFile, "OutputFilePath = \"from-config.txt\nSourceDirectoryPaths = [");

        // Empty file
        _emptyConfigFile = Path.GetTempFileName();
        File.WriteAllText(_emptyConfigFile, string.Empty);

        // Config missing SourceDirectoryPaths
        _missingSourcesConfigFile = Path.GetTempFileName();
        File.WriteAllText(_missingSourcesConfigFile, """
            OutputFilePath = "from-config.txt"
            SourceDirectoryPaths = []
            """);

        // Config missing OutputFilePath
        _missingOutputConfigFile = Path.GetTempFileName();
        File.WriteAllText(_missingOutputConfigFile, """
            OutputFilePath = ""
            SourceDirectoryPaths = ["./src"]
            """);
    }

    public void Dispose()
    {
        File.Delete(_tempConfigFile);
        File.Delete(_malformedConfigFile);
        File.Delete(_emptyConfigFile);
        File.Delete(_missingSourcesConfigFile);
        File.Delete(_missingOutputConfigFile);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Build_WithValidConfigFile_LoadsOptionsFromConfigFileExactly()
    {
        FileInfo configFile = new(_tempConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("from-config.txt");
        options.SourceDirectoryPaths.Should().HaveCount(2).And.ContainInOrder("./config-src-1", "./config-src-2");
        options.ExcludeDirectoryNames.Should().HaveCount(2).And.ContainInOrder(".git", "node_modules");
        options.IncludeExtensions.Should().BeEquivalentTo(".toml", ".json");
        options.ExcludeExtensions.Should().BeEquivalentTo(".tmp");
        options.PathOnlyExtensions.Should().BeEquivalentTo(".svg");
    }

    [Fact]
    public void Build_WhenSourceDirectoriesEmpty_FailsValidationAndReturnsNull()
    {
        FileInfo configFile = new(_missingSourcesConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WhenOutputFilePathEmpty_FailsValidationAndReturnsNull()
    {
        FileInfo configFile = new(_missingOutputConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WithMalformedTomlFile_ReturnsNull()
    {
        FileInfo configFile = new(_malformedConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WithEmptyConfigFile_LoadsEmptyConfigAndFailsValidation()
    {
        FileInfo configFile = new(_emptyConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WhenSpecifiedConfigFileNotFound_ReturnsNullAndFailsFast()
    {
        FileInfo nonExistentFile = new("non_existent_config.toml");

        AppOptions? options = ConfigurationProvider.Build(nonExistentFile);

        options.Should().BeNull();
    }
}
