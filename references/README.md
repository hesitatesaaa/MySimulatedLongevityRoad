# 第三方参考源码

本目录保存供模组开发使用的第三方参考源码。参考源码不属于模组运行时实现，不加入模组解决方案，也不随发布包分发。

## WorldBoxGameDecompiled

- 来源：<https://github.com/hesitatesaaa/WorldBoxGameDecompiled.git>
- 分支：`main`
- 固定提交：`1f42475e2d7e4fcd2595b856662c92f91c22a184`
- 导入日期：2026-09-20
- 用途：检索 WorldBox 原版 `Assembly-CSharp` 类型、字段、方法和行为，辅助编写 Harmony 补丁与 NeoModLoader 集成。

该仓库的 `Assembly-CSharp.csproj` 主要用于源码检索，缺少完整的 Unity、FMOD、SQLite 等本地程序集引用，因此不要把它作为模组编译依赖。后续模组修改提交到上层模组仓库，不直接修改这里的反编译文件。

WorldBox 及其代码的权利归相应权利人所有。本目录未附加额外开源许可证，也不授予复制、分发或商业使用权利；建议保持仓库私有，仅用于本地兼容性分析。
