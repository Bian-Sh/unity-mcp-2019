# unity-sprite-sheet-slicer skill 搬运记录

## 背景

用户安装 MCP for Unity 的常见方式是直接把 `MCPForUnity/` 文件夹复制到其它 Unity 工程。仓库根目录下的 `.claude/`、`.agents/` 不会随这种安装方式进入目标工程，因此 skill 资源必须放在 `MCPForUnity/` 包目录内。

## 代码追踪结论

- `MCPForUnity/Editor/Windows/Components/ClientConfig/McpClientConfigSection.cs` 的 `OnInstallSkillsClicked()` 是右下角 `Install Skills` 按钮入口。
- 原实现通过 `SkillSyncService.SyncAsync()` 从 `https://github.com/CoplayDev/unity-mcp` 远端拉取 `.claude/skills/unity-mcp-skill`，不读取本地包内资源。
- `AssetPathUtility.GetMcpPackageRootPath()` 能定位已安装包根目录，包括 `Assets/MCPForUnity` 这种直接复制安装形态。

## 变更

- 新增包内隐藏资源目录 `MCPForUnity/Skill~/`。Unity 会忽略以 `~` 结尾的目录，避免这些 skill 文件出现在 Inspector / AssetDatabase 中。
- 将 `unity-mcp-skill` 放入 `MCPForUnity/Skill~/unity-mcp-skill/`。
- 将 `unity-sprite-sheet-slicer` 放入 `MCPForUnity/Skill~/unity-sprite-sheet-slicer/`。
- 新增 `SkillSyncService.SyncPackagedAsync()`：从当前已安装的 `MCPForUnity/Skill~/` 复制 skill 到客户端 `skills` 根目录。
- 右下角 `Install Skills` 按钮改为调用 `SyncPackagedAsync()`，不再依赖仓库根目录 `.claude/` 或远端 GitHub。
- 保留独立 `McpForUnitySkillInstaller` 窗口的远端同步逻辑，避免扩大影响面。

## 验证

- 模拟只复制 `MCPForUnity/` 后，能携带 `MCPForUnity/Skill~/unity-sprite-sheet-slicer/SKILL.md`。
- 已确认包内 `unity-sprite-sheet-slicer` 与本机源 skill 的 `SKILL.md`、`scripts/slice_icons.py` SHA256 一致。
- 已运行 `dotnet build TestProjects/UnityMCPTests/MCPForUnity.Editor.csproj --no-restore` 做快速编译验证。
