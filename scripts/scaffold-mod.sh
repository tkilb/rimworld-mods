#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=scripts/common.sh
source "$SCRIPT_DIR/common.sh"

MOD_NAME=""
MOD_TYPE="xml"
PACKAGE_ID=""
AUTHOR=""
DESCRIPTION=""
DRY_RUN=false

show_help() {
  echo "Usage: $0 [OPTIONS] [MOD_NAME]"
  echo ""
  echo "Scaffold a new custom RimWorld mod into mods/custom/<MOD_NAME>."
  echo ""
  echo "Options:"
  echo "  --name, -n NAME         Mod directory name (e.g. my-awesome-mod)"
  echo "  --type, -t TYPE         Mod archetype: 'xml' or 'csharp' (default: 'xml')"
  echo "  --package-id, -p ID     Unique packageId (default: <author>.<mod_name>)"
  echo "  --author, -a AUTHOR     Author name (default: current user or git user)"
  echo "  --description, -d DESC  Short mod description"
  echo "  --dry-run               Preview files to be created without writing to disk"
  echo "  --help, -h              Display this help message"
  exit 0
}

# Parse flags
while [[ $# -gt 0 ]]; do
  case "$1" in
    --name|-n)
      MOD_NAME="$2"
      shift 2
      ;;
    --type|-t)
      MOD_TYPE="$2"
      shift 2
      ;;
    --package-id|-p)
      PACKAGE_ID="$2"
      shift 2
      ;;
    --author|-a)
      AUTHOR="$2"
      shift 2
      ;;
    --description|-d)
      DESCRIPTION="$2"
      shift 2
      ;;
    --dry-run)
      DRY_RUN=true
      shift
      ;;
    --help|-h)
      show_help
      ;;
    *)
      if [[ -z "$MOD_NAME" ]]; then
        MOD_NAME="$1"
        shift
      else
        log_err "Unknown argument: $1"
        exit 1
      fi
      ;;
  esac
done

if [[ -z "$MOD_NAME" ]]; then
  log_err "Mod name is required. Specify via --name <name> or as first argument."
  echo "Example: $0 --name my-mod --type csharp"
  exit 1
fi

# Sanitize mod directory name: lowercase alphanumeric, dashes, and underscores
if [[ ! "$MOD_NAME" =~ ^[a-zA-Z0-9_-]+$ ]]; then
  log_err "Invalid mod name '$MOD_NAME'. Only letters, numbers, hyphens, and underscores are allowed."
  exit 1
fi

# Validate archetype
if [[ "$MOD_TYPE" != "xml" && "$MOD_TYPE" != "csharp" ]]; then
  log_err "Invalid mod type '$MOD_TYPE'. Must be either 'xml' or 'csharp'."
  exit 1
fi

TEMPLATES_DIR="$REPO_ROOT/templates/custom-mod/$MOD_TYPE"
if [[ ! -d "$TEMPLATES_DIR" ]]; then
  log_err "Template directory not found: $TEMPLATES_DIR"
  exit 1
fi

TARGET_DIR="$REPO_ROOT/mods/custom/$MOD_NAME"

# Default author
if [[ -z "$AUTHOR" ]]; then
  AUTHOR="$(git config user.name 2>/dev/null || true)"
  if [[ -z "$AUTHOR" ]]; then
    AUTHOR="${USER:-author}"
  fi
fi

# Default description
if [[ -z "$DESCRIPTION" ]]; then
  DESCRIPTION="A custom RimWorld mod ($MOD_TYPE)."
fi

# Compute PascalCase project name (e.g. 'my-cool-mod' -> 'MyCoolMod')
to_pascal_case() {
  local input="$1"
  python3 -c "import sys, re; s = sys.argv[1]; print(''.join(w.capitalize() for w in re.split(r'[-_ ]+', s) if w))" "$input"
}

# Compute Title Case display name (e.g. 'my-cool-mod' -> 'My Cool Mod')
to_title_case() {
  local input="$1"
  python3 -c "import sys, re; s = sys.argv[1]; print(' '.join(w.capitalize() for w in re.split(r'[-_ ]+', s) if w))" "$input"
}

# Compute alphanumeric lower identifier (e.g. 'my-cool-mod' -> 'mycoolmod')
to_clean_id() {
  local input="$1"
  python3 -c "import sys, re; s = sys.argv[1]; print(re.sub(r'[^a-zA-Z0-9]', '', s).lower())" "$input"
}

