# presets for Coalesce

This directory is for your custom `coalesce init` presets.

### 🛠️ How to Use Presets

Run the `init` command with the `--preset` option:

```sh
coalesce init --preset <name>
```

For example, to use the Node.js preset:

```sh
coalesce init --preset node
```

### 💡 Available Presets

Built-in presets include:

- `dotnet`
- `node`

### ✨ Create Your Own

To create a new preset, copy the `_template.yaml` file in this directory to a new file (e.g., `my-project.yaml`) and customize it to your needs.

Then, you can use it like any other preset:

```sh
coalesce init --preset my-project
```

---

> 👉 **Heads up:** Your custom presets in this folder will override any built-in presets that have the same name.