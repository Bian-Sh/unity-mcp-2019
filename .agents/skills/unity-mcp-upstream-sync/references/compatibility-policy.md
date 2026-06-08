# Unity MCP 兼容同步策略

本文档用于配合 `unity-mcp-upstream-sync` skill。它不是硬规则清单，而是 AI 在同步 upstream 变更时的判断框架。

## 优先级

1. 保留 upstream 的真实功能意图。
2. 保留本 fork 的 Unity 2019 / C# 7.3 可用性。
3. 保持 Python MCP tools、CLI、resources 与 Unity C# handlers 的协议一致。
4. 避免为了快速合并而引入长期维护成本。

## 兼容处理原则

- 语法降级必须保持语义等价，不做只为编译通过的表面替换。
- Unity API 版本差异优先封装到 helper / compat shim，不在业务调用点扩散条件编译。
- 已存在的本地兼容实现不要因为 upstream 没有就删除；先判断它是否仍然必要。
- upstream 如果引入新的抽象，优先理解抽象意图，再决定本 fork 是否直接采用或以兼容形式改写。

## 需要重点审查的区域

- `MCPForUnity/Editor/Tools/`：MCP 工具行为和 Unity Editor API 使用。
- `MCPForUnity/Runtime/Helpers/`：Unity API compatibility shims。
- `Server/src/services/tools/`：AI 助手可见 MCP tools。
- `Server/src/cli/commands/`：开发者 CLI，与 MCP tools 分开维护。
- `Server/src/services/resources/`：只读资源接口。
- `package.json`、`package-lock.json`、`pyproject.toml`、`uv.lock`：版本与依赖漂移。
- `Assembly-CSharp.csproj`、`.asmdef`、`.meta`：Unity 资产和编译纳入关系。

## 常见降级方式

- `new()` 改为显式类型构造。
- file-scoped namespace 改为 block-scoped namespace。
- `record` 改为 class 或 struct，并显式实现需要的成员。
- `init` 改为 `set` 或构造函数赋值。
- collection expression 改为数组或集合构造。
- switch expression 改为 `switch` statement。
- 复杂 pattern matching 改为显式 `if` / cast / null check。

## 不要做的事

- 不要批量格式化无关文件。
- 不要为了消除冲突删除本地兼容层。
- 不要把 upstream 的 release artifact 当成源码真相。
- 不要在 fetch 失败时声称已同步到 upstream 最新版本。
- 不要提交或推送，除非用户明确要求。
