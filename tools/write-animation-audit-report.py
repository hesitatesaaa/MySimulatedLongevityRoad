"""Write evidence-based offline audit deliverables from the actual scan results."""
import json
from collections import Counter
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'output/animation-audit-20261020-final'


def main():
    data = json.loads((OUT / 'inventory.json').read_text(encoding='utf-8'))
    groups = data['groups']
    clips = [(g, a, c) for g in groups for a, c in g['actions'].items()]
    fallback = [(g['id'], a, c['fallback']) for g, a, c in clips if c.get('fallback')]
    edges = {(f['path']): (g['id'], a, f['name']) for g, a, c in clips for f in c['frames'] if f.get('edgeTouch')}
    geometry = Counter(x['action'] for x in data['geometryCandidates'])
    tests = json.loads((ROOT / 'build/frame-sequence-check.log').read_text(encoding='utf-8-sig').strip())
    compiler = json.loads((ROOT / 'build/animation-nml-check.log').read_text(encoding='utf-8-sig'))
    review = dict(scope='offline', registeredAssets=data['registeredAssets'], resourceGroups=len(groups),
                  clips=len(clips), uniqueImages=data['uniqueImages'], resourceErrors=len(data['errors']),
                  unreferencedImages=len(data['unreferencedImages']), sharedPoseStates=len(fallback),
                  edgeContacts=len(edges), geometryCandidates=dict(geometry), newRedrawnFrames=0,
                  gameVerified=False, tests=tests, compiler=compiler,
                  acceptance='Offline test candidate only; no full animation quality or Unity acceptance claimed.')
    (OUT / 'review-results.json').write_text(json.dumps(review, ensure_ascii=True, indent=2), encoding='utf-8')
    data['visualReview'] = ('Offline contact sheets for all 72 groups and all 18 overview pages reviewed; '
        'phoenix and dafeng GIF playback sampled. Geometry candidates concern attack/death pose or effects; '
        '34 canvas contacts and 81 shared-pose states are disclosed, not certified as resolved. '
        'No claim that all 370 clips received full real-time visual playback review.')
    (OUT / 'inventory.json').write_text(json.dumps(data, ensure_ascii=True, indent=2), encoding='utf-8')

    summary = '''# 0.6.0 新增生物动画离线审查与测试包

本次按“先完成离线审查与测试包”交付。版本日期：2026-10-20。没有进行 WorldBox 游戏内验收，本包是可供游戏内复核的测试候选包，不宣称全部动作质量验收通过。

## 覆盖与证据

- 73 个注册入口：62 组清单资源、10 组天地之魂资源，以及 1 个按元素解析资源的通用化身入口。亚种复用所属物种／形态的同一动画；独立人形、原形和特殊人物逐项列入清单。
- 72 组资源、370 个动作片段、2,278 张唯一源图；注册与资源清单核对无遗漏，引用、哈希、画布、透明通道检查无错误，无未引用图片。
- 逐帧图 72 张、动作 GIF 370 个、切换 GIF 72 个、总览图 18 张。全部组逐帧图已离线审看；凤凰、大风已抽样播放复核。没有将全部 GIF 的成功加载称为全部动作播放已人工验收。
- [选择生物播放](gallery.html)、[全部逐帧与循环](index.html)、[逐项审查清单](01-审查清单.md)、[问题报告](02-问题报告.md)、[重绘与对比](03-重绘与对比.md)、[性能检查](04-性能检查.md)、[最终测试](05-最终测试.md)。

## 本次修改

天地之魂的画面动画改用已有模拟时间缓存，消除按渲染帧计时导致的播放速率随 FPS 改变；重新进入待机从本地动作起点开始；成功施法通知现有攻击动画入口。保留现有伤害脉冲的计时和数值，没有把攻击伤害节奏随同动画改写。

本次没有新增重绘帧。此前已接入的凤凰／大风 28 张待机及移动帧保留，没有重复计算为本次重绘。本次未改动既定体型、锚点、贴图索引或游戏属性。

## 尚未完成的验收

81 个攻击／受击状态共用待机姿态，34 张贴图接触画布边缘，均逐项披露；没有将其标为独立动作完整或裁边问题已全部消除。天地之魂预览使用居中支点，原生加载器最终支点仍需游戏验证。所有生物逐一生成、方向切换、实战攻击／施法／受击／死亡、正常与加速、大量单位、存档兼容、CPU/GPU 和内存实测均未完成。

玩家包沿用既定的压缩方案；审查预览单独打包，避免把预览图片塞进 Mod 增大安装包。
'''
    (OUT / '审查总报告.md').write_text(summary, encoding='utf-8')
    listing = ['# 新增生物动画审查清单', '', '下列每组均已纳入逐帧离线审看及资源校验；“共用”表示既有视觉回退，不能视作独立动作动画。方向由现有渲染器处理，没有虚构额外方向帧。', '']
    for g in groups:
        actions = '；'.join(f"{a}：{len(c['frames'])}帧" + ('，共用' + c['fallback'] if c.get('fallback') else '') for a, c in g['actions'].items())
        listing += [f"## {g['id']}", '', actions + '。', '', f"[逐帧图]({g['id']}.png) · [切换播放]({g['id']}-transitions.gif)。资源校验无错误；游戏内未验证。", '']
    listing += ['## 通用化身 mclsl_world_soul_avatar', '', '根据天地之魂元素使用上列对应资源，无法解析时走现有图标回退。无独立动作帧，游戏内未验证。']
    (OUT / '01-审查清单.md').write_text('\n'.join(listing), encoding='utf-8')

    issues = ['# 动画问题报告', '',
        '## 已修复的配置问题', '',
        '- 中：天地之魂的待机、移动、攻击、受击、死亡播放依赖 Time.frameCount，实际时长随渲染 FPS 变化。替换为已有线程安全模拟时钟的 60 tick/秒时间轴；保留原帧顺序和每帧节奏。暂停、加速实际效果仍待游戏验收。',
        '- 中：天地之魂待机使用全局相位，切回待机不一定从首帧开始，并且每次获取贴图都重写动作时钟。现在只在切入待机或时间回退时设定起点，随后依相对时间选择待机／呼吸帧。',
        '- 中：共享施法提交入口缺少身体动画通知，脱战施法可能仅播放法术特效。成功提交时调用既有 MarkAttack；没有独立攻击资源的状态仍使用既有视觉回退。', '',
        '## 几何候选的离线复核', '',
        f"检测到 {sum(geometry.values())} 处相邻帧几何变化：{dict(geometry)}。均位于攻击或死亡，逐帧图中包括法术扩张、扑击、倒地及消散。待机／移动未触发该阈值；这不证明其动作自然性全部通过。详细帧对和量值见 inventory.json 的 geometryCandidates。", '',
        '火、空天地之魂各有一张死亡末帧完全透明，属于消散终点，保留；其他空白帧仍按错误检测。', '',
        '## 保留待游戏确认的边缘接触（潜在视觉问题）', '',
        '判据为 alpha≥16 的像素接触源画布边界。逐项预览见 edge-1.png 至 edge-6.png；观察到须尖、嘴尖、衣摆和法术拖尾等接触，未据此认定角色主体缺失，也没有声称这些边缘均已修复。', '']
    issues += [f'- {actor} / {action} / {name}：`{path}`' for path, (actor, action, name) in edges.items()]
    issues += ['', '## 现有共用姿态（独立动作资源不足）', '', '以下状态没有独立动作序列；这是本包继续保留的资源限制。完整动作自然性验收仍须逐项决定是否需要绘制独立攻击或受击，不能以调速替代。', '']
    issues += [f'- {actor} / {action} → {source}' for actor, action, source in fallback]
    (OUT / '02-问题报告.md').write_text('\n'.join(issues), encoding='utf-8')

    edge_list = list(edges.items())
    for start in range(0, len(edge_list), 6):
        page = Image.new('RGB', (720, 1080), (30, 35, 42))
        draw = ImageDraw.Draw(page)
        for n, (path, (actor, action, name)) in enumerate(edge_list[start:start + 6]):
            x, y = (n % 2) * 360, (n // 2) * 360
            with Image.open(ROOT / path) as source:
                im = source.convert('RGBA')
                im.thumbnail((330, 300), Image.Resampling.NEAREST)
                page.paste(im, (x + 15, y + 45), im)
            draw.rectangle((x + 15, y + 45, x + 14 + im.width, y + 44 + im.height), outline=(255, 65, 65))
            draw.text((x + 8, y + 6), actor, fill='white')
            draw.text((x + 8, y + 23), name, fill='white')
        page.save(OUT / f'edge-{start // 6 + 1}.png')

    # Compare the actual run frames under old/new timing equations, explicitly offline.
    soul = next(g for g in groups if g.get('folder') == 'Wind_Soul')
    frames = soul['actions']['run']['frames']
    images = [Image.open(ROOT / f['path']).convert('RGBA') for f in frames]
    gif = []
    for sample in range(90):
        t = sample * .02
        tile = Image.new('RGB', (640, 330), (30, 35, 42)); draw = ImageDraw.Draw(tile)
        for col, (label, tickrate) in enumerate([('Before: render clock @144 FPS', 144), ('After: simulation clock @1x', 60)]):
            index = int(t * tickrate) // 6 % len(images)
            im = images[index].resize((260, 260), Image.Resampling.NEAREST)
            tile.paste(im, (col * 320 + 30, 40), im)
            draw.text((col * 320 + 8, 8), label, fill='white')
            draw.text((col * 320 + 8, 308), f'frame {index} | offline timing model', fill='white')
        gif.append(tile)
    gif[0].save(OUT / 'timing-before-after.gif', save_all=True, append_images=gif[1:], duration=20, loop=0)
    (OUT / '03-重绘与对比.md').write_text('''# 动画重绘与对比报告

本次新增重绘为 0 帧，源贴图、尺寸和锚点未调整；修复发生在动画时间轴与施法事件接入。此前凤凰与大风待机各 6 帧、移动各 8 帧共 28 帧继续使用，不能算作本次新增重绘。

[时钟修复前后循环对比](timing-before-after.gif) 使用同一套真实风天地之魂移动贴图：左侧按旧渲染帧时钟、假定 144 FPS；右侧按修复后的 60 tick/模拟秒时钟、假定 1 倍速。两侧均为离线公式重放，非 Unity 录屏，也非游戏 FPS 或性能测试。

所有实际帧和状态切换见 gallery.html。没有用重复帧、水平翻转或改速掩盖已识别的源图结构问题。当前的共用姿态和边缘接触仍作为资源限制披露，不伪称完成了不存在的重绘。
''', encoding='utf-8')
    (OUT / '04-性能检查.md').write_text('''# 性能检查报告

本次源资源数量、源图分辨率、贴图缓存与加载架构均未增加。没有新增运行时贴图生成、纹理复制或每帧数组构造。离线 GIF 及审查图不装入玩家包，不增加游戏贴图加载量。

动画读取使用既有 MclslActorAnimationTime 缓存；long 计时替代 int 以避免时间轴下标溢出，现有字典条目的值类型增大，实际内存变化未测量。待机路径去除了每次取图都覆盖时钟的写入，只在切换／回退时写入。施法提交增加一次现有攻击动画通知；战斗施法可能与既有通知重合，未增加每帧扫描。

压缩发生在归档副本：PNG 最多 256 色，大图标最大边 256，保留既定元数据策略；角色画布尺寸、锚点和 PPU 保留，工作目录原图不改写。量化效果尚需游戏内复核。

CPU/GPU、Unity 纹理驻留、帧时间、100/500/1000 单位和加速运行未实测。磁盘大小与代码检查不能代替这些测量，不作“无性能回退”的实测结论。
''', encoding='utf-8')
    (OUT / '05-最终测试.md').write_text(f'''# 最终离线测试报告

- 注册与资产扫描：{data['registeredAssets']} 个入口、{len(groups)} 组独立资源、{len(clips)} 个动作、{data['uniqueImages']} 张唯一源图；错误 {len(data['errors'])}，未引用 {len(data['unreferencedImages'])}。
- 真实生产帧序列与状态选择代码：{tests['checks']} 项断言，{tests['clips']} 个真实资源动作，30/60/144 FPS 采样验证索引边界、帧可达性、循环和终点；通过。测试直接链接生产 MclslAnimationClipData 和 MclslActorAnimationState，不使用 Mock/Stub。这验证通用帧选择器，不替代天地之魂 Unity 路径执行。
- 安装环境实际 NML Roslyn 4.9 编译器、真实游戏引用：{compiler['sourceCount']} 个生产源码，错误 {compiler['errorCount']}；通过。只是编译通过，未在 Unity 中加载本次包。
- 全部逐帧图离线审看，凤凰／大风播放抽样；完整每状态播放验收仍未全部完成。34 处边缘接触和 81 个共用姿态逐项列明。
- 游戏内逐一生成、形态／方向切换、实战、暂停与加速、存档兼容、大量单位和 CPU/GPU 内存测试：未执行。电脑工具未获 WorldBox 访问授权，用户选择先交付离线审查与测试包。

可复现命令（使用带 Pillow 的 Python）：

```text
python tools/audit-actor-animations.py --output output/<new-audit-directory>
dotnet run --project .dev-tools/frame-sequence-check -- output/animation-audit-20261020-final/inventory.json
python tools/check-nml-sources.py --root . --json
python tools/package-release.py --local-development --output build/<new-package-directory>
```

完整原始检查记录位于本目录 evidence。玩家包哈希和逐文件校验见压缩包同级 package-record.json、player-file-manifest.json、SHA256SUMS.txt。
''', encoding='utf-8')
    evidence = OUT / 'evidence'; evidence.mkdir(exist_ok=True)
    for name in ('frame-sequence-check.log', 'animation-nml-check.log', 'animation-nml-sources.json'):
        (evidence / name).write_bytes((ROOT / 'build' / name).read_bytes())
    print(json.dumps(review, ensure_ascii=True))


if __name__ == '__main__':
    main()
