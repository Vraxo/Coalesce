using AwesomeAssertions;
using Coalesce.Configuration;
using Xunit;

namespace Coalesce.Tests.Configuration;

public class ConfigurationProviderTests : IDisposable
{
    private readonly string _tempConfigFile;
    private readonly string _defaultOutputConfigFile;
    private readonly string _malformedConfigFile;
    private readonly string _emptyConfigFile;
    private readonly string _missingSourcesConfigFile;
    private readonly string _emptyOutputConfigFile;

    public ConfigurationProviderTests()
    {
        // Valid TOML config with all fields specified
        _tempConfigFile = Path.GetTempFileName();
        string tomlContent = """
            output = "from-config.txt"
            sources = [
              "./config-src-1",
              "./config-src-2"
            ]
            exclude_dirs = [
              "node_modules",
              "dist"
            ]
            exclude_files = [
              "package-lock.json"
            ]
            include_extensions = [
              ".toml",
              ".json"
            ]
            exclude_extensions = [
              ".tmp"
            ]
            """;
        File.WriteAllText(_tempConfigFile, tomlContent);

        // Config omitting 'output' (should default to "coalesce.md")
        _defaultOutputConfigFile = Path.GetTempFileName();
        File.WriteAllText(_defaultOutputConfigFile, """
            sources = ["./src"]
            """);

        // Malformed TOML config (unclosed brackets/quotes)
        _malformedConfigFile = Path.GetTempFileName();
        File.WriteAllText(_malformedConfigFile, "output = \"from-config.txt\nsources = [");

        // Empty file
        _emptyConfigFile = Path.GetTempFileName();
        File.WriteAllText(_emptyConfigFile, string.Empty);

        // Config missing sources
        _missingSourcesConfigFile = Path.GetTempFileName();
        File.WriteAllText(_missingSourcesConfigFile, """
            output = "from-config.txt"
            sources = []
            """);

        // Config with explicitly empty output
        _emptyOutputConfigFile = Path.GetTempFileName();
        File.WriteAllText(_emptyOutputConfigFile, """
            output = ""
            sources = ["./src"]
            """);
    }

    public void Dispose()
    {
        File.Delete(_tempConfigFile);
        File.Delete(_defaultOutputConfigFile);
        File.Delete(_malformedConfigFile);
        File.Delete(_emptyConfigFile);
        File.Delete(_missingSourcesConfigFile);
        File.Delete(_emptyOutputConfigFile);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Build_WithValidConfigFile_LoadsOptionsFromConfigFileExactly()
    {
        FileInfo configFile = new(_tempConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("from-config.txt");
        options.ConfigFilePath.Should().Be(configFile.FullName);
        options.SourceDirectoryPaths.Should().HaveCount(2).And.ContainInOrder("./config-src-1", "./config-src-2");
        options.ExcludeDirectoryNames.Should().HaveCount(2).And.ContainInOrder("node_modules", "dist");
        options.ExcludeFileNames.Should().BeEquivalentTo("package-lock.json");
        options.IncludeExtensions.Should().BeEquivalentTo(".toml", ".json");
        options.ExcludeExtensions.Should().BeEquivalentTo(".tmp");
    }

    [Fact]
    public void Build_WhenOutputIsOmitted_UsesDefaultOutputFilePath()
    {
        FileInfo configFile = new(_defaultOutputConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be(AppOptions.DefaultOutputFilePath);
        options.SourceDirectoryPaths.Should().BeEquivalentTo("./src");
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
        FileInfo configFile = new(_emptyOutputConfigFile);

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
