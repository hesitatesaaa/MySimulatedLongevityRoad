# 0.6.0接续交接（2026-10-15，当前生效）

此文件替代旧聊天派发流程和历史矛盾条款。用户要求在新主对话继续，旧主对话停止绘图；当前用户要求改为主对话直接调用内置imagegen；先收齐当前子进程已提交结果，此后不再使用子进程绘图，旧子进程上下文与记录保留。既定画风和尺寸不再反复推翻。

## 实际断点
- 迁移Git检查点192cc49；仅本地仓库，不上传。候选1165动画、24图标；生产接入18套592动画、14图标。releaseReady=false。
- 真实缺项228项：output/actor-redraw/balanced-resume-after-quota.jobs.json（214动画、14图标）。恢复后四轮子进程128项及主对话16项已全部完成并minimal汇总；第二轮曾被Codex用量/自动审批限制中断保存，32项旧imagegen全部成功返回并恢复，无重复绘图。旧批索引不再适用，不另建results等报告。
- 2026-10-15的imagegen HTTP429 usage_limit_reached 已在2026-10-16 03:47:07 UTC后解除停派；恢复后144项全部返回无429。当前三个绘图工作进程已完成分片、空闲保留上下文，无在途；继续前复查pipeline.control.json、磁盘输出及provenance。
- 进度读output/actor-redraw/progress.json；生产清单resources/actor-frames.manifest.json；目标resources/actor-redraw.targets.json及actor-generation-scope.json。目标62形态、38图标，1374帧为当前制作目标，缺失数以真实队列为准。

## 工作方式
- 本主对话直接调用内置imagegen持续绘图，不再spawn或followup派发子进程绘图；旧 /root/round3_a、/root/round3_b、/root/round3_c 保留上下文与记录、空闲待用。先收齐当前在途结果，再从真实缺项队列续绘。不创建新绘图聊天。
- 主对话只处理真实缺项及相关高清参考，先查看参考，调用内置imagegen，用户允许同次请求多个角色的多个独立透明PNG，每个文件仅一角色一帧，立即原样留存；每个输出逐一绑定队列项、参考和来源SHA。仅在工具实际返回多个独立文件时采用多图接收；若只支持单图则分开调用，不将合图裁切成帧。保留原图/候选/必要provenance，按既有流程更新共享队列和进度，不夹带并行玩法修改。
- 旧子进程保留上下文与记录，不再派发绘图。只清理已不使用的本轮专用临时文件（先验证路径），不得删子进程记录、图片、参考、provenance、Git或既有成果。
- 只保留必要来源及既有进度，不新增每批jobs/results/notes/review/reconciled/prepared/队列备份。后续用prepare-next-actor-batch.py --in-place --count 32，复用共享队列；全部在途结束后才能改其索引。retain-native-actor-frame.py支持队列内提示词。静态观察合并同一provenance.staticReview，必须绑定sourceSha256、注明实际审看者（主对话或历史worker），完整动作未通过不得写通过。主对话可用reconcile-native-batch.py --minimal --notes -从标准输入汇总，不落额外报告文件。
- 任一真实绘图额度错误立即全局停派、等在途保存；同一个0-6-0任务按服务恢复时间+约30秒续接。不要伪造恢复时间。返回savedPath的图片先恢复，避免重复绘图。

