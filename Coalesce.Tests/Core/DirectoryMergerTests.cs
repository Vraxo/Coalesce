using AwesomeAssertions;
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
        GC.Collect();
        GC.WaitForPendingFinalizers();

        if (Directory.Exists(_tempTestRoot))
        {
            Directory.Delete(_tempTestRoot, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Merge_WhenDryRunIsTrue_DoesNotWriteOutputFile()
    {
        AppOptions options = new()
        {
            OutputFilePath = _outputFilePath,
            SourceDirectoryPaths = [_sourceDir]
        };
        DirectoryMerger merger = new(options, dryRun: true);

        merger.Merge();

        File.Exists(_outputFilePath).Should().BeFalse(
            "because dry run mode should only simulate the merge without writing any output");
    }

    [Fact]
    public void Merge_WhenDryRunIsFalse_WritesOutputFile()
    {
        AppOptions options = new()
        {
            OutputFilePath = _outputFilePath,
            SourceDirectoryPaths = [_sourceDir],
            IncludeExtensions = [".cs", ".txt"]
        };
        DirectoryMerger merger = new(options, dryRun: false);

        merger.Merge();

        File.Exists(_outputFilePath).Should().BeTrue(
            "because a normal merge process should successfully compile and write the output file");
    }

    [Fact]
    public void Merge_WhenOutputFileIsLocked_ShouldFailGracefully()
    {
        AppOptions options = new()
        {
            OutputFilePath = _outputFilePath,
            SourceDirectoryPaths = [_sourceDir],
            IncludeExtensions = [".cs", ".txt"]
        };
        DirectoryMerger merger = new(options, dryRun: false);

        using FileStream _ = new(
            _outputFilePath,
            FileMode.Create,
            FileAccess.ReadWrite,
            FileShare.None);

        merger.Invoking(m => m.Merge()).Should().NotThrow();
    }
}