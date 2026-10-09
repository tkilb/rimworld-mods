#!/usr/bin/env python3
"""
scripts/import-deck-mods.py

Discovers Steam Workshop mods from a Steam Deck (or local workshop directory),
parses mod metadata (About.xml), resolves the dependency DAG, hides transitive
dependencies, and launches an interactive fzf TUI to cherry-pick mods into manifests/mods.yaml.
"""

import argparse
import os
import re
import shutil
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
from pathlib import Path

try:
    import yaml
except ImportError:
    print("Error: PyYAML is required. Install with: pip install pyyaml", file=sys.stderr)
    sys.exit(1)


OFFICIAL_PACKAGES = {
    "ludeon.rimworld",
    "ludeon.rimworld.royalty",
    "ludeon.rimworld.ideology",
    "ludeon.rimworld.biotech",
    "ludeon.rimworld.anomaly",
    "ludeon.rimworld.odyssey",
}


GENERIC_SLUG_NAMES = {"core", "mod", "base", "framework", "lib", "library", "patch", "redux"}

def sanitize_slug(package_id: str, existing_slugs: set) -> str:
    """Generate a clean, collision-free slug from a packageId."""
    parts = package_id.split(".")
    base = parts[-1].lower()
    base = re.sub(r"[^a-z0-9_-]", "", base)
    if (base in GENERIC_SLUG_NAMES or len(base) < 3) and len(parts) > 1:
        # Prefix with preceding part, e.g. VanillaFactionsExpanded.Core -> vanillafactionsexpandedcore
        prev = re.sub(r"[^a-z0-9_-]", "", parts[-2].lower())
        base = f"{prev}{base}"

    if not base or len(base) < 3:
        base = re.sub(r"[^a-z0-9_-]", "", package_id.lower())

    slug = base
    counter = 1
    while slug in existing_slugs:
        slug = f"{base}{counter}"
        counter += 1
    return slug


def parse_about_xml(xml_path: Path):
    """Parses an About.xml file and extracts metadata."""
    try:
        tree = ET.parse(xml_path)
        root = tree.getroot()
    except Exception as e:
        return None

    def get_text(tag: str) -> str:
        elem = root.find(tag)
        return elem.text.strip() if elem is not None and elem.text else ""

    name = get_text("name")
    package_id = get_text("packageId")
    author = get_text("author")
    description = get_text("description")

    dependencies = []
    # Check <modDependencies>
    for dep_li in root.findall(".//modDependencies/li"):
        pid = dep_li.find("packageId")
        dname = dep_li.find("displayName")
        if pid is not None and pid.text:
            dependencies.append({
                "package_id": pid.text.strip(),
                "display_name": dname.text.strip() if dname is not None and dname.text else ""
            })

    # Check <modDependenciesByVersion>
    for dep_li in root.findall(".//modDependenciesByVersion/*/li"):
        pid = dep_li.find("packageId")
        dname = dep_li.find("displayName")
        if pid is not None and pid.text:
            dep_id = pid.text.strip()
            if not any(d["package_id"].lower() == dep_id.lower() for d in dependencies):
                dependencies.append({
                    "package_id": dep_id,
                    "display_name": dname.text.strip() if dname is not None and dname.text else ""
                })

    return {
        "name": name,
        "package_id": package_id,
        "author": author,
        "description": description,
        "dependencies": dependencies,
    }


def find_workshop_about_files(base_dir: Path):
    """Scans a workshop content directory for each workshop_id and its About.xml."""
    mods = {}
    if not base_dir.is_dir():
        return mods

    for item_dir in base_dir.iterdir():
        if not item_dir.is_dir():
            continue
        workshop_id = item_dir.name
        if not workshop_id.isdigit():
            continue

        about_candidates = [
            item_dir / "About" / "About.xml",
            item_dir / "about" / "about.xml",
            item_dir / "About" / "about.xml",
        ]
        about_path = None
        for candidate in about_candidates:
            if candidate.is_file():
                about_path = candidate
                break

        if not about_path:
            # Recursive search with max depth 3
            for p in item_dir.glob("**/About.xml"):
                if p.is_file():
                    about_path = p
                    break

        if about_path:
            meta = parse_about_xml(about_path)
            if meta and meta.get("package_id"):
                meta["workshop_id"] = workshop_id
                meta["mod_dir"] = str(item_dir)
                mods[meta["package_id"].lower()] = meta

    return mods


