# 0.6.0接续交接（2026-10-15，当前生效）

## 2026-10-19 Current release checkpoint (supersedes historical test plans below)

- User accepted the existing tests: do not run additional Unity tests or image reviews.
- Version is 0.6.0. Formal Release compilation succeeded; resource gates passed with 1374 frames, 62 forms and 38 actor icons.
- All 9 latest clearly mapped, non-rejected generated_images repairs are integrated, in addition to earlier review repairs. Original PNG bytes and rollback evidence are retained.
- Actual native 100/500/1000-unit 120-second measurements: 29.97/59.62/56.70 FPS; native coverage minimum 100% in each scenario. Actual continuous observation was 1140.96669 seconds (about 19 minutes), not a completed 20-minute pass. Final matched baseline and additional exhaustive sweeps were not rerun.
- Separate actual 1000-beast save reload passed after the ordinary-beast age-sanity fix. Release supports new saves only.
- Explicit scoped acceptance and immutable evidence hashes: resources/runtime-acceptance-0.6.0.json. No raw test result was rewritten as a pass.
- Formal build: build/Release-0.6.0-final/MySimulatedLongevityRoad.dll. Packaging and publication are the remaining steps; local branch is 0.6.0.

## 2026-10-19 用户最新指令（历史运行断点，以上述最新检查点为准）

- 最新运行断点：`gameplay-stable-runner-observation.json` 受控玩法完成，21化形/48图集/7级兽潮/3炼制/4交易/16死亡路由及背包、配方、未完成兽潮磁盘恢复均无错。随后真实1000妖兽存档重载暴露普通妖兽被人类年龄规则降阶的缺陷，`MclslCultivationAgeSanity` 增加普通妖兽豁免；用同一存档的 `save-reload-age-fixed-observation.json` 实测1000角色编号/形态/境界全部恢复，0失败。
- 性能初测 `baseline-matched` 可见数0，无效；`baseline-visible` 同因低精度地图模式失败。`baseline-full-detail` 100/500/1000档自然可见数均等于放置数，平均29.73/59.63/57.15 FPS，但最后新档重载失败（即上述年龄缺陷），且早于最终基线缩放/60秒预热调整，不能直接用于最终验收。需用当前修正代码重测最终基线。
- `native-full-detail` 的1GiB缓存实测100单位40秒时原生覆盖率仅85%，已主动结束并保留证据。生产缓存现为2GiB；`native-2g-full-detail` 正在运行，100/500/1000三个120秒稳定测量均已完成，平均29.97/59.62/56.70 FPS，三档原生覆盖率最低值均100%，自然可见数最低分别100/500/1000。已继续运行超过15分钟，最初1000单位全存活，当前自然可见1082，显存约3.2GiB。未完成20分钟/新档重载/最终基线对比前不得写整体通过。性能夹具仅提高自动低精度地图切换阈值、隐藏日志面板，AI/自然可见性/正常渲染流程保留；新旧版本共用固定空地图和renderScale=1的相同世界高度。
- 用户已两次确认窗口权限，并最新确认“已解锁并保持解锁”。computer-use已读，可用node_repl的@oai/sky；activate_window超时，但直接get_window_state和截图坐标关闭日志面板成功。若再次看见锁屏，停止窗口输入并让用户解锁；不得代解锁。无需重新审看资产。

- 继续执行补充：`size-cache-observation.json` 与 `size-realm-observation.json` 都只采到45/69，不能验收。三具名人型已注册且非加载失败；最终放置状态回兽形，原因是诊断新生角色未满足元婴150岁年龄门槛。尺寸夹具改为200岁及生产境界赋予入口，`size-mature` 完整采到69/69，但小缩放受原生±0.01尺寸加减影响，轮廓高度比较失败。随后同比调整PPU与renderScale（全部renderScale=1），保留原图字节和设计世界高度；`size-normalized-observation.json` 完整69/69，62形态实际轮廓高度比较通过，最大偏差约3%，证据在同名 `-size-comparison.json`。仅尺寸/纹理配置通过，未通过整体发布验收。
- `tools/verify-runtime-sizes.py` 只接受完整69单位实际放置记录及匹配的测试清单，比较参照alpha轮廓高度、原生SHA/画布/PPU/纹理设置，不能替代性能或解剖审看。性能夹具已改为基线保存空地图、新图读取同一地图，准备阶段暂停，测量阶段正常AI/渲染；`baseline-matched` 已启动。参考模组从隔离game/mods移到同目录 `reference-disabled`，不影响原装游戏；稳定参照PNG仍在 `output/runtime-validation/fantasy-reference-used`。

