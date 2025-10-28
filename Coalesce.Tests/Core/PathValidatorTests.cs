using Coalesce.Configuration;
using Coalesce.Core;
using Xunit;

namespace Coalesce.Tests.Core;

public class PathValidatorTests : IDisposable
{
    private readonly string _tempTestRoot;
    private readonly string _existingSourceDir;
    private readonly string _existingOutputDir;

    public PathValidatorTests()
    {
        // Create a unique root directory for this test run.
        _tempTestRoot = Path.Combine(Path.GetTempPath(), "CoalesceTest_" + Guid.NewGuid().ToString("N"));
        _existingSourceDir = Path.Combine(_tempTestRoot, "src");
        _existingOutputDir = Path.Combine(_tempTestRoot, "out");

        // Create the directories that our tests will expect to exist.
        Directory.CreateDirectory(_existingSourceDir);
        Directory.CreateDirectory(_existingOutputDir);
    }

    public void Dispose()
    {
        // Clean up the entire temporary directory structure after tests are done.
        if (Directory.Exists(_tempTestRoot))
        {
            Directory.Delete(_tempTestRoot, true);
        }
    }

    [Fact]
    public void TryValidateAndPrepare_WithValidPaths_ReturnsTrue()
    {
        // Arrange
        var options = new AppOptions
        {
            OutputFilePath = Path.Combine(_existingOutputDir, "output.txt"),
            SourceDirectoryPaths = [_existingSourceDir]
        };

        // Act
        bool result = PathValidator.TryValidateAndPrepare(options);

        // Assert
        Assert.True(result);
        var validPath = Assert.Single(options.ValidSourceDirectoryPaths);
        Assert.Equal(Path.GetFullPath(_existingSourceDir), validPath);
    }

    [Fact]
    public void TryValidateAndPrepare_WhenOutputDirectoryDoesNotExist_ReturnsFalse()
    {
        // Arrange
        string nonExistentOutputDir = Path.Combine(_tempTestRoot, "nonexistent_out");
        var options = new AppOptions
        {
            OutputFilePath = Path.Combine(nonExistentOutputDir, "output.txt"),
            SourceDirectoryPaths = [_existingSourceDir]
        };

        // Act
        bool result = PathValidator.TryValidateAndPrepare(options);

        // Assert
        Assert.False(result);
    }



    [Fact]
    public void TryValidateAndPrepare_WhenAllSourceDirectoriesDoNotExist_ReturnsFalse()
    {
        // Arrange
        string nonExistentSourceDir = Path.Combine(_tempTestRoot, "nonexistent_src");
        var options = new AppOptions
        {
            OutputFilePath = Path.Combine(_existingOutputDir, "output.txt"),
            SourceDirectoryPaths = [nonExistentSourceDir]
        };

        // Act
        bool result = PathValidator.TryValidateAndPrepare(options);

        // Assert
        Assert.False(result);
        Assert.Empty(options.ValidSourceDirectoryPaths);
    }

    [Fact]
    public void TryValidateAndPrepare_WithMixedValidAndInvalidSourceDirs_ReturnsTrueAndIncludesOnlyValid()
    {
        // Arrange
        string nonExistentSourceDir = Path.Combine(_tempTestRoot, "nonexistent_src");
        var options = new AppOptions
        {
            OutputFilePath = Path.Combine(_existingOutputDir, "output.txt"),
            SourceDirectoryPaths = [_existingSourceDir, nonExistentSourceDir]
        };

        // Act
        bool result = PathValidator.TryValidateAndPrepare(options);

        // Assert
        Assert.True(result);
        // It should succeed but only add the valid source directory to the list.
        var validPath = Assert.Single(options.ValidSourceDirectoryPaths);
        Assert.Equal(Path.GetFullPath(_existingSourceDir), validPath);
    }
}