## 不变的制作标准
- 仅0.6.0制作资源，不扫描/修改沿用0.5.4图片。内置imagegen，不用API/CLI、程序简画、多帧图切割或旧帧变换。每帧独立新绘。
- 原生PNG字节、分辨率保持不变，不缩放、裁切、重编码或mipmap；只有必要时可清理已授权alpha<16杂点并留来源。画风、性别、身份见actor-art-direction.json和actor-native-resolution-policy.md，不追求更复杂装饰。
- 人形左右脚贴地大跨步，近脚前/远脚后，再远脚前/近脚后；自然反向摆臂，禁止高抬膝/双靴收拢到身下。见actor-human-walk-policy.md。貘、红毛、飞球化形各2张高抬膝帧已独立替换，human-walk-revisions-20261015.json保留旧SHA与新SHA；单帧查看完成，整段交替及循环未批准。旧“缺少抬膝过步”的判错作废，不重复重绘。
- 只新增idle/walk/death/icon，已取消attack/hit整段不补齐，已有合格战斗帧保留，必要时idle视觉兜底。用户允许终审修复已有明确缺陷。
- 完整肢体/关节/轮廓，无多余手脚、翅膀，无裁断。32px留白是建议，完整小留白不能单独判废。保持原有世界缩放，不能通过改PNG像素调整大小。

## 尚未完成的必要审看与验收
- 全部绘图返回后做一次62形态/38图标完整终审：真实帧顺序、循环、腿/翼/游动交替、死亡收势、身份/肢体/裁断、主体尺寸及图标占比；只针对具体路径和sourceSHA明确缺陷独立返工，复看相邻帧。不因文件齐全自动安装。合格来源记录和此前静态检查不等于完整终审。
- 已知待确认：青神龙walk_6（SHA17b789b8abf9b100559387af938767a1257ab7285c8e60b12184473474a28e32）主对话明确看到5只爪，须独立修复；青神龙walk_4（SHA300dc5ec5d6c5a6773263485be167c9c67620ddc799e9716b1911b3c117c592b）主对话明确看到5只爪，须独立修复；主对话idle_4/5红黄杂点实测alpha1..3，勿只凭查看器颜色判需重画；帝叁貘兽形death_4右侧獠牙尖触边疑似截断、walk_4远前腿连接不清；帝叁貘化形death_1→2疑似回抬及边缘色边；青神龙idle_1..3颈部空隙彩色杂斑，均见对应provenance.staticReview；帝一化形walk_0/1发间鲜红色块、近远腿交替、death_1→2头高及death_3/4红边；帝一兽形死亡卷尾收势，均记录在对应provenance.staticReview；天兽化形头发空隙红黄斑点、步行近远腿交替不明、death_1→2头部疑似回抬；天兽兽形步行远腿遮挡和死亡收势，详见各自provenance.staticReview；狗首兽形death_0→1尾部回抬、death_4→5头部略回抬，记录在对应provenance.staticReview；狗首化形步行近远脚交替不明、death_1→2和death_4→5疑似回抬；裂界鲸化形步行近远腿交替不明、death_1→2疑似回抬；蛮蛮化形移动缺腿交替、death4→5回抬；墨麒麟死亡收势仍待全段审看；红毛兽形death1→2尾部回抬记录在parent-resume-20261015-02.review.json。红毛化形death5裁边已独立修复，不重复归档；旧六项修正不得再次--activate。
- 最终通过证据只写output/actor-redraw/final-visual-review.json：scope=0.6.0-only、最终manifestSha256、全部62形态ID/38图标ID、问题及修复、unresolvedDefects=[]、passed=true；必须实际完整查看和修正后才填写通过。
- 原生缓存曾实测72张1254纹理、单mip/CPU不可读，缓存计数431.90MiB释放归零；GPU仪器不可用，不等于GPU零占用或长时无泄漏。18套单位尺寸采集曾崩溃，不能说通过。实际Unity尺寸、按钮占比、必要内存/性能、暂停加速与新存档验收仍须完成，旧存档不验收。只做解除当前阻塞或发布必需检查，不重复无关测试。
- 仓库另有用户并行玩法任务。只提交本轮视觉文件，禁止夹带其他C#、GameResources道具/法术、build-release或automation-payload.py已有外部改动；不重置别人文件。

定时任务规范为resources/automation-0-6-0.prompt.md；迁移目标与控制以pipeline.control.json的generationThreadId/handoffReady为准，旧聊天不再绘图。