- 用户取消后续图片审看，要求直接接入、实机验收后生成完整开发者包和玩家包，并发布到 `hesitatesaaa/MySimulatedLongevityRoad` 的 `0.6.0` 分支。不要恢复绘图或图片终审，不创建重复自动任务。
- 已全量原生接入 62 套形态、1374 张目标动画帧、38 个图标。首次接入新增/替换 769 帧；另从 generated_images 和生成记录补入 9 张未明确判废的最新修复图，记录在 `output/actor-redraw/generated-repairs-integration.json`。旧帧、证明和已判废重试均保留，不把取消审看写成审看通过。
- 接入规则由 `resources/0.6.0-release-policy.json` 管理。`integrationAccepted`、`geometryConfigured` 允许直接接入，不伪造历史 `approved`、`anchorReviewed` 或 Unity 验收。尺寸参数基于实际放置参照单位提出修正，尚需最终 Unity 复测。
- 本地分支已建立为 `0.6.0`，远端 origin 已配置；GitHub 连接、账号及 push 权限已只读确认。尚未推送、创建 Release 或生成正式双包，版本声明仍为 0.5.4，releaseReady=false。
- 运行证据目录为 `output/runtime-validation/release-20261019`。早期 b/c 尺寸报告仅采到 59/69 和 66/69，不通过；d 轮因残留隔离进程叠加内存而失败，也不能验收。残留进程已按路径核对并结束；后续由 `tools/release-runtime-probe/run.ps1` 管理单实例及 finally 退出。
- 动画/缓存逻辑 534 项、妖兽目录/配方 692 项、死亡表现替身 804 项通过；这些均不是 Unity 尺寸、CPU/GPU 内存或人口性能验收。正式发布仍须完成完整实机、100/500/1000 单位、二十分钟持续运行及新存档验证。
- 双包入口为 `tools/package-release.ps1`，Python 需 Pillow，可显式传入 `-PythonExecutable`。打包保留正式资源/实机门槛；ZIP64、分卷、CRC 和 SHA256 已实现。发布入口 `tools/publish-github-release.py` 只针对上述授权仓库，先验证附件再公开 Release，凭据不写入产物。

此文件替代旧聊天派发流程和历史矛盾条款。当前用户要求恢复本任务内的绘图子进程，指定gpt-6-luna、max推理，最多3个并行；不创建独立绘图聊天。旧子进程上下文与记录保留。既定画风和尺寸不再反复推翻。

## 最新检查点（2026-10-18，本节优先）
- 基础缺帧队列0项；现有1379帧/38图标，目标1374帧/38图标。38图标已逐张终审并原生接入。完整形态离线终审22/62，已接入33/62，其中原生24套；releaseReady=false，未向远端推送。
- 完整通过ID及sourceInventorySha256只读output/actor-redraw/final-visual-review.json的reviewedForms；对应资源已接入。最新完整通过并原生接入大风、天兽、朱雀、裂界鲸；大风attack_5右翼硬裁已有原生修复及来源证明。记录脚本现强制目标帧数与原图可见边框预检。
- 本轮9帧有效原生修复已安全入库，详见同一final review的repairs及各制作帧provenance.repair；旧原图/暂存/证明在output/actor-redraw/rejected/final-review-20261018。白虎death_10原图已从回滚恢复：该形态播放顺序从death_10开始，站立首帧合格；误画的卧倒候选保留但不采用。玄天王death_2_v3和大风attack_5修复已入库；白虎45帧和青鸾54帧已完整终审并原生迁移。后文青神龙多爪/右须等旧缺陷已修复，不重复绘制。红黄杂点用户接受。
- 当前forms_a负责目标索引0..20，forms_b负责21..41，forms_c负责42..61，均gpt-6-sol低推理。按整形态/多形态尽可能32–64张原图批量emit并审看；仅真实完整结果计通过。主要未解是多个人形walk近远腿不交替；同法两次失败先换角色，合格反相姿态参考共享。结果先保存，失败件留rejected与来源。
- 接入复用tools/record-final-form-review.py（必须--frame-count核目标）、review-native-form.py、install-native-reviewed-form.py；修复复用promote-reviewed-native-repair.py，先--check。地面锚有尾尖例外时记录实际身体接地点；飞行相位保旧sourceAnchor，伏地死亡按接触轮廓。所有geometry与尺寸仍仅离线提案。
- 主进程已完成新隔离目录 output/runtime-validation/visual-final-20261018/compiler 的开发编译（当前声明仍0.5.4，不是正式发布）；运行脚本支持用 -RunDirectory 指定新隔离目录，语法及越界目录拒绝检查通过。尚未复制最终资源或运行Unity，不得视为实机通过。
- 尚需剩余40形态完整终审/明确缺陷修复、剩余原生迁移、最终清单核验及Unity新存档实际尺寸/按钮/100/500/1000单位与必要性能验收。旧size probe在窗口预载时原生崩溃，现有population probe只测暂停强制可见渲染管线，不能当30/60/144FPS或完整玩法验收。无游戏验收通过记录。避开并行玩法代码；最终全部完成才推GitHub的0.6.0分支。

