# Steam Deck Setup & Remote Synchronization Guide

This guide covers setting up SSH on your Steam Deck (SteamOS), configuring your desktop's `~/.ssh/config`, and syncing the RimWorld monorepo from your Arch Linux desktop.

---

## 1. Enable SSH on the Steam Deck

1. On the Steam Deck, switch to **Desktop Mode** (STEAM → Power → Switch to Desktop).
2. Open **Konsole** and set a password for the `deck` account if you haven't already:
   ```bash
   passwd
   ```
3. Start (and optionally enable on boot) the SSH daemon:
   ```bash
   sudo systemctl enable --now sshd
   ```

---

## 2. Configure SSH on Your Desktop (`~/.ssh/config`)

All connection details (user, port, key) live in your `~/.ssh/config` on the **desktop**. The monorepo scripts use `DECK_HOST` as an SSH alias and delegate everything else to this config.

Add a block like the following to `~/.ssh/config`:

```sshconfig
Host steamdeck
    HostName <ip-or-hostname>   # Steam Deck LAN IP or mDNS hostname (e.g. steamdeck.local)
    User deck
    Port 22
    IdentityFile ~/.ssh/id_ed25519
```

Copy your public key to the Deck so you can connect passwordlessly:
```bash
ssh-copy-id steamdeck
```

Verify:
```bash
ssh steamdeck echo "connection OK"
```

Once this works, the monorepo needs no further SSH configuration.

---

## 3. RimWorld Mod Directory Paths on Steam Deck

The correct `RIMWORLD_MODS_DIR` depends on where RimWorld is installed on the Deck:

| Location | Path |
|---|---|
| Internal Storage (default) | `~/.local/share/Steam/steamapps/common/RimWorld/Mods` |
| MicroSD Card | `/run/media/mmcblk0p1/steamapps/common/RimWorld/Mods` |

The `steam-deck` machine profile in `scripts/common.sh` uses the internal path by default. If you're on a MicroSD, create `config/local.env` **on the Deck** with the correct path:

```bash
# On the Deck, after syncing the repo
echo 'RIMWORLD_MODS_DIR="/run/media/mmcblk0p1/steamapps/common/RimWorld/Mods"' \
  > ~/rimworld-mods/config/local.env
```

> `config/local.env` is gitignored — it won't be overwritten on subsequent syncs.

---

## 4. Desktop Configuration

The only monorepo config needed on the **desktop** is `DECK_HOST` (your SSH alias) and optionally `DECK_REMOTE_DIR`. Copy the example:

```bash
cp config/config.env.example config/local.env
```

The defaults are already correct if your SSH alias is `steamdeck` and you want the repo at `~/rimworld-mods` on the Deck.

---

## 5. Remote Sync Workflows

All sync commands run from your **desktop**.

### Test SSH Connectivity
```bash
make sync-deck-check
```

### Preview File Transfer (Dry-Run)
```bash
make sync-deck-dry-run
```

### Sync Repository to Steam Deck
Transfers `manifests/`, `mods/vendor/`, `mods/custom/`, `scripts/`, `Makefile`, and docs. Excludes `config/local.env`, `.git/`, and temp files.
```bash
make sync-deck
```

### Sync + Automatically Deploy Symlinks
Sync files, then run `make link` on the Deck in one step:
```bash
make sync-deck-link
```

---

## 6. Running Monorepo Commands Directly on the Deck

After syncing, you can SSH in and run any `make` target natively:

```bash
ssh steamdeck
cd ~/rimworld-mods

make status    # Inspect mod/symlink status
make link      # Deploy symlinks into RimWorld Mods directory
make unlink    # Remove monorepo symlinks safely
```
