# Upstream 同步 Commit Trailer 规范

最终同步或兼容 commit 应在正文末尾保留机器可读 trailer，便于下一次 skill 自动定位同步范围。

## 推荐格式

```text
Upstream-Repo: CoplayDev/unity-mcp
Upstream-Branch: beta
Upstream-Base: <previous-upstream-commit>
Upstream-Target: <new-upstream-commit>
Upstream-Range: <previous-upstream-commit>..<new-upstream-commit>
Compat-Target: Unity 2019 / C# 7.3
Sync-Mode: semantic-ai-port
```

## Commit message 示例

```text
Sync upstream beta changes with Unity 2019 compatibility

Port upstream tool and server changes semantically, then reapply C# 7.3
syntax compatibility and Unity API shims required by this fork.

Upstream-Repo: CoplayDev/unity-mcp
Upstream-Branch: beta
Upstream-Base: 2d2bdc557502bf38cb2507d486a95137f94341fe
Upstream-Target: 0123456789abcdef0123456789abcdef01234567
Upstream-Range: 2d2bdc557502bf38cb2507d486a95137f94341fe..0123456789abcdef0123456789abcdef01234567
Compat-Target: Unity 2019 / C# 7.3
Sync-Mode: semantic-ai-port
```

## 读取策略

优先从最近的同步 commit 读取 trailer：

```powershell
git log --format=%B --grep="Upstream-Target:" -n 1
```

如果没有历史 trailer，使用 `git merge-base HEAD upstream/beta` 作为临时基线，并在本次同步完成后补齐 trailer。

## 注意事项

- 使用完整 commit hash，避免短 hash 在长期历史中歧义。
- 不确定的值不要写进最终 commit。
- 如果用户选择 squash，确保 squash 后的最终 commit 仍保留 trailer。
