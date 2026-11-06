using AwesomeAssertions;
using Coalesce.Configuration;
using Coalesce.Core;
using Xunit;

namespace Coalesce.Tests.Core;

public class FileFilterTests
{
    private readonly AppOptions _options;

    public FileFilterTests()
    {
        _options = new AppOptions
        {
            OutputFilePath = "/output/merged.txt",
            ConfigFilePath = "/project/custom-config.toml",
            IncludeExtensions = [".cs", ".html", ".json"],
            ExcludeExtensions = [".tmp", ".bak"],
            ExcludeDirectoryNames = ["bin", "obj"],
            ExcludeFileNames = ["package-lock.json"]
        };
    }

    [Fact]
    public void ShouldSkip_WhenFilePathIsTheOutputFile_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/output/merged.txt";

        bool result = filter.ShouldSkip(filePath, "/project");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFilePathIsTheConfigFile_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/custom-config.toml";

        bool result = filter.ShouldSkip(filePath, "/project");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileIsDefaultExcludedFileName_ReturnsTrue()
    {
        AppOptions optionsWithNoIncludes = new()
        {
            OutputFilePath = "/output/merged.txt"
        };
        FileFilter filter = new(optionsWithNoIncludes);

        filter.ShouldSkip("/project/src/.DS_Store", "/project/src").Should().BeTrue();
        filter.ShouldSkip("/project/src/Thumbs.db", "/project/src").Should().BeTrue();
        filter.ShouldSkip("/project/src/coalesce.toml", "/project/src").Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileIsInDefaultExcludedGitDirectory_ReturnsTrue()
    {
        AppOptions optionsWithNoIncludes = new()
        {
            OutputFilePath = "/output/merged.txt"
        };
        FileFilter filter = new(optionsWithNoIncludes);
        string filePath = "/project/src/.git/config";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileIsInAnExcludedDirectory_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/src/bin/debug/app.cs";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnExcludedFileName_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/src/package-lock.json";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnExcludedExtension_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/src/main.tmp";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenIncludeListExistsAndFileExtensionIsNotInIt_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/src/style.css";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnIncludedExtension_ReturnsFalse()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/src/app.cs";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldSkip_WhenIncludeListIsEmptyAndFileIsNotOtherwiseExcluded_ReturnsFalse()
    {
        AppOptions optionsWithNoIncludes = new()
        {
            OutputFilePath = "/output/merged.txt",
            IncludeExtensions = [],
            ExcludeDirectoryNames = ["bin"]
        };
        FileFilter filter = new(optionsWithNoIncludes);
        string filePath = "/project/src/README.md";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeFalse();
    }
}
