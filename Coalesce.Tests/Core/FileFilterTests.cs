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
            OutputFilePath = @"C:\output\merged.txt",
            IncludeExtensions = [".cs", ".html"],
            ExcludeExtensions = [".tmp", ".bak"],
            ExcludeDirectoryNames = ["bin", "obj", ".git"],
            PathOnlyExtensions = [".png", ".jpg"]
        };
    }

    [Fact]
    public void ShouldSkip_WhenFilePathIsTheOutputFile_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = @"C:\output\merged.txt";

        bool result = filter.ShouldSkip(filePath, @"C:\project");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileIsInAnExcludedDirectory_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\bin\debug\app.exe";

        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnExcludedExtension_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\main.tmp";

        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenIncludeListExistsAndFileExtensionIsNotInIt_ReturnsTrue()
    {
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\style.css";

        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        result.Should().BeTrue();
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnIncludedExtension_ReturnsFalse()
    {
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\app.cs";

        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAPathOnlyExtension_ReturnsFalse()
    {
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\logo.png";

        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        result.Should().BeFalse();
    }

    [Fact]
    public void ShouldSkip_WhenIncludeListIsEmptyAndFileIsNotOtherwiseExcluded_ReturnsFalse()
    {
        AppOptions optionsWithNoIncludes = new()
        {
            OutputFilePath = @"C:\output\merged.txt",
            IncludeExtensions = [],
            ExcludeDirectoryNames = ["bin"],
            PathOnlyExtensions = [".png"]
        };
        FileFilter filter = new(optionsWithNoIncludes);
        string filePath = @"C:\project\src\README.md";

        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        result.Should().BeFalse();
    }
}