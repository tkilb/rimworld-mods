# Test Csharp

**Package ID:** `tylerkilburn.testcsharp`  
**Author:** tylerkilburn  

## Overview

A custom RimWorld mod (csharp).

## Structure

- `About/`: Metadata and mod dependencies (`About.xml`).
- `Defs/`: XML definitions (Items, Buildings, PawnKinds, Recipes, etc.).
- `Source/`: C# source code and project file (`TestCsharp.csproj`).
- `Assemblies/`: Compiled .NET assembly output (`TestCsharp.dll`).
- `Textures/`: (Optional) Mod graphics and icons.
- `Sounds/`: (Optional) Mod audio clips.
- `Patches/`: (Optional) XML patches.

## Building the C# Assembly

Ensure the .NET SDK (`dotnet`) is installed. Build with:

```bash
make build-mod MOD=test-csharp
```

Or build manually inside `Source/`:

```bash
cd Source
dotnet build -c Release
```
