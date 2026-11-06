# ✨ Coalesce

A simple CLI tool to merge source code from multiple files and directories into a single, context-rich Markdown file.

---

### 🤖 About This Project

**Coalesce** is a command-line utility designed to solve a specific problem: consolidating an entire project's source code into one file. The primary goal is to create a comprehensive input for Large Language Models (LLMs), providing them with the full context of a codebase in a single pass—a process used extensively during the development of Coalesce itself.

This is a solo project, developed almost entirely by AI. It's built primarily for personal use, but is being shared in the hope that it might be useful to others. Please use it with the understanding that it's a personal tool, not a commercial product. It's provided "as-is" without any guarantees.

---

## ✅ Features

- **Zero-Config Ready**: Run `coalesce` in any project without a config file—it scans the current directory, skips common build/dependency folders (`bin`, `obj`, `node_modules`, `.git`, `dist`, `artifacts`, etc.) and lockfiles automatically, and writes `coalesce.md`.
- **Flat `.coalesce` Pattern File**: Optionally customize what gets merged using a simple, line-based `.coalesce` file—no TOML, YAML, or JSON boilerplate.
- **Exclude & Include Rules**:
  - Plain lines exclude directories (`docs/`), extensions (`*.log`), or files (`secret.json`).
  - Lines starting with `!` (or `+`) narrow the merge to specific directories (`!src/`) or extensions (`!*.cs`).
- **Automatic Output Naming**: Running `coalesce` outputs `coalesce.md`. Running `coalesce --config api.coalesce` automatically outputs `api.md`.
- **Automatic Binary Handling**: Automatically detects binary files (by extension or null-byte inspection) and includes their path in the output without dumping raw binary content.
- **Dry Run Mode**: Preview which files will be included in the output without writing the file using `--dry-run`.

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

1. **Run Immediately (Zero Config)**

   In any project directory, simply run:

   ```sh
   coalesce
   ```

   This scans the current directory (automatically skipping `.git`, `bin`, `obj`, `node_modules`, `dist`, `artifacts`, lockfiles, etc.) and generates `coalesce.md`.

2. **Customize with `.coalesce` (Optional)**

   To create a starter `.coalesce` file in the current directory:

   ```sh
   coalesce init
   ```

   Edit `.coalesce` with plain lines to exclude items, or `!` lines to narrow what gets included:

   ```gitignore
   # Only scan Coalesce/ and Coalesce.Tests/, and only include .cs and .csproj files
   !Coalesce/
   !Coalesce.Tests/
   !*.cs
   !*.csproj
   ```

---

## 🛠️ Usage

### The `merge` Command

This is the default command. It merges files using `.coalesce` in the current directory if present, or built-in defaults if no `.coalesce` file exists.

**Syntax**

```sh
coalesce [options]
```

**Options**

| Option              | Description                                                                                        |
| ------------------- | -------------------------------------------------------------------------------------------------- |
| `--config <CONFIG>` | Path to a configuration file. Output is named `<config-name>.md` (defaults to `.coalesce` → `coalesce.md`). |
| `--dry-run`         | Simulates a merge, printing which files would be included without writing the output file.         |
| `--quiet`           | Suppresses all informational output. Only warnings and errors will be displayed.                   |
| `--verbose`         | Enables detailed output, showing why files are skipped and how configuration is applied.           |
| `-h`, `--help`      | Show help and usage information.                                                                   |

**Examples**

```sh
# Run using .coalesce in the current directory (or zero-config defaults) -> outputs coalesce.md
coalesce

# Preview which files would be merged without writing the output file
coalesce --dry-run

# Run using a custom profile -> outputs api.md
coalesce --config api.coalesce
```

### Other Commands

#### `init`

Generates a starter `.coalesce` configuration file in the current directory.

```sh
coalesce init
```

#### `install` / `uninstall`

Manages the application's presence in the system PATH.

- On **Windows**, this command modifies the user's PATH automatically.
- On **macOS/Linux**, this command displays manual instructions for updating your PATH.

---

## 📝 Configuration (`.coalesce`)

`.coalesce` is a plain-text file with one rule per line (`#` starts a comment):

- **Plain lines exclude** directories, extensions, or files on top of the built-in defaults:
  ```gitignore
  # Exclude extra folders, extensions, or specific files
  docs/
  benchmarks/
  *.md
  publish.ps1
  ```
- **Lines starting with `!` (or `+`) include** specific directories or extensions:
  - If any `!dir/` lines are present, Coalesce scans only those directories instead of `.`.
  - If any `!*.ext` lines are present, Coalesce includes only files with those extensions.
  ```gitignore
  # Only include C# and project files from src/
  !src/
  !*.cs
  !*.csproj

  # Still exclude specific files or folders inside src/
  Generated/
  AssemblyInfo.cs
  ```

**Built-in Exclusions**

Coalesce always skips the following automatically:
- **Directories**: `.git`, `.vs`, `.idea`, `.vscode`, `bin`, `obj`, `artifacts`, `packages`, `TestResults`, `node_modules`, `dist`, `build`, `.next`, `.nuxt`, `.cache`, `coverage`, `__pycache__`, `.venv`, `venv`, `target`
- **Files**: The output `.md` file itself, `.coalesce` (and the active `--config` file), `.DS_Store`, `Thumbs.db`, and common lockfiles (`package-lock.json`, `yarn.lock`, `pnpm-lock.yaml`, `bun.lock`, `Cargo.lock`, `poetry.lock`, `uv.lock`, `packages.lock.json`)

---

## 🤝 Contributing

Contributions are very welcome! If you encounter a bug, have an idea for a new feature, or want to improve the code, please feel free to:

- 🐛 **Open an Issue**: Report bugs or suggest new features.
- 🧑‍💻 **Submit a Pull Request**: Offer improvements, fixes, or new functionality.

---

## 📜 License

This project is licensed under the MIT License.