def load_manifest(manifest_path: Path):
    """Loads existing manifests/mods.yaml."""
    if not manifest_path.is_file():
        return {"version": 1, "mods": {}}
    try:
        with open(manifest_path, "r", encoding="utf-8") as f:
            data = yaml.safe_load(f) or {}
            if "mods" not in data or data["mods"] is None:
                data["mods"] = {}
            return data
    except Exception as e:
        print(f"Error loading manifest: {e}", file=sys.stderr)
        return {"version": 1, "mods": {}}


def stream_deck_workshop_meta(deck_host: str, local_tmp_dir: Path, remote_dirs=None):
    """
    Connects to Steam Deck over SSH, locates RimWorld Workshop content (AppID 294100),
    and streams back only About/ folders and metadata into local_tmp_dir.
    """
    print(f"\033[34m[INFO]\033[0m Searching for RimWorld workshop mods on {deck_host}...")
    
    # Discovery script on Deck
    discovery_cmd = (
        'for d in "$HOME/.local/share/Steam/steamapps/workshop/content/294100" '
        '/run/media/*/steamapps/workshop/content/294100; do '
        '  if [ -d "$d" ]; then echo "$d"; fi; '
        'done; '
        'find /run/media -maxdepth 5 -type d -path "*/steamapps/workshop/content/294100" 2>/dev/null || true'
    )
    res = subprocess.run(
        ["ssh", "-o", "BatchMode=yes", "-o", "ConnectTimeout=10", deck_host, discovery_cmd],
        capture_output=True,
        text=True,
        check=False
    )
    if res.returncode != 0:
        print(f"\033[31m[ERROR]\033[0m Failed to query workshop directory on {deck_host}: {res.stderr.strip()}", file=sys.stderr)
        sys.exit(1)

    raw_dirs = [line.strip() for line in res.stdout.splitlines() if line.strip()]
    # Deduplicate while preserving order
    found_dirs = []
    for d in raw_dirs:
        if d not in found_dirs:
            found_dirs.append(d)

    if remote_dirs:
        found_dirs = remote_dirs

    if not found_dirs:
        print(f"\033[31m[ERROR]\033[0m No RimWorld workshop directory found on {deck_host} (checked default Steam & SD card paths).", file=sys.stderr)
        sys.exit(1)

    print(f"\033[32m[OK]\033[0m Located workshop content at: {', '.join(found_dirs)}")

    # Archive and stream About/ directories from remote deck to local temp dir
    mods = {}
    for remote_dir in found_dirs:
        print(f"\033[34m[INFO]\033[0m Fetching mod metadata from {remote_dir}...")
        tar_cmd = f"find {remote_dir} -maxdepth 4 -iname 'About.xml' -print0 2>/dev/null | tar -czf - --null -T -"
        ssh_proc = subprocess.Popen(
            ["ssh", "-o", "BatchMode=yes", deck_host, tar_cmd],
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE
        )
        untar_proc = subprocess.Popen(
            ["tar", "-xzf", "-", "-C", str(local_tmp_dir)],
            stdin=ssh_proc.stdout,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE
        )
        ssh_proc.stdout.close()
        untar_proc.communicate()
        ssh_proc.wait()

        # Search for About.xml files in local_tmp_dir for this remote_dir
        for about_file in local_tmp_dir.glob("**/About.xml"):
            meta = parse_about_xml(about_file)
            if not meta or not meta.get("package_id"):
                continue

            wid = None
            if "294100" in about_file.parts:
                idx = about_file.parts.index("294100")
                if idx + 1 < len(about_file.parts) and about_file.parts[idx + 1].isdigit():
                    wid = about_file.parts[idx + 1]

            if not wid:
                # Search upwards from About.xml for the first numeric directory that is NOT 294100
                for part in reversed(about_file.parts):
                    if part.isdigit() and part != "294100":
                        wid = part
                        break

            if wid and meta["package_id"].lower() not in mods:
                meta["workshop_id"] = wid
                meta["mod_dir"] = f"{remote_dir}/{wid}"
                mods[meta["package_id"].lower()] = meta

    return mods, found_dirs


