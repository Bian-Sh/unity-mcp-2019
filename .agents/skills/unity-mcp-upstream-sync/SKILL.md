---
name: unity-mcp-upstream-sync
description: Semantically synchronize this Unity 2019/C# 7.3 fork with upstream CoplayDev/unity-mcp. Use when the user asks to pull, merge, upgrade, sync, port, or adapt new upstream unity-mcp changes into this repository while preserving local Unity 2019 compatibility, syntax downgrades, MCP protocol behavior, local customizations, and final upstream commit provenance.
---

# Unity MCP Upstream Sync

Use this skill to perform an AI-guided semantic upgrade from `CoplayDev/unity-mcp` into this repository. Treat scripts and Git output as evidence, not as the source of truth. The objective is to preserve upstream intent while maintaining this fork's Unity 2019 / C# 7.3 compatibility and local behavior.

## Operating Principles

- Do not treat upstream sync as mechanical CRUD.
- Prefer semantic understanding before applying patches.
- Preserve local compatibility work unless upstream made it obsolete.
- Use Git, diffs, tests, and source inspection as evidence for AI judgment.
- Keep helper scripts read-only or report-generating unless the user explicitly approves mutation.
- Do not commit, push, or open PRs unless the user explicitly asks.

## Repository Context

- Upstream remote: `https://github.com/CoplayDev/unity-mcp.git`.
- Primary upstream branch is usually `upstream/beta` unless the user specifies another branch/tag.
- This fork targets Unity 2019-era compatibility and C# 7.3 syntax constraints.
- Python MCP tools, Python CLI commands, MCP resources, and Unity C# tools are separate layers; do not assume one is generated from another.
- Unity API compatibility should be centralized through runtime helper shims when possible, not scattered across call sites.
- Installable AI assistant skills that must ship with copied Unity packages live under `MCPForUnity/Skill~/`; do not rely on repository-root `.claude/skills/` or `.agents/skills/` for files that must survive copying only the `MCPForUnity/` folder.

## Recommended Workflow

1. Establish a clean baseline.
   - Run `git status --short` and identify unrelated local changes.
   - If unrelated changes exist, avoid touching them and warn the user before destructive actions.
   - Run `git fetch upstream --prune`; if it fails, report the exact error and stop before pretending the upstream state is fresh.

2. Identify the sync range.
   - First look for the latest sync commit trailers: `Upstream-Base`, `Upstream-Target`, and `Upstream-Range`.
   - If no trailer exists, use `git merge-base HEAD upstream/beta` as the provisional previous upstream point.
   - Confirm the target commit with the user when the branch/tag is ambiguous.

3. Collect evidence.
   - Use `scripts/collect_upstream_context.ps1` to generate a local report when helpful.
   - Inspect `git diff --name-status <old>..<new>` to classify additions, deletions, renames, and modifications.
   - Inspect actual patches for behavior-bearing files; do not rely only on filenames.
   - For large diffs, group by domain: Python server, CLI, resources, Unity editor tools, runtime helpers, tests, docs, package metadata.

4. Build a semantic change map.
   - For each upstream change, decide whether to accept directly, adapt, skip, replace with an existing local equivalent, or ask the user.
   - Record why skipped or substantially rewritten changes are not applied verbatim.
   - Watch for protocol/schema drift across Python and C# layers.
   - Watch for behavior changes hidden in version bumps, generated files, settings, and package metadata.

5. Integrate carefully.
   - Prefer `git merge --no-commit <target>` when repository history is compatible.
   - If merge is too noisy, apply targeted patches or manually port changes by domain.
   - Resolve conflicts by preserving upstream intent and local compatibility constraints.
   - Do not delete local adaptations simply because upstream lacks them.

6. Run compatibility pass.
   - Downgrade C# syntax to C# 7.3 without changing semantics.
   - Replace newer Unity APIs with compatibility helpers where needed.
   - Ensure new Unity `.cs` assets have matching `.meta` and are included in relevant `.csproj` files when the project tracks them.
   - Re-check MCP tool names, parameter names, response shapes, and error semantics across Python and C#.
   - If upstream changes assistant skill installation, verify the right-bottom `Install Skills` button path in `McpClientConfigSection` and `SkillSyncService`; package-shipped skills must be copied from the installed package root (`MCPForUnity/Skill~/`) so direct folder-copy installs work offline.

