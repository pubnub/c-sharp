# CLAUDE.md

@AGENTS.md

## Claude Code notes

- **Where guidance goes:** put guidance that applies to all agents in `AGENTS.md`. Keep only Claude Code-specific notes in this file.
- **Area rules:** Claude Code doesn't attach `.cursor/rules/*.mdc` automatically. Before editing an area, read the rule listed for it in the "Where to find more" table in `AGENTS.md`.
- **Skills** in `.claude/skills/`: `add-endpoint`, `run-checks` and `port-cross-sdk-feature`. Use `run-checks` before reporting a code change as done.
- **Permissions** (`.claude/settings.json`):
  - Pre-approved: `dotnet build`, `dotnet test`, `dotnet restore`, the `run-checks` scripts and read-only git commands.
  - Blocked: editing `bin/`, `obj/` and `pubnub.snk`.
  - Personal overrides go in `.claude/settings.local.json` and personal notes in `CLAUDE.local.md`; both are git-ignored.
- **PubNub MCP server:** `.mcp.json` defines it at project scope. Approve it when Claude Code asks, then run `/mcp`, select `pubnub` and authenticate.
- **Shell:** the `run-checks` scripts are PowerShell. Use `powershell -ExecutionPolicy Bypass -File <script>` on Windows, or `pwsh -File <script>` on macOS and Linux.
