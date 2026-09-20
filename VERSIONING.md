# 版本隔离与回滚规则

本项目从 `v0.1.4` 起采用不可变 Git 标签隔离正式版本，确保每次发布均可独立下载、比较和回滚。

## 版本对应关系

发布时，下列位置的版本号必须完全一致：

- `mod.json` 中的 `version`；
- `CHANGELOG.md` 的版本标题；
- Git 标签 `v主版本.次版本.修订版本`；
- 发布压缩包及其顶层目录名称；
- GitHub Release 标题。

## 日常构建与 GitHub 推送边界

每次完成代码、资源、配置或本地化修改后，先执行本地可运行包流程：

```powershell
.\scripts\Package-Mod.ps1 -ChangeTag 更新标签
```

`更新标签`必须是本次最重要功能或修复的 2–5 个中文字符。脚本从 `mod.json` 读取版本，按现有源码型发布包结构生成 `发布包/0.5.1+我的模拟长生路0.1.9-<更新标签>.zip`；同名时追加时间戳。包包含入口源码、解决方案/项目文件、`mod.json`、入口资源、`code`、`GameResources`、`Locales`、配置和说明文档，并排除 `references/`、构建缓存、本地开发工具和打包脚本。

日常构建不修改版本号、不创建正式标签，也不执行 GitHub 写操作。源码型发布包不要求本机存在 WorldBox、Unity、Harmony、NeoModLoader 或 Publicized 游戏程序集；若需要编译验证，再单独执行项目构建。

除非用户明确说“推到 GitHub”或使用等价表达，否则禁止执行 `git push`、远程标签推送、GitHub Release、Pull Request 或其他远程写操作。收到推送指令后，仍须先完成构建、ZIP 内容校验和 `git diff` 检查。

## 正式开发和发布流程

1. 日常开发提交到 `main`；较大功能先使用 `feature/功能名` 分支，再合并到 `main`。本地提交不等于 GitHub 推送。
2. 发布前更新 `mod.json`、`README.md` 和 `CHANGELOG.md`。
3. 完成语法、配置、资源、Release 构建和压缩包结构检查。
4. 提交发布版本，使用带说明的标签固定该提交，例如：

   ```powershell
   git tag -a v0.1.7 -m "我的模拟长生路 v0.1.7"
   git push origin main
   git push origin v0.1.7
   ```

5. 只从该标签对应的源码生成同版本发布包，并附到对应的 GitHub Release。
6. 标签发布后禁止强制移动、删除或复用；如需修复，递增修订版本发布新标签。

构建文件、临时目录、旧版本的复制目录及发布 ZIP 不提交到源码分支；二进制发布包由 GitHub Release 保存，避免仓库文件堆积。

## 更新日志格式

每个版本在 `CHANGELOG.md` 中只使用一组从 `1.` 开始的连续编号记录更新内容，例如 `1.`、`2.`、`3.`。同一版本内不按功能分类重新编号；需要标明功能类别时，在该条开头使用加粗的简体中文名称。后续新增版本均沿用此格式。

## 回滚方式

临时查看或构建旧版本：

```powershell
git fetch --tags
git switch --detach v0.1.6
```

从旧版本建立修复分支：

```powershell
git switch -c hotfix/0.1.6 v0.1.6
```

恢复到当前开发分支：

```powershell
git switch main
```

不应对共享的 `main` 使用破坏历史的强制回退。需要撤销已合并修改时使用 `git revert` 生成新提交；需要以旧版本为基础继续开发时创建分支并发布更高版本号。

## 已隔离版本

| 版本 | Git 标签 |
| --- | --- |
| 0.1.4 | `v0.1.4` |
| 0.1.6 | `v0.1.6` |
| 0.1.7 | `v0.1.7` |
| 0.1.8 | `v0.1.8` |
