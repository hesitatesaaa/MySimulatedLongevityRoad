# 根目录整合记录

当前模组版本：0.8.4。

- code、GameResources、NativeActorResources、InterfaceResources、Locales、resources：源码和资源。
- tools：开发、打包和验证工具；构建输出统一使用 build。
- .tests：测试项目，避免被模组加载器编译。
- .dev-tools：玩家包排除的开发编辑器源码。
- docs：开发文档；docs/history：历史记录。
- .archive/snapshots：旧优化目录完整压缩快照和逐文件 SHA-256 清单（16,590 文件，已逐个验证）。

旧优化目录所有内容已删除，但空目录被 Windows 占用，目录本身暂时无法删除。旧上游目录的部分 .git/build 文件仍因访问限制保留，其余内容在 .archive/upstream-0.6.0。

检查：NML 源码扫描通过（284 个源码文件）；本地化和灵兽天赋资源验证通过。完整资源验证发现既有 kunpeng idle_0.png 与 actor-frames 清单或来源证明哈希不一致，未篡改来源证明以掩盖问题；尚未完成 Unity 运行验收。

本次整合尚未提交 Git。
