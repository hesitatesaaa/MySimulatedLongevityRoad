# 全面修复进度（2026-10-25）

## 基线
- 已读根目录 AGENTS.md。保留当前全部未提交成果，不推送。
- 当前 mod.json 为 0.7.0，沿用现有版本命名，不回退成果。
- CodeGraph CLI / MCP 不可用，采用定向源码搜索。
- 玄黄族基线：19 项独立建筑（住宅6、主厅3、风车2、其他8），公共建筑6项共用。

## 当前阶段（可接续）
1. 已读约定并确认基线；所有既有未提交修改保留。
2. 已解码3085张PNG，694映射帧+720职业效果帧完成几何审计，当前0错误/0贴边。
3. 建筑清单25项，住宅6、主厅3、风车2、其他14；23独立+2指定共用。
4–7. 原19项已重绘，雕像/井/矿井/木桩4项补齐；篝火/仓储广场根据后续要求原版共用。施工/完整/废墟原画及风车固定轴动画已导入。
8–13. 代码/资源修改已完成。另修复实测发现的跟随延迟关闭、本地化更新缓存和栏目导航引用问题。
14. 最终输入5c2e511b3b0d75d32996b608810229ef0e4145dede6a5107f60b899882e1a832：开发者专项Unity440项、玩家包17项检查均通过，0错误。运行记录build/runtime-070/20261025_064601和20261025_064447；结果已复制至resources/full-repair-runtime.json及full-repair-player-runtime.json。两级风车在真实城市归属下播放4帧，采集了原生相机及人物UI画面。
15. 已完成最终压缩发布：build/full-repair-release。玩家包71918679字节/68.59 MiB；开发者包78247243字节/74.62 MiB。版本0.7.0，player + compress-assets；两个包均检查结构、关键资源、源码和权限边界。原素材保留，未推送GitHub。
16. 已完成报告resources/full-repair-report.md；发布路径/大小/哈希见resources/full-repair-release.json，交付目录也有full-repair-release.json、package-record.json和SHA256SUMS.txt。所有阶段已有实际文件和记录，人工/长期未验收范围已单列。

## 资源原件
- 原建筑：.dev-tools/combat-art/xuanhuang-before-front/
- 新建筑三套原画：.dev-tools/combat-art/xuanhuang-{front,construction,ruins}-20261025.png
- 特效原图：.dev-tools/combat-art/career-effects-before-padding/
- 重建脚本：tools/rebuild-xuanhuang-front.py、tools/pad-career-effect-atlases.py

## 验收边界
任何静态、离线或编译通过均不代表 WorldBox 游戏内验收。实机未做的项目必须写明。