def build_dependency_graph(mods: dict):
    """
    Computes which mods are dependencies of other installed mods.
    Returns (transitive_set, dependency_map).
    """
    all_dependencies = set()
    for mod in mods.values():
        for dep in mod.get("dependencies", []):
            dep_pid = dep["package_id"].lower()
            if dep_pid not in OFFICIAL_PACKAGES:
                all_dependencies.add(dep_pid)

    return all_dependencies


def run_fzf_picker(candidates: list, previews_dir: Path):
    """
    Launches fzf with multi-selection and a rich preview pane.
    Returns list of selected package_ids.
    """
    fzf_bin = shutil.which("fzf")
    if not fzf_bin:
        print("Error: fzf is not installed.", file=sys.stderr)
        sys.exit(1)

    # Prepare input lines: Friendly Name  |  package_id  |  [Workshop: id]
    lines = []
    for mod in candidates:
        name = mod["name"]
        pkg = mod["package_id"]
        wid = mod["workshop_id"]
        # Format columns cleanly
        lines.append(f"{name:45} │ {pkg:35} │ [Workshop: {wid}]")

    input_text = "\n".join(lines)

    preview_cmd = (
        f"python3 -c '"
        f"import sys, os, re; "
        f"line = sys.argv[1]; "
        f"parts = [p.strip() for p in line.split(\"│\")]; "
        f"pkg = parts[1] if len(parts) > 1 else \"\"; "
        f"p = os.path.join(\"{previews_dir}\", pkg.lower() + \".txt\"); "
        f"print(open(p).read() if os.path.exists(p) else \"No preview\");"
        f"' {{}}"
    )

    fzf_args = [
        fzf_bin,
        "-m",
        "--ansi",
        "--bind=space:toggle+down",
        "--header=Select mods to import [Space: toggle selection | Enter: confirm | Esc: cancel]",
        "--delimiter=│",
        f"--preview={preview_cmd}",
        "--preview-window=right:50%:wrap",
        "--prompt=Cherry-pick mods > ",
    ]

    proc = subprocess.run(fzf_args, input=input_text, text=True, capture_output=True)
    if proc.returncode != 0:
        # User canceled (ESC / Ctrl-C)
        return []

    selected_pkg_ids = []
    for line in proc.stdout.splitlines():
        if "│" in line:
            parts = [p.strip() for p in line.split("│")]
            if len(parts) >= 2:
                selected_pkg_ids.append(parts[1].lower())

    return selected_pkg_ids


