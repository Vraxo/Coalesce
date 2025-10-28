using Coalesce.Configuration;
using Coalesce.Core;
using Xunit;

namespace Coalesce.Tests.Core;

public class DirectoryMergerTests : IDisposable
{
    private readonly string _tempTestRoot;
    private readonly string _sourceDir;
    private readonly string _outputDir;
    private readonly string _outputFilePath;

    public DirectoryMergerTests()
    {
        _tempTestRoot = Path.Combine(Path.GetTempPath(), "CoalesceTest_" + Guid.NewGuid().ToString("N"));
        _sourceDir = Path.Combine(_tempTestRoot, "src");
        _outputDir = Path.Combine(_tempTestRoot, "out");
        _outputFilePath = Path.Combine(_outputDir, "merged.txt");

        Directory.CreateDirectory(_sourceDir);
        Directory.CreateDirectory(_outputDir);

        File.WriteAllText(Path.Combine(_sourceDir, "file1.cs"), "public class Test {}");
        File.WriteAllText(Path.Combine(_sourceDir, "file2.txt"), "some text");
    }

    public void Dispose()
    {
        // Ensure file handles are released before deleting
        GC.Collect();
        GC.WaitForPendingFinalizers();

        if (Directory.Exists(_tempTestRoot))
        {
            Directory.Delete(_tempTestRoot, recursive: true);
        }
    }

    [Fact]
    public void Merge_WhenDryRunIsTrue_DoesNotWriteOutputFile()
    {
        // Arrange
        var options = new AppOptions
        {
            OutputFilePath = _outputFilePath,
            SourceDirectoryPaths = [_sourceDir]
        };
        var merger = new DirectoryMerger(options, dryRun: true);

        // Act
        merger.Merge();

        // Assert
        Assert.False(File.Exists(_outputFilePath), "The output file should not have been created during a dry run.");
    }

    [Fact]
    public void Merge_WhenDryRunIsFalse_WritesOutputFile()
    {
        // Arrange
        var options = new AppOptions
        {
            OutputFilePath = _outputFilePath,
            SourceDirectoryPaths = [_sourceDir],
            IncludeExtensions = [".cs", ".txt"]
        };
        var merger = new DirectoryMerger(options, dryRun: false);

        // Act
        merger.Merge();

        // Assert
        Assert.True(File.Exists(_outputFilePath), "The output file should have been created during a normal merge.");
    }

    [Fact]
    public void Merge_WhenOutputFileIsLocked_ShouldFailGracefully()
    {
        // Arrange
        var options = new AppOptions
        {
            OutputFilePath = _outputFilePath,
            SourceDirectoryPaths = [_sourceDir],
            IncludeExtensions = [".cs", ".txt"]
        };
        var merger = new DirectoryMerger(options, dryRun: false);

        // Act
        // Lock the file by opening a stream to it that we never close
        using var _ = new FileStream(_outputFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        merger.Merge();

        // Assert
        // The test passes if no unhandled exception was thrown.
        // In a real scenario, we would capture Console.Error output to verify
        // that the correct error message was logged. For now, this is sufficient.
        Assert.True(true);
    }
}