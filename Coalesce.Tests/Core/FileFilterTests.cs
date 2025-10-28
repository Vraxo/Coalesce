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
        // Arrange
        FileFilter filter = new(_options);
        string filePath = @"C:\output\merged.txt";

        // Act
        bool result = filter.ShouldSkip(filePath, @"C:\project");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldSkip_WhenFileIsInAnExcludedDirectory_ReturnsTrue()
    {
        // Arrange
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\bin\debug\app.exe";

        // Act
        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnExcludedExtension_ReturnsTrue()
    {
        // Arrange
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\main.tmp";

        // Act
        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldSkip_WhenIncludeListExistsAndFileExtensionIsNotInIt_ReturnsTrue()
    {
        // Arrange
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\style.css"; // .css is not in IncludeExtensions

        // Act
        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAnIncludedExtension_ReturnsFalse()
    {
        // Arrange
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\app.cs";

        // Act
        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldSkip_WhenFileHasAPathOnlyExtension_ReturnsFalse()
    {
        // Arrange
        FileFilter filter = new(_options);
        string filePath = @"C:\project\src\logo.png"; // .png is a path-only extension

        // Act
        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ShouldSkip_WhenIncludeListIsEmptyAndFileIsNotOtherwiseExcluded_ReturnsFalse()
    {
        // Arrange
        AppOptions optionsWithNoIncludes = new()
        {
            OutputFilePath = @"C:\output\merged.txt",
            IncludeExtensions = [], // Empty list
            ExcludeDirectoryNames = ["bin"],
            PathOnlyExtensions = [".png"]
        };
        FileFilter filter = new(optionsWithNoIncludes);
        string filePath = @"C:\project\src\README.md";

        // Act
        bool result = filter.ShouldSkip(filePath, @"C:\project\src");

        // Assert
        Assert.False(result);
    }
}