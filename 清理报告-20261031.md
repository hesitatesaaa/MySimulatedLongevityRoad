# 项目清理报告 — 2026-10-31

## 结果

| | 清理前 | 清理后 |
|---|---:|---:|
| 工作区 `D:\模拟长胜路` | 4.53 GB | **3.27 GB** |
| 文件数 | 38,698 | 32,275 |

本次移走 **1,131 MB（1.10 GB）**，暂存于 `D:\_trash-模拟长胜路`（可回滚）。

## 本次清理内容（按你选的 C+D 组）

| 组 | 内容 | 大小 |
|---|---|---:|
| C1 | `validation`（0.7.0 运行时验收快照） | 736 MB |
| C2 | `.dev-tools\combat-art`（第三方美术参考） | 261 MB |
| C3 | `source`（上游 mod 素材） | 114 MB |
| C4 | `_ref\WorldBoxGameDecompiled`（反编译参考） | 11 MB |
| C5 | `.codegraph`（索引缓存，可重建） | 8 MB |
| D | ACL 备份、空目录、反射探针、4 个旧清理报告 | 约 2 MB |

逐项清单见 `清理报告-20261031.csv`。

## 保留的（A、B 组按你的意思未动）

- **历史构建目录**：`upstream-0.6.0\build` 下 10 项（约 972 MB）全部保留
- **0.8.1 过渡产物**：`optimization...\delivery` 171 MB、`_pkg_build` 152 MB、`baseline-0.8.1-20261030` 163 MB
- **承载目录**：`optimization-0.8.1-20261030` 589 MB（活跃中）、`deliverables` 818 MB（0.7.0 737 + 0.8.1 74）
- **源码**：`upstream-0.6.0\code\` 273 个文件 / 3.4 MB，HEAD 仍是 `6d3c23e`
- **素材**：`GameResources` 132 MB、`NativeActorResources` 89 MB、`InterfaceResources` 13 MB、`resources` 12 MB
- **0.8.1 交付包**：SHA256 与 `.sha256.txt` 记录一致（`c1b9aff1…43bbc`）

## 注意：项目正在被另一个会话同时修改

清理期间观察到：

- `optimization-0.8.1-20261030` 体积从 746 MB 变成 589 MB（它的 `build` 目录一直在写）
- `upstream-0.6.0` 未提交文件从 292 处涨到 **386 处**

说明有另一个会话/进程正在这个工作区里跑游戏构建与验证。本报告的体积统计是某一时刻的快照，会随对方工作持续变化。

**建议：等那边告一段落后，把 `upstream-0.6.0` 的改动提交一次**，目前 386 处改动全在工作区里，没有任何版本记录。

## 回滚方式

内容按原相对路径保存在 `D:\_trash-模拟长胜路\`。恢复某一项时移回原位置即可。

确认无误后永久释放：

```powershell
Remove-Item -LiteralPath 'D:\_trash-模拟长胜路' -Recurse -Force
```
