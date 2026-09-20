# 导入记录

- 来源：0.5.1+我的模拟长生路0.1.4-新法时代优化-最终版.zip
- 原始路径：D:\worldboxmods\0.5.1+我的模拟长生路0.1.4-新法时代优化-最终版.zip
- SHA-256：8F3669C8F658404D2A65272A9F5A68F43D259B345D030059E567FCA158F5AC4D
- 整理日期：2026-09-08
- 操作：去除压缩包外层版本目录，将其内容作为仓库根目录；新增 README.md、.gitignore 和本记录。原包文件未改动。
- 验证：逐文件比对解压内容与 ZIP 条目的 SHA-256；未进行编译或游戏运行验证。

## GitHub 仓库关系

当前仓库目录 `模拟长生路/` 对应模组仓库：

- `https://github.com/hesitatesaaa/MySimulatedLongevityRoad.git`
- 当前分支：`main`
- 该仓库保留自己的 Git 历史；本次未重复克隆或移动它。

为便于接口检索和兼容性分析，导入了 WorldBox 原版反编译参考源码：

- 来源：`https://github.com/hesitatesaaa/WorldBoxGameDecompiled.git`
- 分支：`main`
- 固定提交：`1f42475e2d7e4fcd2595b856662c92f91c22a184`
- 导入日期：2026-09-20
- 导入路径：`references/WorldBoxGameDecompiled/`
- 导入方式：按上述提交的源码归档导入，不保留嵌套 `.git` 目录

该目录只作为源码检索参考，不属于模组运行时源码，不加入 `InterestingTrait.sln`，也不会随玩家发布包分发。`InterestingTrait.csproj` 已明确排除其中的 `.cs` 文件。反编译代码可能包含原版权利人的受保护内容，仓库未附加额外开源许可，建议仅在私有开发环境中使用。
