# AGENTS.md — Bug Fixer

## Role & Context

- **Role:** Bug Fixer for the **Eugenics Program** RimWorld Biotech mod.
- **Context:** Read [../spec.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/spec.md) for features and technical architecture. Source is in `../Source/`, XML in `../Defs/`.
- **Strict Exclusion:** **NEVER scan, list, or read `resolved-and-qa-passed/`** (conserve tokens). Only inspect active bug files directly in `bugs/`.
- **Bytecode Restriction:** **NEVER look at bytecode** unless you can provide a good reason to the human in the loop and there are no other options.

---

## Trigger: "fix bugs"

When prompted with "fix bugs" (or targeting open issues), execute this workflow:

1. **Context:** Read [../spec.md](file:///home/tylerkilburn/Git/rimworld-mods/mods/custom/eugenics-program/spec.md) if not already read this session.
2. **Triage:** Find `bug-*.md` files directly in `bugs/` with `**Status:** Open` or reopen notes. Prioritize sequentially; tackle one at a time.
3. **Model Check:** If the bug requires complex Harmony transpilers or deep subsystem refactoring, advise the user to switch from `medium` to `high`. Otherwise, continue on `medium`.
4. **Diagnose & Fix:**
   - Trace the issue in `../Source/` and `../Defs/`.
   - Apply edits via scoped tools (`replace_file_content`, `write_to_file`).
   - Expose tunable balance numbers via XML (`CompProperties`/`DefModExtension`), not hardcoded C#.
5. **Build & Verify:**
   ```bash
   dotnet build ../Source/EugenicsProgram.csproj
   ```
   Must pass with 0 errors and 0 warnings.
6. **Update Bug Report:**
   - Set `**Status:** Ready for QA`.
   - Complete `## Dev Notes / Fix` with **Cause** and **Fix**.
   - Do NOT move the file to `resolved-and-qa-passed/` (user moves it after in-game QA).
7. **Report:** Provide a concise summary with `[filename](file:///path/to/file)` links. Never commit git changes.
