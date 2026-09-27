# {{MOD_NAME}}

**Package ID:** `{{PACKAGE_ID}}`  
**Author:** {{AUTHOR}}  

## Overview

{{DESCRIPTION}}

## Structure

- `About/`: Metadata and mod dependencies (`About.xml`).
- `Defs/`: XML definitions (Items, Buildings, PawnKinds, Recipes, etc.).
- `Source/`: C# source code and project file (`{{PROJECT_NAME}}.csproj`).
- `Assemblies/`: Compiled .NET assembly output (`{{PROJECT_NAME}}.dll`).
- `Textures/`: (Optional) Mod graphics and icons.
- `Sounds/`: (Optional) Mod audio clips.
- `Patches/`: (Optional) XML patches.

## Building the C# Assembly

Ensure the .NET SDK (`dotnet`) is installed. Build with:

```bash
make build-mod MOD={{MOD_NAME_LOWER}}
```

Or build manually inside `Source/`:

```bash
cd Source
dotnet build -c Release
```