def generate_preview_files(mods: dict, previews_dir: Path, transitive_set: set):
    """Generates preview text files for fzf."""
    previews_dir.mkdir(parents=True, exist_ok=True)
    for pkg_lower, mod in mods.items():
        preview_path = previews_dir / f"{pkg_lower}.txt"
        lines = []
        lines.append(f"\033[1;36m{mod['name']}\033[0m")
        lines.append("─" * 50)
        lines.append(f"\033[1mPackage ID:\033[0m   {mod['package_id']}")
        lines.append(f"\033[1mWorkshop ID:\033[0m  {mod['workshop_id']}")
        if mod.get("author"):
            lines.append(f"\033[1mAuthor:\033[0m       {mod['author']}")
        lines.append(f"\033[1mSteam URL:\033[0m    https://steamcommunity.com/sharedfiles/filedetails/?id={mod['workshop_id']}")

        deps = mod.get("dependencies", [])
        if deps:
            lines.append("\n\033[1;33mDependencies:\033[0m")
            for d in deps:
                d_pkg = d["package_id"]
                d_name = d.get("display_name") or d_pkg
                on_deck = d_pkg.lower() in mods
                is_official = d_pkg.lower() in OFFICIAL_PACKAGES
                if is_official:
                    status = "\033[32m[Official DLC/Core]\033[0m"
                elif on_deck:
                    status = "\033[32m[Available on Deck - will auto-import]\033[0m"
                else:
                    status = "\033[31m[External/Missing from Deck]\033[0m"
                lines.append(f"  • {d_name} ({d_pkg}) {status}")
        else:
            lines.append("\n\033[1;32mNo dependencies required\033[0m")

        if pkg_lower in transitive_set:
            lines.append("\n\033[35m[Note: This is a transitive dependency of another mod]\033[0m")

        desc = mod.get("description", "").strip()
        if desc:
            lines.append("\n\033[1mDescription:\033[0m")
            lines.append(desc[:1000] + ("..." if len(desc) > 1000 else ""))

        preview_path.write_text("\n".join(lines), encoding="utf-8")


def resolve_transitive_closure(selected_pkgs: list, mods: dict):
    """
    Computes the full transitive closure of dependencies for the selected mods.
    Returns (final_mods_to_import, auto_included_dependencies).
    """
    to_import = {}
    auto_included = []

    queue = list(selected_pkgs)
    visited = set()

    while queue:
        pkg = queue.pop(0)
        if pkg in visited:
            continue
        visited.add(pkg)

        mod = mods.get(pkg)
        if not mod:
            continue

        to_import[pkg] = mod
        if pkg not in selected_pkgs:
            auto_included.append(mod)

        for dep in mod.get("dependencies", []):
            dep_pkg = dep["package_id"].lower()
            if dep_pkg in OFFICIAL_PACKAGES:
                continue
            if dep_pkg in mods and dep_pkg not in visited:
                queue.append(dep_pkg)

    return to_import, auto_included


