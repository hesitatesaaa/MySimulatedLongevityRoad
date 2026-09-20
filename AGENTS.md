# 项目协作规则

## 每次修改后的可运行 Mod 包

- 每次完成代码、资源、配置或本地化修改后，都必须尝试执行 Release 构建并生成可运行 Mod ZIP。
- 使用 `scripts/Package-Mod.ps1` 打包；更新标签使用本次最重要功能或修复的 2–5 个中文字符，例如 `空间继承`、`排行榜`。
- 日常包输出到 `发布包/`，格式为 `我的模拟长生路-v<mod.json版本>-<更新标签>.zip`；同名时追加时间戳，不覆盖历史包。
- 发布包沿用 `发布包/` 中现有的源码型 Mod 结构，包含入口源码、项目文件、`code/`、`GameResources/`、`Locales/`、配置、图标和说明文档；不强制要求本机先生成 DLL。
- `references/WorldBoxGameDecompiled/`、`.git/`、`obj/`、`bin/`、`DeveloperTools/`、打包脚本和本地临时文件不得进入玩家发布包。

## 版本与 GitHub 边界

- 日常修改和日常可运行包保持 `mod.json` 当前版本不变；只有用户明确要求正式版本时才递增版本号、更新 CHANGELOG 或创建正式标签。
- 未收到用户明确的“推到 GitHub”或等价指令时，禁止执行 `git push`、远程标签推送、GitHub Release、Pull Request 或其他 GitHub 写操作。
- 本地检查、构建、打包和保留工作区修改不需要 GitHub 授权；推送前必须先完成构建、包内容检查和 Git 差异检查。

## 完成报告

- 修改完成后报告：源码型 ZIP 路径和包内容校验结果；如果进行了构建，也单独报告构建结果。
- 不要把 `references/` 中的反编译源码编入模组，也不要改变项目现有 WorldBox DLL `HintPath` 机制来绕过环境缺失。
