# Coalesce

A CLI tool that merges a codebase into a single Markdown file for LLM context.

## Usage

Run `coalesce` in any project directory to scan `.` and generate `coalesce.md`:

```sh
coalesce
```

To customize what gets included or excluded, create a `.coalesce` file:

```sh
coalesce init
```

### Options

| Option              | Description                                                                                                  |
| ------------------- | ------------------------------------------------------------------------------------------------------------ |
| `--config <CONFIG>` | Path to a configuration file. Output is named `<config-name>.md` (defaults to `.coalesce` → `coalesce.md`). |
| `--dry-run`         | Preview which files would be merged without writing the output file.                                         |
| `--quiet`           | Suppress informational output (show only warnings and errors).                                               |
| `--verbose`         | Show detailed output, including skipped files and applied rules.                                             |
| `-h`, `--help`      | Show help and usage information.                                                                             |

```sh
# Preview eligible files without writing coalesce.md
coalesce --dry-run

# Merge using a custom profile -> outputs api.md
coalesce --config api.coalesce
```

## Configuration (`.coalesce`)

`.coalesce` is a plain-text file with one rule per line (`#` starts a comment):

- **Plain lines exclude** directories (`docs/`), extensions (`*.log`), or files (`publish.ps1`).
- **Lines starting with `!` (or `+`) include** specific directories (`!src/`) or extensions (`!*.cs`).
  - If any `!dir/` rules are present, only those directories are scanned instead of `.`.
  - If any `!*.ext` rules are present, only files with those extensions are included.

```gitignore
# Only scan src/ and tests/, and only include .cs and .csproj files
!src/
!tests/
!*.cs
!*.csproj

# Still exclude specific directories or files inside them
Generated/
AssemblyInfo.cs
```

### Built-in Defaults

- **Auto-excluded directories**: `.git`, `.vs`, `.idea`, `.vscode`, `bin`, `obj`, `artifacts`, `packages`, `TestResults`, `node_modules`, `dist`, `build`, `.next`, `.nuxt`, `.cache`, `coverage`, `__pycache__`, `.venv`, `venv`, `target`
- **Auto-excluded files**: The output `.md` file, `.coalesce` (and the active `--config` file), `.DS_Store`, `Thumbs.db`, and common lockfiles (`package-lock.json`, `yarn.lock`, `pnpm-lock.yaml`, `bun.lock`, `Cargo.lock`, `poetry.lock`, `uv.lock`, `packages.lock.json`)
- **Binary files**: Automatically detected (by extension or null-byte check) and listed by path only without raw binary content.

## License

MIT
