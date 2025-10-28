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
        // Valid config
        _tempConfigFile = Path.GetTempFileName();
        string yamlContent = """
            outputFilePath: from-config.txt
            sourceDirectoryPaths:
              - ./config-src-1
              - ./config-src-2
            excludeDirectoryNames:
              - .git
              - node_modules
            includeExtensions:
              - .yaml
              - .json
            excludeExtensions:
              - .tmp
            pathOnlyExtensions:
              - .svg
            """;
        File.WriteAllText(_tempConfigFile, yamlContent);

        // Malformed config
        _malformedConfigFile = Path.GetTempFileName();
        File.WriteAllText(_malformedConfigFile, "outputFilePath: from-config.txt\n  - invalid-indent");

        // Empty file
        _emptyConfigFile = Path.GetTempFileName();
        File.WriteAllText(_emptyConfigFile, string.Empty);
    }

    public void Dispose()
    {
        File.Delete(_tempConfigFile);
        File.Delete(_malformedConfigFile);
        File.Delete(_emptyConfigFile);
    }

    [Fact]
    public void Build_WhenNoConfigFile_UsesCliArgumentsAndDefaults()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        string cliOutputPath = "cli-output.txt";
        var cliSourcePaths = new List<string> { "./cli-src" };
        var cliExcludeDirs = new List<string> { "bin" };
        var expectedExcludes = new AppOptions().ExcludeDirectoryNames;

        // Act
        var options = provider.Build(cliOutputPath, cliSourcePaths, cliExcludeDirs, [], [], [], [], null);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(cliOutputPath, options.OutputFilePath);
        Assert.Equal(cliSourcePaths, options.SourceDirectoryPaths);
        Assert.Contains("bin", options.ExcludeDirectoryNames); // Should add to defaults
    }

    [Fact]
    public void Build_WhenNoCliOverrides_LoadsOptionsFromConfigFile()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        var configFile = new FileInfo(_tempConfigFile);

        // Act
        var options = provider.Build(null, [], [], [], [], [], [], configFile);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("from-config.txt", options.OutputFilePath);
        Assert.Equal(2, options.SourceDirectoryPaths.Count);
        Assert.Contains("./config-src-1", options.SourceDirectoryPaths);
        Assert.Equal(2, options.ExcludeDirectoryNames.Count);
        Assert.Contains("node_modules", options.ExcludeDirectoryNames);
        Assert.Contains(".yaml", options.IncludeExtensions);
        Assert.Contains(".tmp", options.ExcludeExtensions);
        Assert.Contains(".svg", options.PathOnlyExtensions);
    }

    [Fact]
    public void Build_WhenCliPathArgsExist_TheyReplaceConfigFileValues()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        var configFile = new FileInfo(_tempConfigFile);
        string cliOutputPath = "cli-output.txt";
        var cliSourcePaths = new List<string> { "./cli-src" };

        // Act
        var options = provider.Build(cliOutputPath, cliSourcePaths, [], [], [], [], [], configFile);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(cliOutputPath, options.OutputFilePath);
        Assert.Equal(cliSourcePaths, options.SourceDirectoryPaths);
    }

    [Fact]
    public void Build_WhenCliExcludeArgsExist_TheyAreAddedToConfigFileValues()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        var configFile = new FileInfo(_tempConfigFile);
        var cliExcludeDirs = new List<string> { "bin", "obj" };

        // Act
        var options = provider.Build(null, [], cliExcludeDirs, [], [], [], [], configFile);

        // Assert
        Assert.NotNull(options);
        Assert.Equal(4, options.ExcludeDirectoryNames.Count);
        Assert.Contains(".git", options.ExcludeDirectoryNames);
        Assert.Contains("node_modules", options.ExcludeDirectoryNames);
        Assert.Contains("bin", options.ExcludeDirectoryNames);
        Assert.Contains("obj", options.ExcludeDirectoryNames);
    }

    [Fact]
    public void Build_WhenCliExtensionArgsExist_TheyAreAppliedCorrectly()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        var configFile = new FileInfo(_tempConfigFile);
        var cliIncludeExt = new List<string> { ".md" }; // should replace [.yaml, .json]
        var cliExcludeExt = new List<string> { ".log" }; // should be added to [.tmp]
        var cliPathOnlyExt = new List<string> { ".bin" }; // should be added to [.svg]

        // Act
        var options = provider.Build(
            null,
            [],
            [],
            [],
            cliIncludeExt,
            cliExcludeExt,
            cliPathOnlyExt,
            configFile);

        // Assert
        Assert.NotNull(options);

        // --include-ext should REPLACE
        Assert.Equal(cliIncludeExt, options.IncludeExtensions);
        Assert.DoesNotContain(".yaml", options.IncludeExtensions);

        // --exclude-ext should ADD
        Assert.Equal(2, options.ExcludeExtensions.Count);
        Assert.Contains(".tmp", options.ExcludeExtensions);
        Assert.Contains(".log", options.ExcludeExtensions);

        // --path-only-ext should ADD
        Assert.Equal(2, options.PathOnlyExtensions.Count);
        Assert.Contains(".svg", options.PathOnlyExtensions);
        Assert.Contains(".bin", options.PathOnlyExtensions);
    }

    [Fact]
    public void Build_WhenValidationFails_ReturnsNull()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        string cliOutputPath = "cli-output.txt";
        var emptySourcePaths = new List<string>();
        var emptyConfigFileInfo = new FileInfo(_emptyConfigFile);

        // Act
        // No source paths are provided via CLI, and the config file is empty, so it should fail validation.
        var options = provider.Build(cliOutputPath, emptySourcePaths, [], [], [], [], [], emptyConfigFileInfo);

        // Assert
        Assert.Null(options);
    }

    [Fact]
    public void Build_WithMalformedYamlFile_ReturnsNull()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        var configFile = new FileInfo(_malformedConfigFile);

        // Act
        var options = provider.Build("output.txt", ["./src"], [], [], [], [], [], configFile);

        // Assert
        Assert.Null(options); // Should fail gracefully
    }

    [Fact]
    public void Build_WithEmptyYamlFile_DoesNotThrowAndBuildsFromCli()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        var configFile = new FileInfo(_emptyConfigFile);

        // Act
        var options = provider.Build("output.txt", ["./src"], [], [], [], [], [], configFile);

        // Assert
        Assert.NotNull(options);
        Assert.Equal("output.txt", options.OutputFilePath);
        Assert.Empty(options.ExcludeDirectoryNames); // An empty config file provides no defaults.
    }

    [Fact]
    public void Build_WhenSpecifiedConfigFileNotFound_FallsBackToDefaults()
    {
        // Arrange
        var provider = new ConfigurationProvider();
        var nonExistentFile = new FileInfo("non_existent_config.yaml");

        // Act
        var options = provider.Build("output.txt", ["./src"], [], [], [], [], [], nonExistentFile);

        // Assert
        Assert.NotNull(options);
        // This confirms it used the defaults/CLI args, not an empty config
        Assert.NotEmpty(options.ExcludeDirectoryNames);
    }
}