7. Validate.
   - Run the narrowest relevant tests first.
   - For Python changes, prefer `cd Server && uv run pytest tests/ -v` or a narrower test file.
   - For Unity/C# structural changes, run the project-appropriate compile check. In this repo, `dotnet build .\Assembly-CSharp.csproj --no-restore` may be useful when that project file exists.
   - If Unity compatibility shims or `#if UNITY_*` gates changed, run `tools/check-unity-versions.sh` when available and practical.
   - If validation cannot run, record the reason and the exact command the user should run.

8. Prepare the final handoff.
   - Summarize upstream range, applied domains, compatibility adaptations, skipped changes, and validation results.
   - Do not claim the sync is complete if the upstream fetch failed or target commit was not verified.
   - If the user asks for a commit, the final compatibility/sync commit message must include the provenance trailers below.

## Required Commit Trailers

When creating the final sync or compatibility commit, include these trailers at the end of the commit message:

```text
Upstream-Repo: CoplayDev/unity-mcp
Upstream-Branch: <branch-or-tag>
Upstream-Base: <previous-upstream-commit>
Upstream-Target: <new-upstream-commit>
Upstream-Range: <previous-upstream-commit>..<new-upstream-commit>
Compat-Target: Unity 2019 / C# 7.3
Sync-Mode: semantic-ai-port
```

Use full commit hashes when available. If a trailer value is uncertain, do not invent it; stop and ask or record `unknown` only in a draft note, not in a final commit.

## C# 7.3 Downgrade Checklist

Review upstream C# files for modern syntax before finalizing:

- Target-typed `new()` expressions.
- File-scoped namespaces.
- Global using directives.
- `record` types.
- `init` setters and `required` members.
- Primary constructors.
- Collection expressions.
- Range/index operators when unsupported by the target runtime.
- Newer switch expressions or pattern matching that exceed C# 7.3.
- Nullable reference type annotations if the project configuration cannot support them safely.

Prefer explicit, readable C# 7.3 equivalents over clever rewrites.

## Skill Packaging Checklist

When upstream adds, removes, or changes assistant skills:

- Treat `MCPForUnity/Skill~/` as the package-shipped source of truth for skills installed by the Unity Editor UI.
- Keep each skill as an independent subdirectory with its own `SKILL.md`, for example `MCPForUnity/Skill~/unity-mcp-skill/` and `MCPForUnity/Skill~/unity-sprite-sheet-slicer/`.
- Remember that folders ending in `~` are intentionally hidden from Unity's AssetDatabase / Inspector; do not add `.meta` files for this ignored payload.
- Verify that copying only `MCPForUnity/` to a temporary folder still includes every skill required by the `Install Skills` button.
- Do not assume repository-root `.claude/skills/` or `.agents/skills/` will be present in installed Unity projects; those paths are development/distribution aids, not the direct-copy package payload.
- If remote GitHub skill sync remains available for a separate maintenance window, keep it clearly separate from the default packaged-skill install path.

## API Alignment Checklist

For every changed MCP-facing domain, compare all layers that apply:

- Python MCP tool in `Server/src/services/tools/`.
- Python CLI command in `Server/src/cli/commands/`.
- Python resource in `Server/src/services/resources/`.
- Unity C# tool/resource in `MCPForUnity/Editor/` or `MCPForUnity/Runtime/`.
- Tests and docs describing the tool contract.

Confirm tool names, action names, parameter aliases, response fields, error messages, paging behavior, and group visibility.

## Conflict Resolution Heuristics

- If upstream changed architecture and local code only changed syntax, port upstream architecture then reapply syntax downgrade.
- If local code added Unity 2019 compatibility, keep it unless upstream added a better compatible abstraction.
- If upstream removed a feature that local users may rely on, inspect commit context before deleting it.
- If upstream renamed or moved files, prefer preserving upstream layout unless this fork has a documented compatibility reason.
- If generated/version files changed, verify whether they are source-of-truth or release artifacts before editing.

## Handoff Format

When reporting results to the user, include:

- Upstream range used.
- Files/domains changed.
- Compatibility adaptations made.
- Skipped or uncertain upstream changes.
- Validation commands run and results.
- Whether any commit/push was intentionally not performed.
