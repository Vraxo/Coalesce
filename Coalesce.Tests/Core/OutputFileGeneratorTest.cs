using AwesomeAssertions;
using Coalesce.Core;
using System.Text;
using Xunit;

namespace Coalesce.Tests.Core;

public class OutputFileGeneratorTests : IDisposable
{
    private readonly string _tempTestRoot;
    private readonly string _tempCsFilePath;
    private readonly string _tempPngFilePath;
    private readonly string _tempCustomBinaryFilePath;
    private readonly string _tempUnknownExtFilePath;

    public OutputFileGeneratorTests()
    {
        _tempTestRoot = Path.Combine(Path.GetTempPath(), "CoalesceTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempTestRoot);

        _tempCsFilePath = Path.Combine(_tempTestRoot, "test.cs");
        File.WriteAllText(_tempCsFilePath, "public class Test {}");

        _tempPngFilePath = Path.Combine(_tempTestRoot, "logo.png");
        File.WriteAllBytes(_tempPngFilePath, [1, 2, 3]);

        _tempCustomBinaryFilePath = Path.Combine(_tempTestRoot, "payload.dat");
        File.WriteAllBytes(_tempCustomBinaryFilePath, [0x48, 0x65, 0x00, 0x6C]);

        _tempUnknownExtFilePath = Path.Combine(_tempTestRoot, "file.unknown");
        File.WriteAllText(_tempUnknownExtFilePath, "some data");
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
    public void WriteFileEntry_ForKnownTextFile_WritesMarkdownFormatWithLanguage()
    {
        StringBuilder stringBuilder = new();
        using StringWriter stringWriter = new(stringBuilder);
        OutputFileGenerator generator = new(stringWriter);
        string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempCsFilePath);
        string expectedOutput = $$"""
            ### `{{relativePath}}`

            ```csharp
            public class Test {}
            ```

            ---


            """;

        generator.WriteFileEntry(_tempCsFilePath);
        string result = stringBuilder.ToString().ReplaceLineEndings("\n");
        expectedOutput = expectedOutput.ReplaceLineEndings("\n");

        result.Should().Be(expectedOutput);
    }

    [Fact]
    public void WriteFileEntry_ForBinaryExtensionFile_WritesMarkdownFormatWithComment()
    {
        StringBuilder stringBuilder = new();
        using StringWriter stringWriter = new(stringBuilder);
        OutputFileGenerator generator = new(stringWriter);
        string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempPngFilePath);
        string expectedOutput = $$"""
            ### `{{relativePath}}`

            <!-- Content of binary file 'logo.png' not included. -->

            ---


            """;

        generator.WriteFileEntry(_tempPngFilePath);
        string result = stringBuilder.ToString().ReplaceLineEndings("\n");
        expectedOutput = expectedOutput.ReplaceLineEndings("\n");

        result.Should().Be(expectedOutput);
    }

    [Fact]
    public void WriteFileEntry_ForNullByteBinaryFile_WritesMarkdownFormatWithComment()
    {
        StringBuilder stringBuilder = new();
        using StringWriter stringWriter = new(stringBuilder);
        OutputFileGenerator generator = new(stringWriter);
        string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempCustomBinaryFilePath);
        string expectedOutput = $$"""
            ### `{{relativePath}}`

            <!-- Content of binary file 'payload.dat' not included. -->

            ---


            """;

        generator.WriteFileEntry(_tempCustomBinaryFilePath);
        string result = stringBuilder.ToString().ReplaceLineEndings("\n");
        expectedOutput = expectedOutput.ReplaceLineEndings("\n");

        result.Should().Be(expectedOutput);
    }

    [Fact]
    public void WriteFileEntry_ForUnknownTextFile_WritesMarkdownFormatWithoutLanguage()
    {
        StringBuilder stringBuilder = new();
        using StringWriter stringWriter = new(stringBuilder);
        OutputFileGenerator generator = new(stringWriter);
        string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, _tempUnknownExtFilePath);
        string expectedOutput = $$"""
            ### `{{relativePath}}`

            ```
            some data
            ```

            ---


            """;

        generator.WriteFileEntry(_tempUnknownExtFilePath);
        string result = stringBuilder.ToString().ReplaceLineEndings("\n");
        expectedOutput = expectedOutput.ReplaceLineEndings("\n");

        result.Should().Be(expectedOutput);
    }
}