PROJECT_NAME="$(to_pascal_case "$MOD_NAME")"
DISPLAY_NAME="$(to_title_case "$MOD_NAME")"
CLEAN_MOD_ID="$(to_clean_id "$MOD_NAME")"
AUTHOR_CLEAN="$(to_clean_id "$AUTHOR")"

if [[ -z "$PACKAGE_ID" ]]; then
  PACKAGE_ID="${AUTHOR_CLEAN}.${CLEAN_MOD_ID}"
fi

log_info "Scaffolding custom mod:"
echo "  Target Path:   $TARGET_DIR"
echo "  Mod Type:      $MOD_TYPE"
echo "  Display Name:  $DISPLAY_NAME"
echo "  Project Name:  $PROJECT_NAME"
echo "  Package ID:    $PACKAGE_ID"
echo "  Author:        $AUTHOR"

if [[ -d "$TARGET_DIR" ]]; then
  log_err "Target directory already exists: $TARGET_DIR"
  exit 1
fi

if $DRY_RUN; then
  log_info "[DRY-RUN] Previewing files to create in $TARGET_DIR:"
  while IFS= read -r -d '' src_file; do
    rel_path="${src_file#$TEMPLATES_DIR/}"
    # Replace Project and ModMain in relative path for preview
    rel_path="${rel_path//Project.csproj/${PROJECT_NAME}.csproj}"
    rel_path="${rel_path//ModMain.cs/${PROJECT_NAME}Mod.cs}"
    echo "  [DRY-RUN] Would create: mods/custom/$MOD_NAME/$rel_path"
  done < <(find "$TEMPLATES_DIR" -type f -print0)
  if [[ "$MOD_TYPE" == "csharp" ]]; then
    echo "  [DRY-RUN] Would create directory: mods/custom/$MOD_NAME/Assemblies"
  fi
  log_succ "[DRY-RUN] Dry run complete. No files written."
  exit 0
fi

# Live creation
log_info "Creating directories..."
mkdir -p "$TARGET_DIR"

if [[ "$MOD_TYPE" == "csharp" ]]; then
  mkdir -p "$TARGET_DIR/Assemblies"
fi

# Copy template files and substitute placeholders
while IFS= read -r -d '' src_file; do
  rel_path="${src_file#$TEMPLATES_DIR/}"
  dest_rel="${rel_path//Project.csproj/${PROJECT_NAME}.csproj}"
  dest_rel="${dest_rel//ModMain.cs/${PROJECT_NAME}Mod.cs}"
  dest_file="$TARGET_DIR/$dest_rel"

  mkdir -p "$(dirname "$dest_file")"

  # Substitute template variables
  python3 - "$src_file" "$dest_file" "$DISPLAY_NAME" "$PACKAGE_ID" "$AUTHOR" "$DESCRIPTION" "$PROJECT_NAME" "$MOD_NAME" << 'EOF'
import sys

src_path = sys.argv[1]
dest_path = sys.argv[2]
display_name = sys.argv[3]
package_id = sys.argv[4]
author = sys.argv[5]
description = sys.argv[6]
project_name = sys.argv[7]
mod_name = sys.argv[8]

with open(src_path, "r", encoding="utf-8") as f:
    content = f.read()

replacements = {
    "{{MOD_NAME}}": display_name,
    "{{PACKAGE_ID}}": package_id,
    "{{AUTHOR}}": author,
    "{{DESCRIPTION}}": description,
    "{{PROJECT_NAME}}": project_name,
    "{{MOD_NAME_PASCAL}}": project_name,
    "{{MOD_NAME_LOWER}}": mod_name,
}

for placeholder, val in replacements.items():
    content = content.replace(placeholder, val)

with open(dest_path, "w", encoding="utf-8") as f:
    f.write(content)
EOF

  log_succ "Created: mods/custom/$MOD_NAME/$dest_rel"
done < <(find "$TEMPLATES_DIR" -type f -print0)

log_succ "Successfully scaffolded '$MOD_NAME' at $TARGET_DIR"
echo ""
echo "Next steps:"
echo "  1. Review metadata in mods/custom/$MOD_NAME/About/About.xml"
if [[ "$MOD_TYPE" == "csharp" ]]; then
  echo "  2. Compile the mod: make build-mod MOD=$MOD_NAME"
fi
echo "  3. Deploy symlink to RimWorld: make link"
echo "  4. Check load order: make order-mods-dry-run"
