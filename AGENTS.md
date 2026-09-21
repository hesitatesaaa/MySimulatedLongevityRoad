# 项目协作规则

## 工作范围

- 当前 worktree 根目录是唯一日常修改范围：`code/`、入口文件、项目文件、配置、资源、本地化、文档和 `scripts/`。
- `references/` 只用于检索 WorldBox API，始终排除在编译和发布包之外。
- `DeveloperTools/` 只用于本机调试，始终排除在正式源码和发布包之外。
- `bin/`、`obj/`、`发布包/`、`_archive/`、`.idea/`、`.codegraph/`、`.codex/` 和日志都是生成物或本地文件，不作为源码修改目标。
- 不修改父级工作区中的 `source/`、`deliverables/`、`validation/`、`.github/` 或其他 worktree；需要比较历史版本时只读处理。

## 快速检索与修改

- 优先使用可用的 CodeGraph 定位符号和调用链；CodeGraph 不可用时使用受限范围的 `rg` 或 `rg --files`。
- 搜索默认排除 `references/`、`DeveloperTools/`、`bin/`、`obj/`、`发布包/`、`_archive/` 和 `.git/`，不要无理由扫描整棵参考源码树。
- 先确认目标文件和调用入口，再读取必要的局部范围；互不依赖的只读检查可以并行。
- 使用 `apply_patch` 修改文件。不得使用 `git reset --hard`、`git clean`、强制 checkout 或覆盖用户已有未提交改动。
- 一次用户请求中的多次修改合并完成后只生成一次发布包，避免每个小改动重复构建和压缩。

## 构建与验证矩阵

- 使用 `scripts/Test-Project.ps1` 作为统一验证入口；它会先执行静态检查，再按模式执行构建或打包。

  ```powershell
  pwsh -NoProfile -File .\scripts\Test-Project.ps1 -Mode Static
  pwsh -NoProfile -File .\scripts\Test-Project.ps1 -Mode Build -WorldBoxDataRoot D:\path\to\worldbox_Data
  pwsh -NoProfile -File .\scripts\Test-Project.ps1 -Mode Package -ChangeTag 项目整理
  ```

- C#、项目文件或构建脚本修改：执行 `Build`；首次构建成功后，后续同一依赖环境可加 `-NoRestore` 减少还原时间。

  ```powershell
  pwsh -NoProfile -File .\scripts\Build-Mod.ps1 -Configuration Release -WorldBoxDataRoot D:\path\to\worldbox_Data
  pwsh -NoProfile -File .\scripts\Package-Mod.ps1 -ChangeTag 项目整理
  ```

- JSON 或本地化修改：解析 JSON，并检查 `Locales/ch.json`、`Locales/cz.json` 的新增键；不因文案修改重复完整编译。
- 资源修改：检查资源路径、文件存在性和最终压缩包内容；只有同时改动 C# 或项目文件时才构建。
- 仅文档修改：检查 Markdown 链接和内容一致性，不运行完整构建。
- 缺少 WorldBox 外部 DLL 时，构建必须快速失败并列出缺失文件；不得用大量无效编译错误代替依赖预检。

## 项目构建边界

- 项目目标框架为 `netstandard2.1`，供 WorldBox 的 Unity/Mono 运行环境加载。
- 外部依赖通过 `WorldBoxDataRoot` 提供，默认值为项目外的 `..\..\worldbox_Data`，可通过脚本参数或 MSBuild 属性覆盖。
- `DeveloperTools/` 和 `references/` 中的 C# 文件不得进入项目编译项；正式包不得包含 `references/`、`.git/`、`bin/`、`obj/`、`DeveloperTools/` 或 `scripts/`。
- 运行时源码按现有 `Core / Data / Interop / Modules / Patches / Queries / Systems / Traits / UI` 领域放置，避免为单个常量或薄包装继续创建碎片文件。

## 功能不变量

- 还真锚点、回载、前世档案和遗产选择必须保持有界重试、同世界同身份校验和版本化外部存储；新增快照字段时同步更新允许字段列表。
- 还真空间、玄黄仙录、修士列传、猫宝和人物信息栏保持独立入口与现有滚动行为；数据刷新不得无故重建滚动节点或改变用户滚动位置。
- 所有玩家可见的新入口、页面、按钮、特征、物品、状态和说明都必须提供中文显示名及 `ch/cz` 本地化键；内部 ID、类名和资源路径保持稳定 ASCII 名称。
- 不因资源动画中存在相邻重复帧而擅自删除资源。

## 版本、打包与 Git 边界

- 日常修改保持 `mod.json` 当前版本，不自动修改版本号、CHANGELOG、正式标签或发布说明。
- 使用 `scripts/Package-Mod.ps1` 生成源码型 Mod ZIP；脚本会校验必需文件、版本号和禁止内容。打包失败时不得保留错误 ZIP。
- 生成的 ZIP 放在 `发布包/`，历史 ZIP 和构建缓存可移动到 `_archive/<时间戳>/`，但不得删除用户数据。
- 除非用户明确要求，不执行 `git push`、远程标签、GitHub Release、Pull Request 或其他远程写操作。
- 修改完成后报告：变更摘要、构建结果或缺失依赖、JSON/本地化检查、源码型 ZIP 路径和包内容校验结果。
- 不为文档或纯配置修改重复执行完整构建；只在任务最终阶段打包一次。
