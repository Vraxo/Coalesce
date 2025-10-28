using Coalesce.Configuration;
using Coalesce.Core;
using Coalesce.Utils;
using System;
using System.CommandLine;

namespace Coalesce.Cli;

public static partial class CommandLineBuilder
{
    public static RootCommand Build()
    {
        // init command
        var presetOption = new Option<string?>(
            name: "--preset",
            description: "Initializes 'coalesce.yaml' from a built-in or custom preset template."
        );
        var initCommand = new Command("init", "Generates a default 'coalesce.yaml' and run script in the current directory.")
        {
            presetOption
        };
        initCommand.SetHandler(ConfigurationGenerator.GenerateDefaultConfig, presetOption);

        // preset command
        var presetListCommand = new Command("list", "Lists all available built-in and custom presets.");
        presetListCommand.SetHandler(PresetManager.List);

        var presetPathCommand = new Command("path", "Displays the path to the user's presets directory.");
        presetPathCommand.SetHandler(PresetManager.ShowPath);

        var presetCommand = new Command("preset", "Manage configuration presets.")
        {
            presetListCommand,
            presetPathCommand
        };

        // path commands
        var installPathCommand = new Command("install-path", "Adds the application directory to the user's PATH (Windows only).");
        installPathCommand.SetHandler(PathManager.Install);

        var uninstallPathCommand = new Command("uninstall-path", "Removes the application directory from the user's PATH (Windows only).");
        uninstallPathCommand.SetHandler(PathManager.Uninstall);

        // Arguments and Options are now named to match the CliParameters properties (PascalCase -> kebab-case)
        Argument<string?> outputArgument = new(name: "output-file")
        {
            Description = "Path for the merged output file. Required if not in config.",
            Arity = ArgumentArity.ZeroOrOne
        };

        Argument<List<string>> sourceArgument = new(name: "source-dirs")
        {
            Description = "Source directories to scan. Required if not in config.",
            Arity = ArgumentArity.ZeroOrMore
        };

        Option<List<string>> excludeDirOption = new(name: "--exclude-dir")
        {
            Description = "Excludes a directory by name (e.g., 'node_modules'). Can be used multiple times.",
            AllowMultipleArgumentsPerToken = true
        };

        Option<List<string>> excludeFileOption = new(name: "--exclude-file")
        {
            Description = "Excludes a file by name (e.g., 'package-lock.json'). Can be used multiple times.",
            AllowMultipleArgumentsPerToken = true
        };

        Option<List<string>> includeExtOption = new(name: "--include-ext")
        {
            Description = "Includes a file extension (e.g., '.md'). Replaces the config list. Can be used multiple times.",
            AllowMultipleArgumentsPerToken = true
        };

        Option<List<string>> excludeExtOption = new(name: "--exclude-ext")
        {
            Description = "Excludes a file extension (e.g., '.log'). Can be used multiple times.",
            AllowMultipleArgumentsPerToken = true
        };

        Option<List<string>> pathOnlyExtOption = new(name: "--path-only-ext")
        {
            Description = "Includes a file by path only, without its content (e.g., '.dll'). Can be used multiple times.",
            AllowMultipleArgumentsPerToken = true
        };

        Option<FileInfo?> configOption = new(name: "--config")
        {
            Description = "Path to a YAML configuration file. If not provided, 'coalesce.yaml' in the current directory is used if it exists."
        };

        Option<bool> dryRunOption = new(name: "--dry-run")
        {
            Description = "Simulates a merge, printing which files would be included without writing the output file."
        };

        Option<bool> quietOption = new(aliases: ["-q", "--quiet"], getDefaultValue: () => false)
        {
            Description = "Suppresses all informational output. Only warnings and errors will be displayed."
        };

        Option<bool> verboseOption = new(aliases: ["-v", "--verbose"], getDefaultValue: () => false)
        {
            Description = "Enables detailed output, showing why files are skipped and how configuration is applied."
        };

        RootCommand rootCommand = new()
        {
            Description = "A tool to merge multiple source files into a single output file, respecting directory structure and file types."
        };

        rootCommand.AddArgument(outputArgument);
        rootCommand.AddArgument(sourceArgument);
        rootCommand.AddOption(excludeDirOption);
        rootCommand.AddOption(excludeFileOption);
        rootCommand.AddOption(includeExtOption);
        rootCommand.AddOption(excludeExtOption);
        rootCommand.AddOption(pathOnlyExtOption);
        rootCommand.AddOption(configOption);
        rootCommand.AddOption(dryRunOption);
        rootCommand.AddOption(quietOption);
        rootCommand.AddOption(verboseOption);
        rootCommand.AddCommand(initCommand);
        rootCommand.AddCommand(presetCommand);
        rootCommand.AddCommand(installPathCommand);
        rootCommand.AddCommand(uninstallPathCommand);

        // The handler now cleanly binds all arguments and options to the CliParameters object.
        CliParametersBinder binder = new()
        {
            OutputArgument = outputArgument,
            SourceArgument = sourceArgument,
            ExcludeDirOption = excludeDirOption,
            ExcludeFileOption = excludeFileOption,
            IncludeExtOption = includeExtOption,
            ExcludeExtOption = excludeExtOption,
            PathOnlyExtOption = pathOnlyExtOption,
            ConfigOption = configOption,
            DryRunOption = dryRunOption,
            QuietOption = quietOption,
            VerboseOption = verboseOption
        };

        rootCommand.SetHandler(RunMerge, binder);

        return rootCommand;
    }

    private static void RunMerge(CliParameters parameters)
    {
        Logger.Initialize(parameters.Quiet, parameters.Verbose);

        ConfigurationProvider configProvider = new();
        AppOptions? options = configProvider.Build(
            parameters.OutputFile,
            parameters.SourceDirs,
            parameters.ExcludeDir,
            parameters.ExcludeFile,
            parameters.IncludeExt,
            parameters.ExcludeExt,
            parameters.PathOnlyExt,
            parameters.Config);

        if (options is not null)
        {
            try
            {
                new DirectoryMerger(options, parameters.DryRun).Merge();
            }
            catch (Exception ex)
            {
                Logger.WriteError($"An unexpected error occurred: {ex.Message}");
            }
        }
        // If options is null, error messages were already printed.
        // The prompt to continue is removed for a standard CLI experience.
    }
}