# ✨ Coalesce

A simple CLI tool to merge source code from multiple files and directories into a single, context-rich text file.

---

### 🤖 About This Project

**Coalesce** is a command-line utility designed to solve a specific problem: consolidating an entire project's source code into one file. The primary goal is to create a comprehensive input for Large Language Models (LLMs), providing them with the full context of a codebase in a single pass—a process used extensively during the development of Coalesce itself.

This is a solo project, developed almost entirely by AI. It's built primarily for personal use, but is being shared in the hope that it might be useful to others. Please use it with the understanding that it's a personal tool, not a commercial product. It's provided "as-is" without any guarantees.

---

## ✅ Features

- **Directory Merging**: Scans one or more source directories and merges all eligible files.
- **Configuration File**: Easily configure all merge and filtering options using a `coalesce.toml` file.
- **Smart Filtering**:
  - Include files based on a whitelist of extensions (`IncludeExtensions`).
  - Exclude directories and files by name (`ExcludeDirectoryNames`, `ExcludeFileNames`).
  - Exclude files by extension (`ExcludeExtensions`).
- **Path-Only Mode**: Include the path of binary or irrelevant files without their content (`PathOnlyExtensions`).
- **Project Presets**: Quickly generate a `coalesce.toml` for common project types like `dotnet` and `node`.
- **Dry Run Mode**: Preview which files will be included in the output without actually writing the file using the `--dry-run` flag.

---

## 📥 Installation

**Platform Support Note:** Coalesce is developed and tested primarily on **Windows** 🖥️. While the core functionality is intended to be cross-platform, its behavior on macOS and Linux has not been verified. Some features, like the automatic `install` command, are Windows-specific.

1. Download the latest executable for your operating system from the **Releases** page.
2. Place the executable in a directory of your choice.
3. For ease of use, add this directory to your system's PATH environment variable.

For Windows users, you can use the built-in command to add Coalesce to your user PATH automatically:

```sh
coalesce install
```

*You may need to restart your terminal for the PATH change to take effect.*

---

## 🚀 Quick Start

The easiest way to get started is by using the `init` command in your project's root directory.

1. **Initialize Configuration**
   
   This command creates a default `coalesce.toml` file in the current directory.
   
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

2. **Customize `coalesce.toml`**
   
   Open the newly created `coalesce.toml` and edit `SourceDirectoryPaths` and other options to match your project structure.
   
   ```toml
   # Path for the final merged output file.
   OutputFilePath = "coalesce.md"
   
   # List of source directories to scan for files.
   SourceDirectoryPaths = [
     "./src",
     # "./another-source-folder"
   ]
   ```

3. **Run the Merge**
   
   Simply execute the main command. It will automatically find and use your `coalesce.toml` file.
   
   ```sh
   coalesce
   ```
   
   This will generate the `coalesce.md` file (or whatever you specified in your config) in the current directory.

---

## 🛠️ Usage

### The `merge` Command

This is the default command. It merges files using `coalesce.toml` in the current directory (or a custom TOML file provided via `--config`).

**Syntax**

```sh
coalesce [options]
```

**Options**

| Option              | Description                                                                                      |
| ------------------- | ------------------------------------------------------------------------------------------------ |
| `--config <CONFIG>` | Path to a TOML configuration file. Defaults to `coalesce.toml` in the current directory.         |
| `--dry-run`         | Simulates a merge, printing which files would be included without writing the output file.       |
| `--quiet`           | Suppresses all informational output. Only warnings and errors will be displayed.                 |
| `--verbose`         | Enables detailed output, showing why files are skipped and how configuration is applied.         |
| `-h`, `--help`      | Show help and usage information.                                                                 |

**Examples**

```sh
# Run using coalesce.toml in the current directory
coalesce

# Preview which files would be merged without writing the output file
coalesce --dry-run

# Run using a specific TOML configuration file
coalesce --config custom-config.toml
```

### Other Commands

#### `init`

Generates a `coalesce.toml` configuration file in the current directory.

```sh
coalesce init [--preset <PRESET>]
```

- `--preset <PRESET>`: (Optional) Initializes `coalesce.toml` from a built-in preset template (`dotnet`, `node`).

#### `install` / `uninstall`

Manages the application's presence in the system PATH.

- On **Windows**, this command modifies the user's PATH automatically.
- On **macOS/Linux**, this command displays manual instructions for updating your PATH.

---

## 📝 Configuration (`coalesce.toml`)

The `coalesce.toml` file is the primary way to configure the tool for a project. The `init` command generates this file for you.

Here is the default configuration:

```toml
# Default configuration for Coalesce.
# Use 'coalesce init --preset <name>' for project-specific templates (e.g., dotnet, node).

# Path for the final merged output file.
OutputFilePath = "coalesce.md"

# List of source directories to scan for files.
SourceDirectoryPaths = [
  "./src"
]

# (Whitelist) To process ONLY specific file types, add their extensions here (e.g., ".cs", ".py").
# If this list is empty, Coalesce processes ALL files not matched by other exclusion rules.
IncludeExtensions = []

# (Blacklist) Explicitly skip files with these extensions.
ExcludeExtensions = []

# Skip directories with these names (case-insensitive).
ExcludeDirectoryNames = [
  ".git"
]

# Skip files with these names (case-insensitive).
ExcludeFileNames = [
  ".DS_Store",
  "Thumbs.db",
  "coalesce.toml"
]

# For these file types, only include their path in the output, not their content.
# Useful for binary files like images, DLLs, etc.
PathOnlyExtensions = [
  ".png",
  ".jpg",
  ".jpeg",
  ".gif",
  ".bmp",
  ".svg",
  ".ico",
  ".webp",
  ".dll",
  ".exe",
  ".so",
  ".bin",
  ".pdf"
]
```

---

## 🎨 Presets

Presets are built-in templates for `coalesce.toml` that help you get started quickly for common project types.

**Available Presets**

- `dotnet`: Configured for .NET projects (includes `.cs`, `.csproj`, `.sln`, etc., and excludes `bin`, `obj`, `.vs`, `packages`).
- `node`: Configured for Node.js projects (includes `.js`, `.ts`, `.jsx`, `.tsx`, etc., and excludes `node_modules`, `dist`, `build`, lock files).

**Using a Preset**

Use the `init` command with the `--preset` option:

```sh
coalesce init --preset node
```

---

## 🤝 Contributing

Contributions are very welcome! If you encounter a bug, have an idea for a new feature, or want to improve the code, please feel free to:

- 🐛 **Open an Issue**: Report bugs or suggest new features.
- 🧑‍💻 **Submit a Pull Request**: Offer improvements, fixes, or new functionality.

---

## 📜 License

This project is licensed under the MIT License.
