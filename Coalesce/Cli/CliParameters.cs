namespace Coalesce.Cli;

public class CliParameters
{
    public string? OutputFile { get; set; }
    public List<string> SourceDirs { get; set; } = [];
    public List<string> ExcludeDir { get; set; } = [];
    public List<string> ExcludeFile { get; set; } = [];
    public List<string> IncludeExt { get; set; } = [];
    public List<string> ExcludeExt { get; set; } = [];
    public List<string> PathOnlyExt { get; set; } = [];
    public FileInfo? Config { get; set; }
    public bool DryRun { get; set; }
    public bool Quiet { get; set; }
    public bool Verbose { get; set; }
}