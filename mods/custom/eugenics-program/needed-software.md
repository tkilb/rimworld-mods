# Required Software — Eugenics Program Mod

## Build Toolchain

### .NET SDK (Required to compile C# assembly)

The mod's C# source targets **`net472`** (`.NET Framework 4.7.2`), which is what RimWorld's engine uses.
On Linux, this is built via the **Mono-compatible .NET SDK** or the **full .NET SDK** with the legacy framework target pack.

**Option A — .NET SDK (Recommended, Arch Linux)**
```bash
# Install via pacman (AUR)
yay -S dotnet-sdk

# Or the specific LTS version
yay -S dotnet-sdk-8.0
```

> [!NOTE]
> The project targets `net472`. On Linux, `dotnet build` will use the `mono` framework compatibility shim
> automatically if the `Microsoft.NETFramework.ReferenceAssemblies` package is resolved.
> No additional target pack download is usually needed — NuGet handles it on first build.

**Option B — Mono (Alternative)**
```bash
# Mono includes msbuild and can build net472 directly
sudo pacman -S mono

# Build with:
msbuild Source/EugenicsProgram.csproj /p:Configuration=Release
```

---

## RimWorld Managed Assemblies (Required for compile references)

The `.csproj` references RimWorld's own DLLs. The project auto-detects them at:

| Priority | Path |
|---|---|
| 1 | `$RIMWORLD_MANAGED_DIR` env var (override) |
| 2 | `/mnt/gaming/SteamLibrary/steamapps/common/RimWorld/RimWorldLinux_Data/Managed` |
| 3 | `~/.local/share/Steam/steamapps/common/RimWorld/RimWorldLinux_Data/Managed` |

If RimWorld is installed in a non-standard path, set the env var before building:
```bash
export RIMWORLD_MANAGED_DIR="/path/to/RimWorld/RimWorldLinux_Data/Managed"
make build-mod MOD=eugenics-program
```

Required DLLs (must be present in the Managed directory):
- `Assembly-CSharp.dll` — RimWorld game assembly
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `0Harmony.dll` — Harmony patching library (from the Harmony mod)

> [!IMPORTANT]
> `0Harmony.dll` is **not** in the base RimWorld install. It is provided by the
> [Harmony mod](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077).
> Copy it from the mod's `Assemblies/` folder into the Managed directory, or point
> `$RIMWORLD_MANAGED_DIR` at a directory where you have manually placed it.
>
> Typical location after subscribing via Steam:
> `~/.local/share/Steam/steamapps/workshop/content/294100/2009463077/Assemblies/0Harmony.dll`

---

## Build Commands

Once the toolchain is installed:

```bash
# Dry run (shows what would be built, no changes)
make build-mod-dry-run MOD=eugenics-program

# Full build — compiles and outputs EugenicsProgram.dll to mods/custom/eugenics-program/Assemblies/
make build-mod MOD=eugenics-program
```

---

## Quick Install Checklist

| Step | Command | Status |
|---|---|---|
| Install .NET SDK | `yay -S dotnet-sdk` | ☐ |
| Verify dotnet | `dotnet --version` | ☐ |
| Confirm RimWorld is installed | `ls $RIMWORLD_MANAGED_DIR/Assembly-CSharp.dll` | ☐ |
| Locate or copy `0Harmony.dll` | see above | ☐ |
| Run dry-run build | `make build-mod-dry-run MOD=eugenics-program` | ☐ |
| Run full build | `make build-mod MOD=eugenics-program` | ☐ |
| Link mods into RimWorld | `make link` | ☐ |
