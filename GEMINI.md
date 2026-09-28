# RimWorld Mods Workspace Rules

This configuration applies to `/home/tylerkilburn/Git/rimworld-mods` and recursively to all child directories and sub-mods.

## CLI Execution Permissions
- **Autonomous Execution Authorized:** The agent is authorized to execute any local terminal commands without prior confirmation, including:
  - Builds and compilation (`dotnet build`).
  - Search, discovery, and inspection (`ls`, `fd`, `find`, `rg`, `grep`, `cat`).
  - Test runners, diagnostic tools, and local bash/python scripts.
- **Scope Boundary:** 
  - External network mutations and remote state commands (e.g., `git push`, remote cloud APIs) still require explicit confirmation.
  - Never automatically `git commit`; git commits are managed manually by the user.

## Conventions
- Scoped editing tools (`replace_file_content`, `write_to_file`) remain preferred for precise code modifications.
- Shell search (`fd`, `rg`, `ls`) and build tooling may be used freely and autonomously whenever helpful.
