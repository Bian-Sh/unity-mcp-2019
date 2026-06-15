# 2026-06-16 upstream beta 同步适配纪要

## 同步范围

- 上游目标：`upstream/beta` @ `dccecd689072dce4c319e15c812006005c68ad0b`。
- 本地基线：`beta` @ `b359fcfbd5964d2b1e615d433ecc8b5e2b38a367`。
- 临时 merge-base：`c0908b88d6ec2d7152df2a8fc9c1590270856390`。
- 上游主要版本变化：`MCPForUnity/package.json` 从 `9.7.2-beta.10` 到 `9.7.4-beta.1`，`manifest.json` 从 `9.7.1` 到 `9.7.3`。

## 主要上游变更

- `StdioBridgeHost` / `PortManager` / `ServerManagementService`：吸收 stdio 端口稳定性、PID 校验、headless local HTTP server 启动与 liveness polling 相关修复。
- `McpConnectionSection` / `HttpAutoStartHandler` / server launcher：同步 HTTP local/remote 连接 UI 状态、启动停止按钮与自动启动逻辑。
- `ConfigJsonBuilder` / `KiloCodeConfigurator`：同步 Kilo Code 新 `~/.config/kilo/kilo.jsonc` MCP 配置格式，使用 `mcp` 容器、`type: remote/local` 与 `enabled` 字段。
- `UnityTypeResolver`：同步 `unity_reflect` 对泛型类型名称解析的修复。
- `MemorySnapshotOps`：同步 Unity 6 / CoreModule memory snapshot API 反射兼容逻辑。
- `tools/local_harness.py`、`.github/workflows/e2e-bridge.yml`、`Server/tests/e2e/bridge_smoke.py`：同步上游 headless bridge harness 和 CI smoke 测试。
- 新增/补齐 Unity EditMode 测试 `.meta` sidecar 与 Python tool/test symmetry guard。

## 本地适配

- 合并冲突集中在 Unity 测试 `.meta` sidecar 的 add/add GUID 差异，已保留本地 GUID，避免现有 Unity 引用漂移。
- `tools/local_harness.py` 上游实现使用宿主 `Path` 拼接模拟的 macOS/Linux/Windows Unity Hub 路径；在 Windows 运行 hermetic 测试时会把目标平台路径改成宿主分隔符。本地新增 `platform_join()`，按目标平台语义拼接路径。
- `tools/tests/test_local_harness.py` 的 fake filesystem 增加 `/` 与 `\` 路径别名，并将跨平台预期路径改用 `lh.platform_join()`，保证 Windows 主机上也能验证 macOS/Linux path resolution。
- 未发现本轮 C# 改动引入 `record`、`init`、`using var`、`is not`、`??=` 等 C# 8+ 语法；保留 Unity 2019 / C# 7.3 兼容策略。

## 验证记录

- `cd Server && uv run pytest tests/test_tool_test_symmetry.py -v`
  - 结果：`35 passed, 3 skipped`。
- `cd Server && uv run pytest ../tools/tests/test_local_harness.py -v`
  - 首次暴露 Windows 主机路径分隔符问题；完成本地适配后结果为 `69 passed`。
- `cd TestProjects/UnityMCPTests && dotnet build .\MCPForUnity.Editor.csproj --no-restore`
  - 结果：`0 errors`，存在 8 个既有 Unity/Plastic/VisualScripting 引用解析 warning。

## 后续注意

- 若后续运行上游 `tools/local_harness.py`，不要把 `Path(root) / version / relpath` 重新引回目标平台路径解析逻辑，否则 Windows 主机上模拟 macOS/Linux 的单元测试会回归失败。
- 如果要做完整 Unity matrix，优先使用上游新增的 `tools/local_harness.py` 或既有 `tools/check-unity-versions.sh`；本次只做了 Python 单测和 `MCPForUnity.Editor.csproj` 快速编译。
