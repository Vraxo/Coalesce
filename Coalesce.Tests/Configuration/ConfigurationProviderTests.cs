using AwesomeAssertions;
using Coalesce.Cli;
using Coalesce.Configuration;
using Xunit;

namespace Coalesce.Tests.Configuration;

public class ConfigurationProviderTests : IDisposable
{
    private readonly string _tempConfigFile;
    private readonly string _malformedConfigFile;
    private readonly string _emptyConfigFile;

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
    }

    public void Dispose()
    {
        File.Delete(_tempConfigFile);
        File.Delete(_malformedConfigFile);
        File.Delete(_emptyConfigFile);
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Build_WhenNoConfigFile_UsesCliArgumentsDirectlyAndRawly()
    {
        MergeSettings settings = new()
        {
            OutputFile = "cli-output.txt",
            SourceDirs = ["./cli-src"],
            ExcludeDir = ["bin"]
        };

        AppOptions? options = ConfigurationProvider.Build(settings, null);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("cli-output.txt");
        options.SourceDirectoryPaths.Should().BeEquivalentTo("./cli-src");
        options.ExcludeDirectoryNames.Should().BeEquivalentTo("bin");
        options.ExcludeFileNames.Should().BeEmpty();
    }

    [Fact]
    public void Build_InConfigMode_LoadsOptionsFromConfigFileExactly()
    {
        FileInfo configFile = new(_tempConfigFile);
        MergeSettings settings = new(); // No positional arguments supplied

        AppOptions? options = ConfigurationProvider.Build(settings, configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("from-config.txt");
        options.SourceDirectoryPaths.Should().HaveCount(2).And.ContainInOrder("./config-src-1", "./config-src-2");
        options.ExcludeDirectoryNames.Should().HaveCount(2).And.ContainInOrder(".git", "node_modules");
        options.IncludeExtensions.Should().BeEquivalentTo(".toml", ".json");
        options.ExcludeExtensions.Should().BeEquivalentTo(".tmp");
        options.PathOnlyExtensions.Should().BeEquivalentTo(".svg");
    }

    [Fact]
    public void Build_InConfigMode_IgnoresCliArguments()
    {
        FileInfo configFile = new(_tempConfigFile);
        MergeSettings settings = new()
        {
            OutputFile = "ignored-output.txt",
            SourceDirs = ["./ignored-src"]
        };

        AppOptions? options = ConfigurationProvider.Build(settings, configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("from-config.txt");
        options.SourceDirectoryPaths.Should().HaveCount(2).And.ContainInOrder("./config-src-1", "./config-src-2");
    }

    [Fact]
    public void Build_WhenValidationFails_ReturnsNull()
    {
        MergeSettings settings = new()
        {
            OutputFile = "cli-output.txt",
            SourceDirs = [] // Invalid empty sources
        };

        AppOptions? options = ConfigurationProvider.Build(settings, null);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WithMalformedTomlFile_ReturnsNull()
    {
        FileInfo configFile = new(_malformedConfigFile);
        MergeSettings settings = new();

        AppOptions? options = ConfigurationProvider.Build(settings, configFile);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WithEmptyConfigFile_LoadsEmptyConfigAndFailsValidation()
    {
        FileInfo configFile = new(_emptyConfigFile);
        MergeSettings settings = new();

        AppOptions? options = ConfigurationProvider.Build(settings, configFile);

        options.Should().BeNull(); // Fails validation as empty config has no outputs or sources
    }

    [Fact]
    public void Build_WhenSpecifiedConfigFileNotFound_ReturnsNullAndFailsFast()
    {
        FileInfo nonExistentFile = new("non_existent_config.toml");
        MergeSettings settings = new();

        AppOptions? options = ConfigurationProvider.Build(settings, nonExistentFile);

        options.Should().BeNull();
    }
}