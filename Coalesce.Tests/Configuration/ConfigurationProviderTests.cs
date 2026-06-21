using AwesomeAssertions;
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
    public void Build_WhenNoConfigFile_UsesCliArgumentsAndDefaults()
    {
        string cliOutputPath = "cli-output.txt";
        List<string> cliSourcePaths = ["./cli-src"];
        List<string> cliExcludeDirs = ["bin"];

        AppOptions? options = ConfigurationProvider.Build(cliOutputPath, cliSourcePaths, cliExcludeDirs, [], [], [], [], null);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be(cliOutputPath);
        options.SourceDirectoryPaths.Should().BeEquivalentTo(cliSourcePaths);
        options.ExcludeDirectoryNames.Should().Contain("bin");
    }

    [Fact]
    public void Build_WhenNoCliOverrides_LoadsOptionsFromConfigFile()
    {
        FileInfo configFile = new(_tempConfigFile);

        AppOptions? options = ConfigurationProvider.Build(null, [], [], [], [], [], [], configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("from-config.txt");
        options.SourceDirectoryPaths.Should().HaveCount(2).And.ContainInOrder("./config-src-1", "./config-src-2");
        options.ExcludeDirectoryNames.Should().HaveCount(2).And.ContainInOrder(".git", "node_modules");
        options.IncludeExtensions.Should().BeEquivalentTo(".toml", ".json");
        options.ExcludeExtensions.Should().BeEquivalentTo(".tmp");
        options.PathOnlyExtensions.Should().BeEquivalentTo(".svg");
    }

    [Fact]
    public void Build_WhenCliPathArgsExist_TheyReplaceConfigFileValues()
    {
        FileInfo configFile = new(_tempConfigFile);
        string cliOutputPath = "cli-output.txt";
        List<string> cliSourcePaths = ["./cli-src"];

        AppOptions? options = ConfigurationProvider.Build(cliOutputPath, cliSourcePaths, [], [], [], [], [], configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be(cliOutputPath);
        options.SourceDirectoryPaths.Should().BeEquivalentTo(cliSourcePaths);
    }

    [Fact]
    public void Build_WhenCliExcludeArgsExist_TheyAreAddedToConfigFileValues()
    {
        FileInfo configFile = new(_tempConfigFile);
        List<string> cliExcludeDirs = ["bin", "obj"];

        AppOptions? options = ConfigurationProvider.Build(null, [], cliExcludeDirs, [], [], [], [], configFile);

        options.Should().NotBeNull();
        options!.ExcludeDirectoryNames.Should().BeEquivalentTo(".git", "node_modules", "bin", "obj");
    }

    [Fact]
    public void Build_WhenCliExtensionArgsExist_TheyAreAppliedCorrectly()
    {
        FileInfo configFile = new(_tempConfigFile);
        List<string> cliIncludeExt = [".md"];
        List<string> cliExcludeExt = [".log"];
        List<string> cliPathOnlyExt = [".bin"];

        AppOptions? options = ConfigurationProvider.Build(
            null,
            [],
            [],
            [],
            cliIncludeExt,
            cliExcludeExt,
            cliPathOnlyExt,
            configFile);

        options.Should().NotBeNull();

        options!.IncludeExtensions.Should().BeEquivalentTo(cliIncludeExt);
        options.IncludeExtensions.Should().NotContain(".toml");

        options.ExcludeExtensions.Should().BeEquivalentTo(".tmp", ".log");
        options.PathOnlyExtensions.Should().BeEquivalentTo(".svg", ".bin");
    }

    [Fact]
    public void Build_WhenValidationFails_ReturnsNull()
    {
        string cliOutputPath = "cli-output.txt";
        List<string> emptySourcePaths = [];
        FileInfo emptyConfigFileInfo = new(_emptyConfigFile);

        AppOptions? options = ConfigurationProvider.Build(
            cliOutputPath,
            emptySourcePaths,
            [],
            [],
            [],
            [],
            [],
            emptyConfigFileInfo);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WithMalformedTomlFile_ReturnsNull()
    {
        FileInfo configFile = new(_malformedConfigFile);

        AppOptions? options = ConfigurationProvider.Build(
            "output.txt",
            ["./src"],
            [],
            [],
            [],
            [],
            [],
            configFile);

        options.Should().BeNull();
    }

    [Fact]
    public void Build_WithEmptyTomlFile_DoesNotThrowAndBuildsFromCli()
    {
        FileInfo configFile = new(_emptyConfigFile);

        AppOptions? options = ConfigurationProvider.Build(
            "output.txt",
            ["./src"],
            [],
            [],
            [],
            [],
            [],
            configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("output.txt");
        options.ExcludeDirectoryNames.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenSpecifiedConfigFileNotFound_FallsBackToDefaults()
    {
        FileInfo nonExistentFile = new("non_existent_config.toml");

        AppOptions? options = ConfigurationProvider.Build("output.txt", ["./src"], [], [], [], [], [], nonExistentFile);

        options.Should().NotBeNull();
        options!.ExcludeDirectoryNames.Should().NotBeEmpty();
    }
}