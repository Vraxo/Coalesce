using Coalesce.Configuration;
using Coalesce.Core;
using System.Text;
using Xunit;

namespace Coalesce.Tests.Core;

public class OutputFileGeneratorTests : IDisposable
{
    private readonly string _tempTestRoot;
    private readonly string _tempCsFilePath;
    private readonly string _tempPngFilePath;
    private readonly string _tempUnknownExtFilePath;
    private readonly AppOptions _options;

    public OutputFileGeneratorTests()
    {
        _tempTestRoot = Path.Combine(Path.GetTempPath(), "CoalesceTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempTestRoot);

        _tempCsFilePath = Path.Combine(_tempTestRoot, "test.cs");
        File.WriteAllText(_tempCsFilePath, "public class Test {}");

        _tempPngFilePath = Path.Combine(_tempTestRoot, "logo.png");
        File.WriteAllBytes(_tempPngFilePath, [1, 2, 3]);

        _tempUnknownExtFilePath = Path.Combine(_tempTestRoot, "file.unknown");
        File.WriteAllText(_tempUnknownExtFilePath, "some data");

        _options = new()
        {
            PathOnlyExtensions = [".png"]
        };
    }

    public void Dispose()
    {
        if (!Directory.Exists(_tempTestRoot))
        {
            return;
        }

        Directory.Delete(_tempTestRoot, true);
    }

    [Fact]
    public void WriteFileEntry_ForKnownTextFile_WritesMarkdownFormatWithLanguage()
    {
        // Arrange
        var stringBuilder = new StringBuilder();
        using var stringWriter = new StringWriter(stringBuilder);
        var generator = new OutputFileGenerator(stringWriter);
        string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempCsFilePath);
        string expectedOutput = $$"""
            ### `{{relativePath}}`

            ```csharp
            public class Test {}
            ```

            """;

        // Act
        generator.WriteFileEntry(_tempCsFilePath, _options);
        string result = stringBuilder.ToString().ReplaceLineEndings("\n");
        expectedOutput = expectedOutput.ReplaceLineEndings("\n");

        // Assert
        Assert.Equal(expectedOutput, result);
    }

    [Fact]
    public void WriteFileEntry_ForPathOnlyFile_WritesMarkdownFormatWithComment()
    {
        // Arrange
        var stringBuilder = new StringBuilder();
        using var stringWriter = new StringWriter(stringBuilder);
        var generator = new OutputFileGenerator(stringWriter);
        string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempPngFilePath);
        string expectedOutput = $$"""
            ### `{{relativePath}}`

            <!-- Content of binary file 'logo.png' not included. -->

            """;

        // Act
        generator.WriteFileEntry(_tempPngFilePath, _options);
        string result = stringBuilder.ToString().ReplaceLineEndings("\n");
        expectedOutput = expectedOutput.ReplaceLineEndings("\n");

        // Assert
        Assert.Equal(expectedOutput, result);
    }

    [Fact]
    public void WriteFileEntry_ForUnknownTextFile_WritesMarkdownFormatWithoutLanguage()
    {
        // Arrange
        var stringBuilder = new StringBuilder();
        using var stringWriter = new StringWriter(stringBuilder);
        var generator = new OutputFileGenerator(stringWriter);
        string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempUnknownExtFilePath);
        string expectedOutput = $$"""
            ### `{{relativePath}}`

            ```
            some data
            ```

            """;

        // Act
        generator.WriteFileEntry(_tempUnknownExtFilePath, _options);
        string result = stringBuilder.ToString().ReplaceLineEndings("\n");
        expectedOutput = expectedOutput.ReplaceLineEndings("\n");

        // Assert
        Assert.Equal(expectedOutput, result);
    }
}