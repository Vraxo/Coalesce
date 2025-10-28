# ✨ Coalesce

A simple CLI tool to merge source code from multiple files and directories into a single, context-rich text file.

---

### 🤖 About This Project

**Coalesce** is a command-line utility designed to solve a specific problem: consolidating an entire project's source code into one file. The primary goal is to create a comprehensive input for Large Language Models (LLMs), providing them with the full context of a codebase in a single pass—a process used extensively during the development of Coalesce itself.

This is a solo project, developed almost entirely by AI. It's built primarily for personal use, but is being shared in the hope that it might be useful to others. Please use it with the understanding that it's a personal tool, not a commercial product. It's provided "as-is" without any guarantees.

---

## ✅ Features

- **Directory Merging**: Scans one or more source directories and merges all eligible files.
- **Configuration File**: Easily configure options using a `coalesce.yaml` file.
- **CLI Overrides**: All configuration options can be overridden with command-line arguments for flexibility.
- **Smart Filtering**:
  - Include files based on a whitelist of extensions (`--include-ext`).
  - Exclude directories and files by name (`--exclude-dir`, `--exclude-file`).
  - Exclude files by extension (`--exclude-ext`).
- **Path-Only Mode**: Include the path of binary or irrelevant files without their content (`--path-only-ext`).
- **Project Presets**: Quickly generate a `coalesce.yaml` for common project types like `dotnet` and `node`.
- **Custom Presets**: Create your own presets for your specific project structures.
- **Dry Run Mode**: Preview which files will be included in the output without actually writing the file using the `--dry-run` flag.
- **Cross-Platform**: Generates OS-specific run scripts (`coalesce-run.bat` or `coalesce-run.sh`) for ease of use.

---

## 📥 Installation

**Platform Support Note:** Coalesce is developed and tested primarily on **Windows** 🖥️. While the core functionality is intended to be cross-platform, its behavior on macOS and Linux has not been verified. Some features, like the automatic `install-path` command, are Windows-specific.

1. Download the latest executable for your operating system from the **Releases** page.
2. Place the executable in a directory of your choice.
3. For ease of use, add this directory to your system's PATH environment variable.

For Windows users, you can use the built-in command to add Coalesce to your user PATH automatically:

```sh
coalesce install-path
```

*You may need to restart your terminal for the PATH change to take effect.*

---

## 🚀 Quick Start

The easiest way to get started is by using the `init` command in your project's root directory.

1. **Initialize Configuration**
   
   This command creates a default `coalesce.yaml` file and an appropriate run script (`.bat` or `.sh`).
   
   ```sh
   coalesce init
   ```
   
   If you're working with a Node.js or .NET project, you can use a preset to get a more tailored configuration:
   
   ```sh
   # For a Node.js project
   coalesce init --preset node
   
   # For a .NET project
   coalesce init --preset dotnet
   ```

2. **Customize `coalesce.yaml`**
   
   Open the newly created `coalesce.yaml` and edit the `sourceDirectoryPaths` and other options to match your project structure.
   
   ```yaml
   # Path for the final merged output file.
   outputFilePath: coalesce.md
   
   # List of source directories to scan for files.
   sourceDirectoryPaths:
     - ./src
     # - ./another-source-folder
   ```

3. **Run the Merge**
   
   Simply execute the main command. It will automatically find and use your `coalesce.yaml` file.
   
   ```sh
   coalesce
   ```
   
   This will generate the `coalesce.md` file (or whatever you specified in your config) in the current directory.

---

## 🛠️ Usage

### The `merge` Command

This is the default command. It merges files based on configuration from `coalesce.yaml` and any command-line overrides.

**Syntax**

```sh
coalesce [output-file] [source-dirs...] [options]
```

- `[output-file]`: (Optional) The path for the merged output file. **Overrides** the value in `coalesce.yaml`.
- `[source-dirs...]`: (Optional) One or more source directories to scan. **Overrides** the list in `coalesce.yaml`.

**Options**

