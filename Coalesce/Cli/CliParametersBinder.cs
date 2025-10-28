using System.CommandLine;
using System.CommandLine.Binding;

namespace Coalesce.Cli;

public class CliParametersBinder : BinderBase<CliParameters>
{
    public required Argument<string?> OutputArgument { get; init; }
    public required Argument<List<string>> SourceArgument { get; init; }
    public required Option<List<string>> ExcludeDirOption { get; init; }
    public required Option<List<string>> ExcludeFileOption { get; init; }
    public required Option<List<string>> IncludeExtOption { get; init; }
    public required Option<List<string>> ExcludeExtOption { get; init; }
    public required Option<List<string>> PathOnlyExtOption { get; init; }
    public required Option<FileInfo?> ConfigOption { get; init; }
    public required Option<bool> DryRunOption { get; init; }
    public required Option<bool> QuietOption { get; init; }
    public required Option<bool> VerboseOption { get; init; }

    protected override CliParameters GetBoundValue(BindingContext bindingContext)
    {
        return new()
        {
            OutputFile = bindingContext.ParseResult.GetValueForArgument(OutputArgument),
            SourceDirs = bindingContext.ParseResult.GetValueForArgument(SourceArgument) ?? [],
            ExcludeDir = bindingContext.ParseResult.GetValueForOption(ExcludeDirOption) ?? [],
            ExcludeFile = bindingContext.ParseResult.GetValueForOption(ExcludeFileOption) ?? [],
            IncludeExt = bindingContext.ParseResult.GetValueForOption(IncludeExtOption) ?? [],
            ExcludeExt = bindingContext.ParseResult.GetValueForOption(ExcludeExtOption) ?? [],
            PathOnlyExt = bindingContext.ParseResult.GetValueForOption(PathOnlyExtOption) ?? [],
            Config = bindingContext.ParseResult.GetValueForOption(ConfigOption),
            DryRun = bindingContext.ParseResult.GetValueForOption(DryRunOption),
            Quiet = bindingContext.ParseResult.GetValueForOption(QuietOption),
            Verbose = bindingContext.ParseResult.GetValueForOption(VerboseOption)
        };
    }
}