## 实际断点
- 2026-10-17子进程重连诊断：三旧子进程虽显示running，但无新审看落盘且即时回报/单进程纯文本探测未返回；已全部中断并保留上下文，勿据running声称正常工作。账户接口ordinaryUsageAllowed=true，未确认具体连接原因；当前主对话继续终审。主对话已对照17个图标原图与身份参考，进度及SHA见唯一final-visual-review.json（passed=false）；其中0..3尚待alpha边缘核查，其余13..25已核，无确认废图，按钮占比未实机验收。
- 迁移Git检查点192cc49；仅本地仓库。绘图、终审、接入和实机验收全部完成前不向Git远端推送或上传。候选1379动画、38图标；生产接入18套592动画、14图标。releaseReady=false。
- 真实缺项0项：output/actor-redraw/balanced-resume-after-quota.jobs.json已清空。本轮三个gpt-6-sol低推理子进程独立绘制162项，全部原生保存、SHA核对、逐帧静态审看并minimal汇总；完整动作终审尚未通过。
- 2026-10-17额度已恢复，三子进程完成全部162项无新429，当前无在途绘图。下一阶段是62形态/38图标完整终审、明确缺陷返工、接入及新存档Unity实机验收。
- 2026-10-18主对话已完成38图标逐张来源绑定终审，并将24张待接入图标按原生字节接入；manifest图标38/38 approved，逐张复制SHA一致。按钮占比与游戏内尺寸仍未实机验收；形态仍18/62接入，releaseReady=false。
- 本轮已按来源SHA核验并入库青神龙walk_4/walk_6/death_9、帝叁貘兽形death_4、蛮蛮化形death_5、红毛兽形death_2六帧；旧原图/暂存/证明在output/actor-redraw/rejected/final-review-20261018，候选及提示词仍在repair-candidates。白虎兽形death_10的1254px候选已保存，但现有暂存/生产为96px旧规格，整套原生迁移前不覆盖。三绘图子进程仍在修人形步态等问题；初筛接触图不能冒充逐张原生终审。
- 玄天王walk_0..5均右脚领步，缺近远腿交替；death_2站立到death_3跪地高度突变。已在各帧source SHA绑定的staticReview记录，完整终审时针对性修复，勿重复生成其他合格帧。
- 进度读output/actor-redraw/progress.json；生产清单resources/actor-frames.manifest.json；目标resources/actor-redraw.targets.json及actor-generation-scope.json。目标62形态、38图标，1374帧为当前制作目标，缺失数以真实队列为准。

