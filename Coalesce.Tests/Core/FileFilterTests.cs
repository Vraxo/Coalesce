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
            ConfigFilePath = "/project/custom.coalesce",
            IncludeExtensions = [".cs", ".html", ".json"],
            ExcludeExtensions = [".tmp", ".bak"],
            ExcludeDirectoryNames = ["legacy"],
            ExcludeFileNames = ["secret.json"]
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
        string filePath = "/project/custom.coalesce";

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
        filter.ShouldSkip("/project/src/.coalesce", "/project/src").Should().BeTrue();
        filter.ShouldSkip("/project/src/package-lock.json", "/project/src").Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileIsInDefaultExcludedDirectory_ReturnsTrue()
    {
        AppOptions optionsWithNoIncludes = new()
        {
            OutputFilePath = "/output/merged.txt"
        };
        FileFilter filter = new(optionsWithNoIncludes);

        filter.ShouldSkip("/project/src/.git/config", "/project/src").Should().BeTrue();
        filter.ShouldSkip("/project/src/bin/debug/app.cs", "/project/src").Should().BeTrue();
        filter.ShouldSkip("/project/src/obj/project.assets.json", "/project/src").Should().BeTrue();
        filter.ShouldSkip("/project/src/node_modules/pkg/index.js", "/project/src").Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileIsInAnExcludedDirectory_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/src/legacy/old.cs";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnExcludedFileName_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = "/project/src/secret.json";

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
            ExcludeDirectoryNames = ["legacy"]
        };
        FileFilter filter = new(optionsWithNoIncludes);
        string filePath = "/project/src/README.md";

        bool result = filter.ShouldSkip(filePath, "/project/src");

        result.Should().BeFalse();
    }
}