| Option                  | Description                                                                               | Behavior with `coalesce.yaml`             |
| ----------------------- | ----------------------------------------------------------------------------------------- | ----------------------------------------- |
| `--config <path>`       | Path to a specific YAML configuration file.                                               |                                           |
| `--exclude-dir <name>`  | Excludes a directory by name (e.g., 'node_modules'). Can be used multiple times.          | **Adds to** the list in the config file.  |
| `--exclude-file <name>` | Excludes a file by name (e.g., 'yarn.lock'). Can be used multiple times.                  | **Adds to** the list in the config file.  |
| `--include-ext <ext>`   | Includes only files with this extension (e.g., '.md'). Can be used multiple times.        | **Replaces** the list in the config file. |
| `--exclude-ext <ext>`   | Excludes files with this extension (e.g., '.log'). Can be used multiple times.            | **Adds to** the list in the config file.  |
| `--path-only-ext <ext>` | Includes a file by path only, without content (e.g., '.dll'). Can be used multiple times. | **Adds to** the list in the config file.  |
| `--dry-run`             | Simulates a merge, printing which files would be included.                                |                                           |
| `-q`, `--quiet`         | Suppresses all informational output.                                                      |                                           |
| `-v`, `--verbose`       | Enables detailed output, showing why files are skipped.                                   |                                           |
| `--help`                | Show help and usage information.                                                          |                                           |

**Example**

```sh
# Run using the config file, but add 'dist' to the excluded directories
coalesce --exclude-dir dist

# Run with a specific output file and source, ignoring the config for those values
coalesce docs/context.md ./src ./lib --include-ext .js .css
```

### Other Commands

#### `init`

Generates a `coalesce.yaml` configuration file and a run script.

```sh
coalesce init [--preset <name>]
```

- `--preset`: (Optional) Initializes the config from a built-in (`dotnet`, `node`) or custom preset.

#### `preset`

Manages configuration presets.

- `coalesce preset list`: Lists all available built-in and custom presets.
- `coalesce preset path`: Displays the path to the user's custom presets directory.

#### `install-path` / `uninstall-path`

Manages the application's presence in the system PATH.

- On **Windows**, this command modifies the user's PATH automatically.
- On **macOS/Linux**, this command displays manual instructions for updating your PATH.

---

## 📝 Configuration (`coalesce.yaml`)

The `coalesce.yaml` file is the primary way to configure the tool for a project. The `init` command generates this file for you.

Here is an example of the default configuration:

```yaml
# Default configuration for Coalesce.
# Use 'coalesce init --preset <name>' for project-specific templates (e.g., dotnet, node).

# Path for the final merged output file.
outputFilePath: coalesce.md

# List of source directories to scan for files.
sourceDirectoryPaths:
  - ./src

# (Whitelist) To process ONLY specific file types, add their extensions here (e.g., ".cs", ".py").
# If this list is empty, Coalesce processes ALL files not matched by other exclusion rules.
includeExtensions: []

# (Blacklist) Explicitly skip files with these extensions.
excludeExtensions: []

# Skip directories with these names (case-insensitive).
excludeDirectoryNames:
  - .git

# Skip files with these names (case-insensitive).
excludeFileNames:
  - .DS_Store
  - Thumbs.db
  - coalesce.yaml

# For these file types, only include their path in the output, not their content.
# Useful for binary files like images, DLLs, etc.
pathOnlyExtensions:
  - .png
  - .jpg
  - .jpeg
  - .gif
  - .bmp
  - .svg
  - .ico
  - .webp
  - .dll
  - .exe
```

---

## 🎨 Presets

Presets are templates for `coalesce.yaml` that help you get started quickly.

**Using a Preset**
Use the `init` command with the `--preset` option:

```sh
coalesce init --preset node
```

**Creating a Custom Preset**

1. Find your presets directory:
   
   ```sh
   coalesce preset path
   ```

2. Navigate to that directory.

3. Copy the `_template.yaml` file to a new file, for example `my-preset.yaml`.

4. Customize `my-preset.yaml` to your needs.

You can now use your custom preset:

```sh
coalesce init --preset my-preset
```

Your custom preset will now also appear when you run `coalesce preset list`.

---

## 🤝 Contributing

Contributions are very welcome! If you encounter a bug, have an idea for a new feature, or want to improve the code, please feel free to:

- 🐛 **Open an Issue**: Report bugs or suggest new features.
- 🧑‍💻 **Submit a Pull Request**: Offer improvements, fixes, or new functionality.

---

## 📜 License

This project is licensed under the MIT License.
