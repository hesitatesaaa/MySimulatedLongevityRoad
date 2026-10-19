# 0.6.0 开发说明

完整开发者包包含运行源码、资源、制作原图、来源、回滚资料、构建工具及实际运行报告，不附带 WorldBox 或其依赖 DLL。

准备自己的 WorldBox / NeoModLoader 依赖、.NET SDK，以及安装 Pillow 的 Python，执行 tools/build-release.ps1 -RequireFinalAssets；可使用 -PythonExecutable 指定 Python。游戏在运行时由 NeoModLoader 编译源码，玩家包不重复附带编译 DLL。

tools/package-release.py 从同一发布提交生成玩家包和开发者包，校验资源、ZIP CRC 和 SHA256。完整原图校验需要开发者包中的 output 资料。

现有实测范围由 resources/runtime-acceptance-0.6.0.json 记录。用户已接受该范围并停止追加测试，未完成的 20 分钟流程与未重跑基线不能标记为通过。保留本地原始证据，不用离线估算替代实际 Unity 记录。
