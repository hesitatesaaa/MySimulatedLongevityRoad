# WorldBox Decompiled Reference

WorldBox 原版游戏程序集 `Assembly-CSharp` 的反编译 C# 源码，仅作为
`MySimulatedLongevityRoad` 模组开发时的接口和行为参考。

## 与模组仓库的关系

- 本仓库保存游戏原版反编译代码。
- 模组实现保存在独立仓库：<https://github.com/hesitatesaaa/MySimulatedLongevityRoad>
- 后续模组修改应提交到模组仓库，不直接改动本仓库中的反编译代码。
- 分析兼容性时，可在本仓库搜索 WorldBox 类型、字段和方法，再在模组仓库中实现
  Harmony 补丁或 NeoModLoader 集成。

常用搜索示例：

```powershell
rg -n "class Actor|updateAge|killHimself" .
rg -n "class MapBox|updateSimulation" .
rg -n "class SaveManager" .
```

## 项目结构

- `Assembly-CSharp.csproj`：反编译工具生成的项目描述，目标为 `netstandard2.1`。
- 根目录：WorldBox 主要游戏类型，如 `Actor`、`MapBox`、`City` 和各种资源库。
- `ai/`：AI 行为与任务。
- `db/`：历史和数据库相关类型。
- `life/`：生命模拟相关代码。
- `UnityEngine/`、`System/`：程序集内包含或生成的相关类型。

项目文件只声明了外部程序集名称，没有提供 Unity、FMOD、SQLite 等依赖的
`HintPath`，因此它主要用于源码检索，不能保证单独编译成功。

## 来源与限制

本仓库由 `worldbox-game-decompiled.zip` 导入，保留压缩包中的目录和源码内容。
反编译代码可能丢失原始注释、局部变量名、项目结构和部分语言语义，不能视为官方源码。

WorldBox 及其代码的权利归相应权利人所有。本仓库未附加开源许可证，也不授予额外的
复制、分发或商业使用权利。建议保持仓库为私有，并仅用于本地模组开发和兼容性分析。
