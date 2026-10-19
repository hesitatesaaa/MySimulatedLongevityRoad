# 项目工作约定

## 定时任务中文编码

更新现有任务0-6-0时，必须使用 tools/automation-payload.py 导出参数，再将控制台ASCII JSON通过JSON.parse还原后传给应用的automation_update工具。不得读取未经转义的中文控制台输出后重新提交；禁止ensure_ascii=False用于此传输路径。完整提示词由UTF-8文件 resources/automation-0-6-0.prompt.md 管理，更新提示词先编辑此文件。

每次工具更新成功后，运行同一脚本 --verify，严格检查已保存提示词与规范文件相等、无U+FFFD或空字符。验证失败时立即修复，不能宣称任务已更新。修改时间仅用--rrule生成参数，不直接写automation.toml，不新建重复任务。保留现有通知设置。

## 验收记录

离线图片审看、替身测试及内存估算不能标记为游戏内尺寸、CPU/GPU内存或100/500/1000单位实测通过。需要实际Unity运行记录及放置单位证据。
