# unity-sprite-sheet-slicer skill 搬运记录

## 背景

本次将本机 Codex skill `C:\Users\BianShanghai\.codex\skills\unity-sprite-sheet-slicer` 搬运到项目内，目标是在 Unity 窗口右下角 `Install Skills` 安装时也能同步安装这个独立 skill。

## 代码追踪结论

- `MCPForUnity/Editor/Windows/Components/ClientConfig/McpClientConfigSection.cs` 的 `OnInstallSkillsClicked()` 调用 `SkillSyncService.SyncAsync()`。
- `MCPForUnity/Editor/Setup/SkillSyncService.cs` 原先硬编码从远端 `.claude/skills/unity-mcp-skill` 拉取数据。
- Codex 的安装目标原先是 `%USERPROFILE%/.codex/skills/unity-mcp-skill`，只能覆盖一个 skill 目录，不能安装 sibling skill。

## 变更

- 新增 `.claude/skills/unity-sprite-sheet-slicer/SKILL.md`。
- 新增 `.claude/skills/unity-sprite-sheet-slicer/scripts/slice_icons.py`。
- 调整 `SkillSyncService`：从固定单个 skill 改为同步 `.claude/skills/unity-mcp-skill` 和 `.claude/skills/unity-sprite-sheet-slicer` 两个远端子目录。
- 调整 Codex / Claude 的 skill 安装目标为各自的 `skills` 根目录，再由同步服务逐个写入独立 skill 子目录，避免托管或删除用户其它 skill。
- 调整 `.gitignore`：继续忽略 `.claude` 下的本地配置，但允许 `.claude/skills/**` 进入仓库。

## 验证

- 已确认 `.claude/skills/unity-sprite-sheet-slicer` 与本机源 skill 的 `SKILL.md`、`scripts/slice_icons.py` SHA256 一致。
- 已确认错误放置的 `.agents/skills/unity-sprite-sheet-slicer` 已移除。
- 已确认 `git status --short` 能显示新的 `.claude/skills/unity-sprite-sheet-slicer/` 待提交文件。