## 工作方式
- 加速与省额度：每次工具调用尽可能查看整套形态全部独立原生PNG，小形态合并两至三套，争取32–64张并逐图标注形态/动作/帧；只有工具实际输出或上下文限制才拆批。疑似缺陷再局部单张复看；缩略接触图只作初筛。以targets真实帧顺序/总数核对，不读旧manifest帧数代替。常规事项自主继续，仅汇报完整通过ID+目标总数、可用候选路径/SHA及真实阻塞。单角色步态连续两次同法失败先换角色，首张合格反相步态供三组共享姿态参考。主进程将通过后的来源核验、审核记录、接入合并批处理；不重复全库扫描。
- 额度恢复后本任务使用最多3个并行绘图子进程，指定gpt-6-luna、max推理；每进程图片请求数与累计张数不设上限。旧子进程若可保持指定模型则复用，否则保留记录并使用符合指定模型的子进程。不创建独立绘图聊天。
- 主进程按真实缺项分配互不重叠的资源与高清参考；子进程只写分配资源，不改共享队列、进度或控制文件。仅内置imagegen，每个独立透明PNG仅一角色一帧，返回后立即原样留存并绑定队列项、参考和来源SHA。仅在工具实际返回多个独立文件时采用多图接收；若只支持单图则分开调用，不将合图裁切成帧。主进程汇总审看、更新共享队列并续接，不夹带并行玩法修改。
- 只清理已不使用的本轮专用临时文件（先验证路径），不得删旧子进程记录、图片、参考、provenance、Git或既有成果。
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
- 本轮新增待修：青神龙death_9右须在原生画布右边缘有alpha>=128像素，需独立修复；青神龙化形walk_0..5近远腿交替不足、前脚姿态近似重复，需完整循环终审并针对具体帧修复。对应来源SHA和静态审看保存在各帧provenance；青神龙化形death_0..5已保存，完整死亡收势未审，death_4/5的alpha>=128主体距右边分别13/21像素，未判硬裁断。
- 天一人形walk_0..5前脚姿态重复、近远腿交替不足；death_0..2下沉幅度小，death_3突然落地，需完整动作终审并针对具体帧修复。idle_1..3、walk_0..5、death_0..5和icon_0已保存并绑定来源SHA；发间红/紫色需alpha检查，图标按钮占比未验收。
- 墨瑞彬idle_1..3、walk_0..5、death_0..5和icon_0已保存并绑定来源SHA；walk六帧几乎重复同一前后脚关系，完整循环未通过，须终审时针对具体帧独立修复。死亡收势和图标按钮占比仍待最终审看。
- 原生缓存曾实测72张1254纹理、单mip/CPU不可读，缓存计数431.90MiB释放归零；GPU仪器不可用，不等于GPU零占用或长时无泄漏。18套单位尺寸采集曾崩溃，不能说通过。实际Unity尺寸、按钮占比、必要内存/性能、暂停加速与新存档验收仍须完成，旧存档不验收。只做解除当前阻塞或发布必需检查，不重复无关测试。
- 仓库另有用户并行玩法任务。只提交本轮视觉文件，禁止夹带其他C#、GameResources道具/法术、build-release或automation-payload.py已有外部改动；不重置别人文件。

定时任务规范为resources/automation-0-6-0.prompt.md；迁移目标与控制以pipeline.control.json的generationThreadId/handoffReady为准，旧聊天不再绘图。

## 独立会话接续（2026-10-19）
- 已创建全新本地会话 01a1507b-8feb-7ca2-86e8-43ae2230195f，在同一仓库独立读取代码、来源、Git和验收文件恢复任务；不继承旧聊天结论。创建前磁盘progress.json仍为releaseReady=false，不构成最终验收通过。当前旧任务无活跃子代理；后续工作由新会话推进，未执行远端推送。


## 独立只读复核（2026-10-19 06:48，北京时间；会话01a15130-a720-71a3-b9dd-7d6648bc6977）
- 根AGENTS.md已重新读取。工作区初查1054项变更，main分支，HEAD短SHA 4f83738；git remote -v无输出，未配置GitHub远程。未提交、未推送。
- 独立按manifest逐项核对900张已批准动画及38张已批准图标：生产SHA=manifest SHA=provenance outputSha256，来源文件SHA=sourceSha256，错误0。未重绘、未迁移资源。
- 对final-visual-review.json全部23套运行record-final-form-review.py --check，核对记录帧数、原图副本、可见边界和sourceInventorySha256，错误0。这是结构与来源核验，不是新增视觉审看。progress.json仍记22套，摘要滞后；62套目标剩39套终审。未覆盖共享进度或批准任何新形态。
- test-beast-resource-inspection.py六项通过；test-automation-payload.py通过ASCII传输、中文往返及损坏文本拒绝回归。替身测试不算Unity验收。
- output/beast-overhaul/unity-validation.json不存在；真实尺寸、按钮、100/500/1000单位及CPU/GPU验收仍待完成。游戏程序存在，但output/runtime-validation/visual-final-20261018仅有compiler，无MySimulatedLongevityRoad可运行副本。现有run-visual-runtime-validation.ps1仅运行缓存/尺寸探针，不能替代完整玩法性能验收。
- 应用列表显示同目录另有活跃会话01a1507b-8feb-7ca2-86e8-43ae2230195f与源会话；未读取旧聊天内容。已向用户询问写入归属，未确认前避免并行改共享资源。23与22差异来自文件摘要不同，不能据此声称本轮有并发写入。
- 本机仅发现automations/automation/automation.toml，状态ACTIVE，名称“继续之前的任务”，每5小时20分续接；与beast-overhaul-status.json所写PAUSED不一致。未创建、修改或删除定时任务，保留待归属核实。终端普通沙箱MXC启动失败，只读核验通过受控提权执行。
- 后续先确认写入归属，再接续39套终审和必要修复/原生迁移，更新滞后摘要；构建隔离验收副本并取得真实Unity证据。全部通过前releaseReady保持false，最终提交推送还需用户提供对应GitHub远程地址。
