# 仓库分仓说明（2026-11-02）

本项目采用**两个独立 Git 仓库共享同一工作区**的方式管理（方案见 `docs/CODE-QUALITY-IMPL-PLAN-20261102.md` 阶段 M0-B）。

## 两个仓库

| 仓库 | Git 目录 | 跟踪内容 | 规模 |
|---|---|---|---|
| 源码仓 | `.git` | `code/ tools/ docs/ resources/ .tests/ .dev-tools/ .github/`、根文件、`Locales/` | 约 1,893 文件 / 7.7 MB（.git） |
| 资源仓 | `.git-assets` | `GameResources/`、`InterfaceResources/`、`NativeActorResources/` | 约 2,232 文件 / 71.8 MB（.git） |

工作区文件不移动，因此 `tools/compile-check.ps1`、`.tests`、打包脚本的相对路径都不变。

## 日常操作

源码仓就是根目录的默认 Git 仓库，直接使用：

```powershell
git status
git add -A
git commit -m "..."
```

资源仓使用独立 git 目录与显式工作区，**用原生 git 命令**（任何 shell 都可用）：

```powershell
git --git-dir=".git-assets" --work-tree="." status
git --git-dir=".git-assets" --work-tree="." add -A
git --git-dir=".git-assets" --work-tree="." commit -m "update art"
git --git-dir=".git-assets" --work-tree="." log --oneline -5
```

也可用封装脚本 `tools/git-assets.ps1`（参数原样透传给 git）：

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/git-assets.ps1 status
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/git-assets.ps1 add -A
pwsh -NoProfile -ExecutionPolicy Bypass -File tools/git-assets.ps1 commit -m "update art"
```

> 本机 `LocalMachine` 执行策略为 `Restricted`，直接运行 `.ps1` 会被拒绝；需加 `-ExecutionPolicy Bypass`（对所有 `tools/*.ps1`、`.tests/run.ps1` 同样适用）。因此上表以原生 git 命令为主。

## 忽略规则如何分工

- 共享的 `.gitignore`：只放**构建产物/临时物**（`build/`、`deliverables/`、`.archive/`、`.codegraph/`、`**/bin/`、`**/obj/`、`*.dll/*.pdb/*.exe` 等），两个仓库都生效。
- 源码仓的 `.git/info/exclude`：额外排除三个美术目录和 `.git-assets/`（本仓库私有，不入库）。
- 资源仓的 `.git-assets/info/exclude`：用 `/*` 反向排除，只保留三个美术目录与 `.gitattributes`。

> 注意：`info/exclude` 是**仓库本地**配置，不会随仓库克隆。若以后新增克隆，需要在克隆里补同样的排除，或改由 `.gitignore` 承担（届时资源仓需用 `git add -f` 强制加入）。

## 备份与历史

- 分仓前旧仓库完整保留在 `.archive/git-legacy-20261102`（HEAD `e8082fb`，4,111 文件，1.2 GB），可随时 `git --git-dir=.archive/git-legacy-20261102 ...` 查看。
- 另有一份 `git bundle`：`build/_git-backup-20261102/repo-backup-20261102.bundle`（76.9 MB，校验为完整历史）。
- 确认无误后，`.archive/git-legacy-20261102` 可删除以回收约 1.2 GB（删除前请确认 bundle 仍可 `git bundle verify`）。

## 为什么不用 LFS / 单仓

实测美术二进制约 72 MB（PNG），直接进源码仓会让历史变重；分仓后源码仓 `.git` 从 1.2 GB 降到 7.7 MB，资源仓独立为 71.8 MB，符合当初"分仓"决定。