def update_manifest(manifest_path: Path, mods_to_add: dict, dry_run: bool = False):
    """Adds the selected mods into manifests/mods.yaml under 'mods:' section."""
    manifest_data = load_manifest(manifest_path)
    existing_mods = manifest_data.get("mods", {}) or {}

    existing_slugs = set(existing_mods.keys())
    existing_package_ids = {m.get("package_id", "").lower() for m in existing_mods.values() if isinstance(m, dict)}
    existing_workshop_ids = {str(m.get("workshop_id", "")) for m in existing_mods.values() if isinstance(m, dict)}

    new_entries = {}
    for pkg_lower, mod in mods_to_add.items():
        if pkg_lower in existing_package_ids or str(mod["workshop_id"]) in existing_workshop_ids:
            continue

        slug = sanitize_slug(mod["package_id"], existing_slugs | set(new_entries.keys()))
        new_entries[slug] = {
            "name": mod["name"],
            "source": "steam_workshop",
            "workshop_id": str(mod["workshop_id"]),
            "package_id": mod["package_id"],
            "enabled": True,
            "description": (mod.get("description") or "").replace("\n", " ").strip()[:300],
        }

    if not new_entries:
        print("\033[33m[WARN]\033[0m All selected mods (and dependencies) are already present in manifests/mods.yaml.")
        return False

    print("\n\033[1;32mPlanned additions to manifests/mods.yaml:\033[0m")
    for slug, entry in new_entries.items():
        print(f"  + \033[1;36m{slug}\033[0m: \"{entry['name']}\" (ID: {entry['workshop_id']}, Package: {entry['package_id']})")

    if dry_run:
        print("\n\033[34m[DRY-RUN]\033[0m Dry-run enabled. mods.yaml was not modified.")
        return True

    # Read original text to preserve comments and format
    content = manifest_path.read_text(encoding="utf-8") if manifest_path.is_file() else "version: 1\nmods:\n"
    
    # Generate YAML block to append
    yaml_lines = []
    for slug, entry in new_entries.items():
        desc_escaped = entry["description"].replace('"', '\\"')
        yaml_lines.append(f"  {slug}:")
        yaml_lines.append(f"    name: \"{entry['name']}\"")
        yaml_lines.append(f"    source: {entry['source']}")
        yaml_lines.append(f"    workshop_id: \"{entry['workshop_id']}\"")
        yaml_lines.append(f"    package_id: \"{entry['package_id']}\"")
        yaml_lines.append(f"    enabled: true")
        if desc_escaped:
            yaml_lines.append(f"    description: \"{desc_escaped}\"")
        yaml_lines.append("")

    formatted_block = "\n".join(yaml_lines)

    # Append to mods: block or end of file
    if "\nmods:" in content:
        # Find where mods: starts
        pos = content.find("\nmods:\n")
        if pos != -1:
            insert_pos = pos + len("\nmods:\n")
            updated_content = content[:insert_pos] + formatted_block + content[insert_pos:]
        else:
            updated_content = content + "\n" + formatted_block
    else:
        updated_content = content + "\nmods:\n" + formatted_block

    manifest_path.write_text(updated_content, encoding="utf-8")
    print(f"\033[32m[OK]\033[0m Successfully updated {manifest_path} with {len(new_entries)} mod(s)!")
    return True


def copy_workshop_files_from_deck(deck_host: str, mods_to_copy: dict, repo_root: Path, dry_run: bool = False):
    """Directly rsyncs mod folders from Steam Deck to local mods/vendor/."""
    vendor_dir = repo_root / "mods" / "vendor"
    manifest_data = load_manifest(repo_root / "manifests" / "mods.yaml")
    mods_declared = manifest_data.get("mods", {})

    # Map package_id to slug
    pkg_to_slug = {}
    for slug, m in mods_declared.items():
        if isinstance(m, dict) and "package_id" in m:
            pkg_to_slug[m["package_id"].lower()] = slug

    for pkg_lower, mod in mods_to_copy.items():
        slug = pkg_to_slug.get(pkg_lower)
        if not slug:
            continue
        dest_dir = vendor_dir / slug
        mod_src = mod.get("mod_dir")
        if not mod_src:
            continue

        print(f"\033[34m[INFO]\033[0m Syncing files for {mod['name']} -> {dest_dir}...")
        if dry_run:
            print(f"  [DRY-RUN] Would rsync from {deck_host}:{mod_src}/ to {dest_dir}/")
        else:
            dest_dir.mkdir(parents=True, exist_ok=True)
            rsync_cmd = ["rsync", "-avz", "--delete", f"{deck_host}:{mod_src}/", f"{dest_dir}/"]
            subprocess.run(rsync_cmd, check=True)
            print(f"\033[32m[OK]\033[0m Synced {slug} successfully.")


