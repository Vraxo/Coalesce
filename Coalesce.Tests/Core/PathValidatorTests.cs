using AwesomeAssertions;
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
        _tempTestRoot = Path.Combine(Path.GetTempPath(), "CoalesceTest_" + Guid.NewGuid().ToString("N"));
        _existingSourceDir = Path.Combine(_tempTestRoot, "src");
        _existingOutputDir = Path.Combine(_tempTestRoot, "out");

        Directory.CreateDirectory(_existingSourceDir);
        Directory.CreateDirectory(_existingOutputDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempTestRoot))
        {
            Directory.Delete(_tempTestRoot, true);
        }
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void TryValidateAndPrepare_WithValidPaths_ReturnsTrue()
    {
        AppOptions options = new()
        {
            OutputFilePath = Path.Combine(_existingOutputDir, "output.txt"),
            SourceDirectoryPaths = [_existingSourceDir]
        };

        bool result = PathValidator.TryValidateAndPrepare(options);

        result.Should().BeTrue();
        options.ValidSourceDirectoryPaths.Should().ContainSingle()
            .Which.Should().Be(Path.GetFullPath(_existingSourceDir));
    }

    [Fact]
    public void TryValidateAndPrepare_WhenOutputDirectoryDoesNotExist_ReturnsFalse()
    {
        string nonExistentOutputDir = Path.Combine(_tempTestRoot, "nonexistent_out");
        AppOptions options = new()
        {
            OutputFilePath = Path.Combine(nonExistentOutputDir, "output.txt"),
            SourceDirectoryPaths = [_existingSourceDir]
        };

        bool result = PathValidator.TryValidateAndPrepare(options);

        result.Should().BeFalse();
    }

    [Fact]
    public void TryValidateAndPrepare_WhenAllSourceDirectoriesDoNotExist_ReturnsFalse()
    {
        string nonExistentSourceDir = Path.Combine(_tempTestRoot, "nonexistent_src");
        AppOptions options = new()
        {
            OutputFilePath = Path.Combine(_existingOutputDir, "output.txt"),
            SourceDirectoryPaths = [nonExistentSourceDir]
        };

        bool result = PathValidator.TryValidateAndPrepare(options);

        result.Should().BeFalse();
        options.ValidSourceDirectoryPaths.Should().BeEmpty();
    }

    [Fact]
    public void TryValidateAndPrepare_WithMixedValidAndInvalidSourceDirs_ReturnsTrueAndIncludesOnlyValid()
    {
        string nonExistentSourceDir = Path.Combine(_tempTestRoot, "nonexistent_src");
        AppOptions options = new()
        {
            OutputFilePath = Path.Combine(_existingOutputDir, "output.txt"),
            SourceDirectoryPaths = [_existingSourceDir, nonExistentSourceDir]
        };

        bool result = PathValidator.TryValidateAndPrepare(options);

        result.Should().BeTrue();
        options.ValidSourceDirectoryPaths.Should().ContainSingle()
            .Which.Should().Be(Path.GetFullPath(_existingSourceDir));
    }
}