# 玄黄 UI 图片资源

资源目录：`GameResources/ui/XuanhuangSkin/`。

本次有七个主界面的独立透明 PNG 边框和标题，共 14 张。文件名与准确标题文字见同目录的 `manifest.json`。所有边框中央预留透明内容区，不烘焙正文、按钮或动态数据；普通控件由 UI 代码绘制。

| 界面 | 边框 | 标题 |
| --- | --- | --- |
| 天玄镜·万界交易所 | `frame_tianxuanjing.png` | `title_tianxuanjing.png` |
| 我的模拟长生路·模组介绍 | `frame_mod_intro.png` | `title_mod_intro.png` |
| 玄黄修士榜 | `frame_ranking.png` | `title_ranking.png` |
| 还真空间 | `frame_huanzhen_space.png` | `title_huanzhen_space.png` |
| 修士档案 | `frame_character_panel.png` | `title_character_panel.png` |
| 修士列传 | `frame_biography.png` | `title_biography.png` |
| 玄黄仙录 | `frame_xuanhuang_record.png` | `title_xuanhuang_record.png` |

修仙家族页面取消独立艺术字和独立边框，统一继承玄黄仙录主框架；“山河万象 · 修仙家族”为动态文字。

图片由内置 imagegen 逐张生成。标题的最终文字已人工目视检查；接入实际界面时仍需核对大小、关闭按钮位置和屏幕缩放效果。
