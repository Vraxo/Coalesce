using AwesomeAssertions;
using Coalesce.Configuration;
using Xunit;

namespace Coalesce.Tests.Configuration;

public class ConfigurationProviderTests : IDisposable
{
    private readonly string _tempDir;
    private readonly string _defaultNamedConfigFile;
    private readonly string _customNamedConfigFile;
    private readonly string _emptyConfigFile;

    public ConfigurationProviderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "CoalesceConfigTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);

        _defaultNamedConfigFile = Path.Combine(_tempDir, ".coalesce");
        File.WriteAllText(_defaultNamedConfigFile, """
            # Comment line
            !./config-src-1
            !./config-src-2/
            !*.cs
            !.json

            docs/
            *.tmp
            secret.txt
            """);

        _customNamedConfigFile = Path.Combine(_tempDir, "api.coalesce");
        File.WriteAllText(_customNamedConfigFile, """
            +src/api/
            +*.ts
            *.spec.ts
            """);

        _emptyConfigFile = Path.Combine(_tempDir, "empty.coalesce");
        File.WriteAllText(_emptyConfigFile, """
            # Only comments and whitespace

            """);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Build_WithDefaultNamedConfigFile_LoadsRulesAndUsesCoalesceMdOutput()
    {
        FileInfo configFile = new(_defaultNamedConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("coalesce.md");
        options.ConfigFilePath.Should().Be(configFile.FullName);
        options.SourceDirectoryPaths.Should().HaveCount(2).And.ContainInOrder("./config-src-1", "./config-src-2");
        options.IncludeExtensions.Should().BeEquivalentTo(".cs", ".json");
        options.ExcludeDirectoryNames.Should().BeEquivalentTo("docs", "secret.txt");
        options.ExcludeFileNames.Should().BeEquivalentTo("secret.txt");
        options.ExcludeExtensions.Should().BeEquivalentTo(".tmp");
    }

    [Fact]
    public void Build_WithCustomNamedConfigFile_DerivesOutputFileNameFromConfigName()
    {
        FileInfo configFile = new(_customNamedConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("api.md");
        options.SourceDirectoryPaths.Should().BeEquivalentTo("src/api");
        options.IncludeExtensions.Should().BeEquivalentTo(".ts");
    }

    [Fact]
    public void Build_WhenNoIncludeDirectoriesSpecified_DefaultsSourcesToCurrentDirectory()
    {
        FileInfo configFile = new(_emptyConfigFile);

        AppOptions? options = ConfigurationProvider.Build(configFile);

        options.Should().NotBeNull();
        options!.OutputFilePath.Should().Be("empty.md");
        options.SourceDirectoryPaths.Should().BeEquivalentTo(".");
        options.IncludeExtensions.Should().BeEmpty();
        options.ExcludeExtensions.Should().BeEmpty();
    }

    [Fact]
    public void Build_WhenSpecifiedConfigFileNotFound_ReturnsNull()
    {
        FileInfo nonExistentFile = new(Path.Combine(_tempDir, "non_existent.coalesce"));

        AppOptions? options = ConfigurationProvider.Build(nonExistentFile);

        options.Should().BeNull();
    }
}