def main():
    parser = argparse.ArgumentParser(description="Cherry-pick Steam Workshop mods from Steam Deck into repo manifest.")
    parser.add_argument("--deck-host", default=os.getenv("DECK_HOST", "steamdeck"), help="SSH hostname for Steam Deck")
    parser.add_argument("--local-dir", help="Scan a local workshop directory instead of remote Steam Deck")
    parser.add_argument("--dry-run", action="store_true", help="Preview modifications without writing to disk")
    parser.add_argument("--show-all", action="store_true", help="Show all mods including transitive dependencies in TUI")
    parser.add_argument("--copy-files", action="store_true", help="Copy mod files directly from Steam Deck into mods/vendor/")
    parser.add_argument("--repo-root", default=str(Path(__file__).resolve().parent.parent), help="Root directory of repository")
    args = parser.parse_args()

    repo_root = Path(args.repo_root)
    manifest_path = repo_root / "manifests" / "mods.yaml"

    tmp_dir = Path(tempfile.mkdtemp(prefix="rimworld_deck_mods_"))
    previews_dir = tmp_dir / "previews"

    try:
        if args.local_dir:
            print(f"\033[34m[INFO]\033[0m Scanning local workshop directory: {args.local_dir}")
            mods = find_workshop_about_files(Path(args.local_dir))
        else:
            mods, found_dirs = stream_deck_workshop_meta(args.deck_host, tmp_dir)

        if not mods:
            print("\033[33m[WARN]\033[0m No valid RimWorld workshop mods with About.xml found.")
            return

        print(f"\033[32m[OK]\033[0m Discovered {len(mods)} workshop mods.")

        # Build dependency DAG
        transitive_set = build_dependency_graph(mods)

        # Load existing manifest to filter out already added mods
        existing_manifest = load_manifest(manifest_path)
        existing_mods = existing_manifest.get("mods", {})
        existing_package_ids = {m.get("package_id", "").lower() for m in existing_mods.values() if isinstance(m, dict)}
        existing_workshop_ids = {str(m.get("workshop_id", "")) for m in existing_mods.values() if isinstance(m, dict)}

        # Filter candidates for cherry-picking
        candidates = []
        for pkg_lower, mod in sorted(mods.items(), key=lambda x: x[1]["name"].lower()):
            # Filter already imported mods
            if pkg_lower in existing_package_ids or str(mod["workshop_id"]) in existing_workshop_ids:
                continue

            # Hide transitive dependencies unless --show-all is specified
            if not args.show_all and pkg_lower in transitive_set:
                continue

            candidates.append(mod)

        if not candidates:
            print("\033[33m[INFO]\033[0m All discoverable mods are already declared in manifests/mods.yaml (or are hidden transitive dependencies).")
            if not args.show_all and transitive_set:
                print(f"  (\033[33m{len(transitive_set)}\033[0m transitive dependencies are hidden. Use --show-all to display everything).")
            return

        # Generate previews
        generate_preview_files(mods, previews_dir, transitive_set)

        # Launch fzf picker
        selected_pkgs = run_fzf_picker(candidates, previews_dir)
        if not selected_pkgs:
            print("\033[33m[INFO]\033[0m No mods selected. Operation canceled.")
            return

        # Resolve transitive closure for selected mods
        to_import, auto_included = resolve_transitive_closure(selected_pkgs, mods)

        print(f"\n\033[1mSelected mods for import:\033[0m")
        for pkg in selected_pkgs:
            m = mods[pkg]
            print(f"  • \033[1;32m{m['name']}\033[0m ({m['package_id']})")

        if auto_included:
            print(f"\n\033[1mAutomatically included dependencies:\033[0m")
            for m in auto_included:
                print(f"  + \033[1;35m{m['name']}\033[0m ({m['package_id']})")

        # Update manifests/mods.yaml
        manifest_updated = update_manifest(manifest_path, to_import, dry_run=args.dry_run)

        # Handle file copying if requested
        if manifest_updated and args.copy_files and not args.dry_run:
            copy_workshop_files_from_deck(args.deck_host, to_import, repo_root, dry_run=args.dry_run)
        elif manifest_updated and not args.dry_run:
            print("\n\033[36mNext steps:\033[0m")
            print("  1. Run \033[1mmake fetch-mods\033[0m to download and lock the new mods.")
            print("  2. Run \033[1mmake order-mods\033[0m to compute and verify the new load order.")
            print("  3. Run \033[1mmake link\033[0m to link them into your active RimWorld installation.")

    finally:
        shutil.rmtree(tmp_dir, ignore_errors=True)


if __name__ == "__main__":
    main()
