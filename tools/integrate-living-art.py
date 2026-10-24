"""Import reviewed living/career artwork; operate only on derived project copies."""
import argparse
import hashlib
import json
import shutil
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / "resources/living-resources.json"
CAREERS = ("sword", "martial", "elemental", "sigil", "array", "spirit")
# Source indices are the stable alphabetical order of the original 110 files.
# Each assignment was reviewed against the skill's name and visual motif.
ICONS = {
    "sword": ([93, 17, 18, 20, 91], [26, 36, 56, 80, 92], 14),
    "martial": ([97, 25, 45, 62, 83], [95, 40, 68, 79, 7], 63),
    "elemental": ([57, 42, 31, 4, 52], [67, 15, 9, 71, 16], 35),
    "sigil": ([76, 72, 103, 50, 74], [98, 60, 85, 43, 32], 19),
    "array": ([47, 81, 73, 54, 39], [87, 86, 96, 99, 51], 39),
    "spirit": ([65, 88, 30, 77, 27], [78, 88, 30, 77, 61], 27),
}
EFFECTS = {
    "sword": [106, 59, 104, 5, 75],
    "martial": [None, 70, 84, None, 23],
    "elemental": [41, 48, 66, 33, 109],
    "sigil": [49, 94, 10, 82, 55],
    "array": [6, 21, 108, 29, 28],
    "spirit": [100, 11, 13, 22, None],
}
CHARACTERS = {
    "male_1": [102, 58, 34],
    "female_1": [8, 101, 64],
}
STATUSES = ("已接入", "可加工接入", "需要补绘", "明确弃用")


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_manifest(data):
    MANIFEST.write_text(json.dumps(data, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")


def load_manifest():
    data = json.loads(MANIFEST.read_text(encoding="utf-8-sig"))
    return {"Groups": data, "Sources": [], "Outputs": []} if isinstance(data, list) else data


def save_image(image, relative):
    path = ROOT / "GameResources" / (relative + ".png")
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)
    return path


def swimming(source, asset, references):
    """Import one independently generated identity, with 3 directions x 4 strokes."""
    data = load_manifest()
    image = Image.open(source).convert("RGBA")
    assert image.getextrema()[3][0] == 0, "Swimming source must be transparent"
    # Remove only low-opacity generation glow; never synthesize a walking crop.
    image.putalpha(image.getchannel("A").point(lambda a: a if a >= 160 else 0))
    rows = row_bounds(image, 3)
    raw = ROOT / ".dev-tools/combat-art/0.7.0" / ("swim_" + asset + ".png")
    shutil.copyfile(source, raw)
    cells = []
    for top, bottom in rows:
        for column in range(4):
            cell = image.crop((round(column*image.width/4), top, round((column+1)*image.width/4), bottom))
            box = cell.getchannel("A").getbbox()
            if box is None:
                raise ValueError("Empty swim stroke")
            cells.append(cell.crop(box))
    child = asset.endswith("_child")
    height = 9 if child else 13
    scale = min(height / max(c.height for c in cells), 26 / max(c.width for c in cells))
    outputs = {o["Resource"]: o for o in data["Outputs"]}
    for direction, suffix in enumerate(("", "_front", "_back")):
        group = next(g for g in data["Groups"] if g["Path"] == "actors/species/civs/mclsl_xuanhuang"+suffix+"/"+asset)
        atlas = group["Atlas"]
        sheet = Image.open(ROOT / "GameResources" / (atlas+".png")).convert("RGBA")
        mask = Image.open(ROOT / "GameResources" / (atlas+"_robe_mask.png")).convert("RGBA")
        for frame in group["Frames"]:
            if not frame["Name"].startswith("swim_"):
                continue
            pose = cells[direction*4+int(frame["Name"][-1])]
            pose = pose.resize((max(1,round(pose.width*scale)),max(1,round(pose.height*scale))),Image.Resampling.NEAREST)
            pose = pose.crop(pose.getchannel("A").getbbox())
            x,y=frame["X"],sheet.height-frame["Y"]-frame["Height"]
            px,py=x+(28-pose.width)//2,y+26-pose.height
            sheet.paste((0,0,0,0),(x,y,x+28,y+28));sheet.paste(pose,(px,py))
            mask.paste((0,0,0,0),(x,y,x+28,y+28))
            pixels=mask.load()
            for iy in range(pose.height):
                for ix in range(pose.width):
                    r,g,b,a=pose.getpixel((ix,iy))
                    selected=(iy>=round(pose.height*.60) and g>r*1.25 and g>b*1.15) if asset.startswith("female") else (r>70 and r>g*1.6 and r>b*1.5)
                    if a and selected:
                        shade=min(255,round(g*190/115)) if asset.startswith("female") else r
                        pixels[px+ix,py+iy]=(shade,shade,shade,255)
            frame.update(PivotX=14,PivotY=2,ItemX=20,ItemY=5)
        for tail,content in (("",sheet),("_robe_mask",mask)):
            resource=atlas+tail
            path=save_image(content,resource)
            outputs[resource]["SHA256"]=digest(path)
            outputs[resource]["Use"]="原绘行走帧保持不变；独立生成四帧自然泳姿，同身份、同方向，透明水线"
    data.pop("Swimming",None)
    data.setdefault("SwimmingSets",{})[asset]={"Source":raw.relative_to(ROOT).as_posix(),"SHA256":digest(raw),
        "References":[str(r) for r in references],"Method":"individually-generated-natural-strokes",
        "Frames":12,"Directions":["side","front","back"],"WaterlinePixelY":26,"PivotY":2}
    write_manifest(data)


def career_titles(source):
    data=load_manifest()
    image=Image.open(source).convert("RGBA")
    raw=ROOT/".dev-tools/combat-art/0.7.0/career_titles.png"
    shutil.copyfile(source,raw)
    for index,asset in enumerate(("title_career_skills","title_career_archive")):
        cell=image.crop((0,index*image.height//2,image.width,(index+1)*image.height//2))
        box=cell.getchannel("A").point(lambda a:255 if a>=80 else 0).getbbox()
        assert box
        cell=cell.crop(box)
        path=save_image(cell,"ui/XuanhuangSkin/"+asset)
        resource="ui/XuanhuangSkin/"+asset
        data["Outputs"]=[o for o in data["Outputs"] if o["Resource"]!=resource]
        data["Outputs"].append({"Resource":resource,"SHA256":digest(path),"Size":list(cell.size),"Use":"职业技能/返回信息金玉艺术字，与修士列传配套"})
    data["CareerTitleSource"]={"Source":raw.relative_to(ROOT).as_posix(),"SHA256":digest(raw),"Reference":"ui/XuanhuangSkin/title_biography"}
    write_manifest(data)


def robe_masks():
    """Bake explicit, reviewable garment pixels rather than classifying during rendering."""
    data = load_manifest()
    for atlas in sorted({g["Atlas"] for g in data["Groups"] if g["Path"].startswith("actors/species/civs/mclsl_xuanhuang")}):
        image = Image.open(ROOT / "GameResources" / (atlas + ".png")).convert("RGBA")
        female = "female" in atlas
        mask = Image.new("RGBA", image.size)
        pixels = []
        for i, (r, g, b, a) in enumerate(image.getdata()):
            x, y = i % image.width % 28, i // image.width % 28
            swimming = i // image.width >= 28
            child = "_child" in atlas
            # Clothing ROIs exclude the green hair ribbon even when it shares the robe hue.
            garment = y >= (17 if child else 12)
            if swimming:
                garment = y >= (20 if child else 16)
                if not atlas.endswith(("_front", "_back")) and y <= (21 if child else 17) and 9 <= x <= 16:
                    garment = False
                if atlas.endswith("_back") and y <= (21 if child else 17) and x >= 17:
                    garment = False
            selected = a > 0 and (
                garment and g > r * 1.25 and g > b * 1.18 and r < 135 if female
                else r >= 70 and r > g * 2.5 and r > b * 2 and r-g > 60
            )
            shade = min(255, round(g * 190 / 115)) if female else r
            pixels.append((shade, shade, shade, 255) if selected else (0, 0, 0, 0))
        mask.putdata(pixels)
        path = save_image(mask, atlas + "_robe_mask")
        data["Outputs"] = [o for o in data["Outputs"] if o["Resource"] != atlas + "_robe_mask"]
        data["Outputs"].append({"Resource": atlas + "_robe_mask", "SHA256": digest(path),
                                "Size": list(mask.size), "Use": "明确衣服染色区域及亮度；不在渲染回调分类像素"})
    write_manifest(data)


def effect(source, asset, references):
    data = load_manifest()
    image = Image.open(source).convert("RGBA")
    if image.size != (1024, 1536) or image.getextrema()[3][0] != 0:
        raise ValueError("Expected transparent 4 x 6 effect atlas")
    raw = ROOT / ".dev-tools/combat-art/0.7.0" / (asset + ".png")
    raw.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(source, raw)
    resource = "effects/Careers/" + asset
    path = save_image(image.resize((768, 1152), Image.Resampling.LANCZOS), resource)
    phases = {"cast": list(range(8)), "travel/area": list(range(8, 16)),
              "hit": list(range(16, 20)), "cancel": list(range(20, 24))}
    data["Outputs"] = [o for o in data["Outputs"] if o["Resource"] != resource]
    data["Outputs"].append(dict(Resource=resource, SHA256=digest(path),
                                SourceSHA256=digest(source), Size=[768, 1152], Frames=phases))
    data["Sources"] = [s for s in data["Sources"] if s.get("Asset") != asset]
    data["Sources"].append(dict(File=raw.relative_to(ROOT).as_posix(), Asset=asset,
        SHA256=digest(raw), Status=STATUSES[0],
        References=[dict(File=p.name, SHA256=digest(p)) for p in references],
        Uses=[dict(Resource=resource, Frames=phases, Use="完整24帧主动技能动画，四列六行顺序")],
        Prompt="以本职业已有图标和动画为图像输入，保持核心符号、能量形态、像素颗粒、配色、"
               "光照与消散方式；仅补指定技能。8蓄力、8飞行或区域、4命中、4取消；"
               "真实透明背景，无文字，无底板，统一画布锚点。"))
    data["Missing"] = [m for m in data["Missing"] if m["Resource"] != resource]
    write_manifest(data)
    print(json.dumps(dict(effect=asset, frames=24)))


def icon(source, asset, targets, references):
    data = load_manifest()
    image = Image.open(source).convert("RGBA")
    if image.getextrema()[3][0] != 0:
        raise ValueError("Icon source needs real alpha")
    raw = ROOT / ".dev-tools/combat-art/0.7.0" / (asset + "_icon.png")
    shutil.copyfile(source, raw)
    for record in data["Sources"]:
        record["Uses"] = [use for use in record["Uses"] if use["Resource"] not in targets]
    for resource in targets:
        path = save_image(image.resize((64,64), Image.Resampling.LANCZOS), resource)
        data["Outputs"] = [o for o in data["Outputs"] if o["Resource"] != resource]
        data["Outputs"].append(dict(Resource=resource,SHA256=digest(path),SourceSHA256=digest(source),Size=[64,64],Frames="0"))
    data["Sources"] = [s for s in data["Sources"] if s.get("Asset") != asset+"_icon"]
    data["Sources"].append(dict(File=raw.relative_to(ROOT).as_posix(),Asset=asset+"_icon",SHA256=digest(raw),
        Status=STATUSES[0],References=[dict(File=p.name,SHA256=digest(p)) for p in references],
        Uses=[dict(Resource=t,Frames="0",Use="岩龟召唤/护主契纹64px图标") for t in targets],
        Prompt="以本职业已有图标为构图、像素颗粒、金玉材质与光效参考，以已接入岩龟图集为造型参考；"
               "厚重玉青龟甲、灰色龟头、金色契纹及青色灵焰，64像素可辨；真实透明，无文字或底板。"))
    write_manifest(data)


def row_bounds(image, count):
    alpha = image.getchannel("A").point(lambda a: 255 if a >= 48 else 0)
    occupied = [alpha.crop((0, y, image.width, y + 1)).getbbox() is not None
                for y in range(image.height)]
    bands = []
    start = None
    for y, active in enumerate(occupied + [False]):
        if active and start is None:
            start = y
        if not active and start is not None:
            if bands and start - bands[-1][1] < 12:
                bands[-1][1] = y
            else:
                bands.append([start, y])
            start = None
    bands = [b for b in bands if b[1] - b[0] > 20]
    if len(bands) != count:
        raise ValueError(f"Expected {count} isolated sprite rows; found {bands}")
    cuts = [0] + [(a[1] + b[0]) // 2 for a, b in zip(bands, bands[1:])] + [image.height]
    return list(zip(cuts, cuts[1:]))


def character(source, sex, role, references):
    """Cut reviewed poses, keeping one scale for all eight frames in each direction."""
    data = load_manifest()
    image = Image.open(source).convert("RGBA")
    if image.getextrema()[3][0] != 0:
        raise ValueError("Character source has no transparent background")
    rows = row_bounds(image, 6)
    asset = sex + "_1_" + role
    raw = ROOT / ".dev-tools/combat-art/0.7.0" / (asset + ".png")
    shutil.copyfile(source, raw)
    uses = []
    for direction, suffix in enumerate(("", "_front", "_back")):
        crops, boxes = [], []
        for row in range(direction * 2, direction * 2 + 2):
            for col in range(4):
                left, right = round(col * image.width / 4), round((col + 1) * image.width / 4)
                top, bottom = rows[row]
                cell = image.crop((left, top, right, bottom))
                box = cell.getchannel("A").point(lambda a: 255 if a >= 48 else 0).getbbox()
                if box is None:
                    raise ValueError("Empty character pose")
                # Include the authored edge alpha while excluding empty grid gutters.
                box = (max(0, box[0]-2), max(0, box[1]-2),
                       min(cell.width, box[2]+2), min(cell.height, box[3]+2))
                crops.append(cell.crop(box))
                boxes.append([left+box[0], top+box[1], left+box[2], top+box[3]])
        target_height = 17 if role == "child" else 23
        scale = min(target_height / max(c.height for c in crops[:4]),
                    26 / max(c.width for c in crops))
        atlas = Image.new("RGBA", (112, 56))
        frames = []
        for i, crop in enumerate(crops):
            pose = crop.resize((max(1, round(crop.width * scale)), max(1, round(crop.height * scale))),
                               Image.Resampling.NEAREST)
            x, y = i % 4 * 28, i // 4 * 28
            atlas.paste(pose, (x+(28-pose.width)//2, y+26-pose.height))
            name = ("walk_" if i < 4 else "swim_") + str(i % 4)
            frames.append(dict(Name=name, X=x, Y=28-y, Width=28, Height=28,
                               PivotX=14, PivotY=2 if i < 4 else 7, PixelsPerUnit=1.8,
                               ItemX=21 if direction == 0 else 20 if direction == 1 else 8,
                               ItemY=13 if i < 4 else 10))
        resource = "actors/Xuanhuang/" + asset + suffix
        path = save_image(atlas, resource)
        group = "actors/species/civs/mclsl_xuanhuang" + suffix + "/" + asset
        data["Groups"] = [g for g in data["Groups"] if g["Path"] != group]
        data["Groups"].append(dict(Path=group, Atlas=resource, Frames=frames))
        data["Outputs"] = [o for o in data["Outputs"] if o["Resource"] != resource]
        data["Outputs"].append(dict(Resource=resource, SHA256=digest(path),
            SourceSHA256=digest(source), Size=list(atlas.size), SourceRectangles=boxes,
            Scale=scale, Frames=[f["Name"] for f in frames]))
        uses.append(dict(Resource=group, Frames=[f["Name"] for f in frames],
                         Use=f"{sex}/{role}/{suffix or 'side'}；脚底2，游泳水线7，原生左右翻转"))
    data["Sources"] = [s for s in data["Sources"] if s.get("Asset") != asset]
    data["Sources"].append(dict(File=raw.relative_to(ROOT).as_posix(), Asset=asset,
        SHA256=digest(raw), Status=STATUSES[0], Uses=uses,
        References=[dict(File=p.name, SHA256=digest(p)) for p in references],
        Prompt="以本性别已有侧面、正面、背面及行走游泳帧为图像输入。只补"+role+
               "身份必要差异；保持比例、黑发束冠、飘带、男女红/绿金边长袍、白内衬、简化五官与粗像素颗粒。"
               "四列六行：侧行走/侧游泳/正行走/正游泳/背行走/背游泳，各四帧；统一中心脚底；"
               "真实透明背景，无文字、底板、武器或跨格。"))
    write_manifest(data)
    print(json.dumps(dict(character=asset, frames=24)))


def normalize_basic(directory):
    data = load_manifest()
    paths = sorted(directory.glob("*.png"))
    for skin, indices in CHARACTERS.items():
        for direction, (suffix, index) in enumerate(zip(("", "_front", "_back"), indices)):
            source = paths[index]
            image = Image.open(source).convert("RGBA")
            rows = row_bounds(image, 2)
            crops, boxes = [], []
            for row, (top, bottom) in enumerate(rows):
                for col in range(4):
                    left, right = round(col*image.width/4), round((col+1)*image.width/4)
                    cell = image.crop((left, top, right, bottom))
                    box = cell.getchannel("A").point(lambda a: 255 if a >= 48 else 0).getbbox()
                    crops.append(cell.crop(box))
                    boxes.append([left+box[0], top+box[1], left+box[2], top+box[3]])
            scale = min(23/max(c.height for c in crops[:4]), 26/max(c.width for c in crops))
            atlas = Image.new("RGBA", (112, 56))
            resource = "actors/Xuanhuang/"+skin+suffix
            group = next(g for g in data["Groups"] if g["Atlas"] == resource and g["Path"].endswith("/"+skin))
            for i, (crop, frame) in enumerate(zip(crops, group["Frames"])):
                pose = crop.resize((max(1, round(crop.width*scale)), max(1, round(crop.height*scale))),
                                   Image.Resampling.NEAREST)
                atlas.paste(pose, (i%4*28+(28-pose.width)//2, i//4*28+26-pose.height))
                frame.update(PivotY=2 if i<4 else 7, PixelsPerUnit=1.8,
                             ItemX=21 if direction==0 else 20 if direction==1 else 8,
                             ItemY=13 if i<4 else 10)
            path = save_image(atlas, resource)
            output = next(o for o in data["Outputs"] if o["Resource"] == resource)
            output.update(SHA256=digest(path), SourceRectangles=boxes, Scale=scale)
    # Native skin keys remain unchanged; direction selection uses the sex-specific full-body skin.
    for suffix in ("", "_front", "_back"):
        for alias, role in (("warrior_1","warrior"), ("king","king"), ("leader","leader"),
                            ("child","child"), ("zombie","zombie")):
            primary = next(g for g in data["Groups"]
                           if g["Path"] == "actors/species/civs/mclsl_xuanhuang"+suffix+"/male_1_"+role)
            target = "actors/species/civs/mclsl_xuanhuang"+suffix+"/"+alias
            data["Groups"] = [g for g in data["Groups"] if g["Path"] != target]
            data["Groups"].append(dict(Path=target, Atlas=primary["Atlas"], Frames=primary["Frames"]))
    write_manifest(data)


def ship_parts(image, columns, rows):
    import numpy as np
    from scipy import ndimage
    labels, _ = ndimage.label(np.array(image.getchannel("A")) >= 48)
    sizes = np.bincount(labels.ravel())
    parts = []
    for i, box in enumerate(ndimage.find_objects(labels), 1):
        if box is None or sizes[i] <= 2000:
            continue
        top, bottom, left, right = box[0].start, box[0].stop, box[1].start, box[1].stop
        parts.append((i, (left, top, right, bottom)))
    if len(parts) != columns*rows:
        raise ValueError(f"Expected {columns*rows} separate ship poses, found {len(parts)}")
    parts.sort(key=lambda p: (p[1][1]+p[1][3])/2)
    ordered = []
    for row in range(rows):
        ordered.extend(sorted(parts[row*columns:(row+1)*columns], key=lambda p: p[1][0]))
    result = []
    for label, box in ordered:
        left, top, right, bottom = box
        box = (max(0,left-2), max(0,top-2), min(image.width,right+2), min(image.height,bottom+2))
        crop = image.crop(box)
        region = labels[box[1]:box[3],box[0]:box[2]] == label
        region = ndimage.binary_dilation(region, iterations=2)
        alpha = np.array(crop.getchannel("A"))
        alpha[~region] = 0
        crop.putalpha(Image.fromarray(alpha))
        result.append((crop, list(box)))
    return result


def ship(source, asset, references, repair=None):
    data = load_manifest()
    image = Image.open(source).convert("RGBA")
    if image.getextrema()[3][0] != 0:
        raise ValueError("Ship source needs real alpha")
    trading = asset == "boat_trading_human"
    parts = ship_parts(image, 8 if trading else 3, 3 if trading else 6)
    raw = ROOT / ".dev-tools/combat-art/0.7.0" / (asset+".png")
    shutil.copyfile(source, raw)
    repair_record = None
    if repair:
        if asset != "boat_fishing":
            raise ValueError("The reviewed repair applies only to fishing pose 16")
        repair_parts = ship_parts(Image.open(repair).convert("RGBA"), 3, 6)
        parts[16] = repair_parts[16]
        repair_raw = raw.with_name(asset+"_pose16-repair.png")
        shutil.copyfile(repair, repair_raw)
        repair_record = dict(File=repair_raw.relative_to(ROOT).as_posix(), SHA256=digest(repair),
                             Pose=16, SourceRectangle=parts[16][1], Use="仅修复第二划动帧的完整棚顶")
    # Reviewed bow directions; normal and wreck are states, never animation frames.
    directions = {180:0, -45:1, -90:2, -135:3, 0:4, 45:5, 90:6, 135:7}
    mapping = {}
    for angle, pose in directions.items():
        mapping[f"{angle}@0"] = pose
        mapping[f"{angle}@1"] = pose+(8 if trading else 9)
    mapping["normal"] = 0 if trading else 8
    mapping["broken"] = 16 if trading else 17
    native = next(r for r in data["NativeArchitecture"]["resources"] if r["path"] == "actors/boats/"+asset)
    width, height = round(native["frames"][0]["width"]*4), round(native["frames"][0]["height"]*4)
    scale = min((width-4)/max(parts[p][0].width for p in mapping.values()),
                (height-4)/max(parts[p][0].height for p in mapping.values()))
    atlas = Image.new("RGBA", (width*6,height*3))
    frames, rectangles = [], {}
    for i, template in enumerate(native["frames"]):
        name = template["name"]
        crop, box = parts[mapping[name]]
        pose = crop.resize((max(1,round(crop.width*scale)),max(1,round(crop.height*scale))),Image.Resampling.LANCZOS)
        x,y = i%6*width,i//6*height
        atlas.paste(pose,(x+(width-pose.width)//2,y+height-2-pose.height))
        frames.append(dict(Name=name,X=x,Y=atlas.height-y-height,Width=width,Height=height,
                           PivotX=width/2,PivotY=height*0.2,PixelsPerUnit=4))
        rectangles[name]=dict(Pose=mapping[name],Rectangle=box,Repair=mapping[name]==16 and repair is not None)
    resource = "actors/Boats/Xuanhuang/"+asset
    path = save_image(atlas,resource)
    group = "actors/boats/mclsl_xuanhuang_"+asset
    data["Groups"] = [g for g in data["Groups"] if g["Path"]!=group]
    data["Groups"].append(dict(Path=group,Atlas=resource,Frames=frames))
    data["Outputs"] = [o for o in data["Outputs"] if o["Resource"]!=resource]
    data["Outputs"].append(dict(Resource=resource,SHA256=digest(path),SourceSHA256=digest(source),
                                Size=list(atlas.size),Frames=rectangles,Scale=scale))
    data["Sources"] = [s for s in data["Sources"] if s.get("Asset")!=asset]
    data["Sources"].append(dict(File=raw.relative_to(ROOT).as_posix(),Asset=asset,SHA256=digest(raw),
        Status=STATUSES[0],References=[dict(File=p.name,SHA256=digest(p)) for p in references],
        Repair=repair_record,Uses=[dict(Resource=group,Frames=rectangles,Use="八向双帧及独立normal/broken状态")],
        Prompt="以既有码头为图像输入，保持青绿瓦顶、木材与金饰、观察角度、像素颗粒、材质光照，"
               "仅改变为指定船只功能与方向。八向双划动帧、静止及破损，真实透明背景，无文字或水面。"
               "渔船修复仅取末行中格的完整棚顶划动帧，其余保留原候选。"))
    write_manifest(data)
    print(json.dumps(dict(ship=asset,frames=len(frames),size=list(atlas.size))))


def spirit(source, kind, references):
    """Bind the eight authored idle/movement poses to the native animal animator."""
    data = load_manifest()
    image = Image.open(source).convert("RGBA")
    if image.getextrema()[3][0] != 0:
        raise ValueError("Spirit source needs real alpha")
    asset = "spirit_" + str(kind)
    raw = ROOT / ".dev-tools/combat-art/0.7.0" / (asset + ".png")
    shutil.copyfile(source, raw)
    crops, boxes = [], []
    for row in range(2):
        for col in range(4):
            box = (round(col*image.width/4), round(row*image.height/2),
                   round((col+1)*image.width/4), round((row+1)*image.height/2))
            cell = image.crop(box)
            bounds = cell.getchannel("A").point(lambda a: 255 if a >= 48 else 0).getbbox()
            if bounds is None:
                raise ValueError("Empty spirit pose")
            bounds = (max(0,bounds[0]-2),max(0,bounds[1]-2),
                      min(cell.width,bounds[2]+2),min(cell.height,bounds[3]+2))
            crops.append(cell.crop(bounds))
            boxes.append([box[0]+bounds[0],box[1]+bounds[1],box[0]+bounds[2],box[1]+bounds[3]])
    scale = min(56/max(c.width for c in crops),56/max(c.height for c in crops))
    atlas = Image.new("RGBA", (256,128))
    frames = []
    for i,crop in enumerate(crops):
        pose = crop.resize((max(1,round(crop.width*scale)),max(1,round(crop.height*scale))),Image.Resampling.NEAREST)
        x,y = i%4*64,i//4*64
        atlas.paste(pose,(x+(64-pose.width)//2,y+60-pose.height))
        frames.append(dict(Name=("idle_" if i<4 else "walk_")+str(i%4),
                           X=x,Y=64-y,Width=64,Height=64,PivotX=32,PivotY=4,PixelsPerUnit=4))
    resource = "actors/CareerSpirits/"+str(kind)
    path = save_image(atlas,resource)
    group = resource+"/main"
    data["Groups"] = [g for g in data["Groups"] if g["Path"]!=group]
    data["Groups"].append(dict(Path=group,Atlas=resource,Frames=frames))
    data["Outputs"] = [o for o in data["Outputs"] if o["Resource"]!=resource]
    data["Outputs"].append(dict(Resource=resource,SHA256=digest(path),SourceSHA256=digest(source),
                                Size=list(atlas.size),SourceRectangles=boxes,Scale=scale))
    data["Sources"] = [s for s in data["Sources"] if s.get("Asset")!=asset]
    data["Sources"].append(dict(File=raw.relative_to(ROOT).as_posix(),Asset=asset,SHA256=digest(raw),
        Status=STATUSES[0],References=[dict(File=p.name,SHA256=digest(p)) for p in references],
        Uses=[dict(Resource=group,Frames=[f["Name"] for f in frames],Use="独立契灵原生静止/行走动画")],
        Prompt="以本职业对应灵兽图标为图像输入，保持主体、玉青配色、金色契纹和像素颗粒。"
               "四列两行：四帧静止、四帧行走或飞行，完整身体朝右，统一比例脚底，真实透明背景，无文字底板。"))
    write_manifest(data)
    print(json.dumps(dict(spirit=kind,frames=8)))


def building(source, asset, references):
    data = load_manifest()
    if "NativeArchitecture" not in data:
        data["NativeArchitecture"] = json.loads(
            (ROOT / "build/native-export/native-human-resources.json").read_text(encoding="utf-8-sig"))
    baseline = data["NativeArchitecture"]
    definition = next(b for b in baseline["buildings"] if b["id"] == asset)
    native = next(r for r in baseline["resources"] if r["path"] == definition["path"])
    image = Image.open(source).convert("RGBA")
    if image.getextrema()[3][0] != 0:
        raise ValueError("Building source must retain genuine transparency")
    raw = ROOT / ".dev-tools/combat-art/0.7.0" / (asset + ".png")
    raw.parent.mkdir(parents=True, exist_ok=True)
    if source.resolve() != raw.resolve():
        shutil.copyfile(source, raw)
    animated = asset.startswith("windmill") or asset == "docks_human"
    rows = 4 if animated else 2
    cells = [image.crop((round(c * image.width / 2), round(r * image.height / rows),
                         round((c + 1) * image.width / 2), round((r + 1) * image.height / rows)))
             for r in range(rows) for c in range(2)]
    crops = [cell.crop(cell.getbbox()) if cell.getbbox() else None for cell in cells]
    world_width = max(f["width"] for f in native["frames"] if not f["name"].startswith("mini"))
    world_height = max(f["height"] for f in native["frames"] if not f["name"].startswith("mini"))
    width, height = round(world_width * 4), round(world_height * 4)
    normal_cells = [crops[i] for i in ([0, 2, 3, 4, 5, 6] if asset.startswith("windmill")
                                     else [0, 1, 2, 3, 4, 5, 7] if animated else [0, 1, 3])]
    scale = min((width - 4) / max(c.width for c in normal_cells),
                (height - 4) / max(c.height for c in normal_cells))
    target = "buildings/Xuanhuang/" + asset
    frames = []
    atlas = Image.new("RGBA", (width * 4, height * ((len(native["frames"]) + 3) // 4)))
    for i, template in enumerate(native["frames"]):
        name = template["name"]
        mini = name.startswith("mini")
        if animated:
            state = (0 if name.startswith("construction") else
                     (1 if asset.startswith("windmill") else 6) if mini else
                     (6 if asset.startswith("windmill") else 7) if name.startswith("ruin") else
                     1 if name.startswith("disabled") else 2 + int(name.rsplit("_", 1)[1]))
        else:
            state = 0 if name.startswith("construction") else 2 if mini else 3 if name.startswith("ruin") else 1
        crop = crops[state]
        if mini:
            size = (max(1, round(template["width"] * 4)), max(1, round(template["height"] * 4)))
            local_scale = min(size[0] / crop.width, size[1] / crop.height)
        else:
            local_scale = scale
        resized = crop.resize((max(1, round(crop.width * local_scale)), max(1, round(crop.height * local_scale))),
                              Image.Resampling.LANCZOS)
        x, y = i % 4 * width, i // 4 * height
        atlas.paste(resized, (x + (width - resized.width) // 2, y + height - resized.height - 2))
        frames.append(dict(Name=name, X=x+(width-resized.width)//2 if mini else x, Y=atlas.height-y-height+2 if mini else atlas.height-y-height,
                           Width=resized.width if mini else width, Height=resized.height if mini else height,
                           PivotX=resized.width/2 if mini else width/2, PivotY=0 if mini else 2, PixelsPerUnit=4))
    path = save_image(atlas, target)
    resource_path = "buildings/civ_main/mclsl_xuanhuang/" + asset
    data["Groups"] = [g for g in data["Groups"] if g["Path"] != resource_path]
    data["Groups"].append(dict(Path=resource_path, Atlas=target, Frames=frames))
    data["Outputs"] = [o for o in data["Outputs"] if o["Resource"] != target]
    data["Outputs"].append(dict(Resource=target, SHA256=digest(path), SourceSHA256=digest(source),
                                 Size=list(atlas.size), Frames=[f["Name"] for f in frames]))
    data["Sources"] = [s for s in data["Sources"] if s.get("Asset") != asset]
    data["Sources"].append(dict(File=raw.relative_to(ROOT).as_posix(), Asset=asset,
                                SHA256=digest(raw), Status=STATUSES[0],
                                References=[dict(File=p.name, SHA256=digest(p)) for p in references],
                                Uses=[dict(Resource=resource_path, Frames=[f["Name"] for f in frames],
                                           Use="施工、完成、小地图、废墟；按原版命名调用")],
                                Prompt="以既有码头原图为图像输入，保持青绿瓦顶、木石结构、金饰、观察角度、像素颗粒、光照和细节密度；"
                                       "只改变为指定建筑功能及状态。真实透明背景，无文字、无展示底板；统一占地与锚点。",
                                RequestedAsset=asset))
    if asset == "fishing_docks_human":
        data["Sources"][69]["Status"] = STATUSES[0]
        data["Sources"][69]["Note"] = "四格用途：施工、完成、小地图、废墟。左下不是顺序动画帧。"
        data["Sources"][69]["Uses"] = [dict(Resource=resource_path, Frames=[f["Name"] for f in frames], Use="独立渔业码头")]
    write_manifest(data)
    print(json.dumps(dict(building=asset, frames=len(frames), size=list(atlas.size))))


def import_originals(directory):
    paths = sorted(directory.glob("*.png"))
    if len(paths) != 110:
        raise ValueError("Expected exactly the reviewed 110 original PNGs")
    data = load_manifest()
    data["Sources"] = []
    data["Outputs"] = []
    by_index = {}
    for i, path in enumerate(paths):
        image = Image.open(path)
        record = {
            "Index": i, "File": path.name, "SHA256": digest(path), "Size": list(image.size),
            "Status": STATUSES[1], "Uses": [], "Note": "",
        }
        data["Sources"].append(record)
        by_index[i] = record

    def output(index, target, image, frames, use):
        path = save_image(image, target)
        source = by_index[index]
        source["Status"] = STATUSES[0]
        source["Uses"].append({"Resource": target, "Frames": frames, "Use": use})
        data["Outputs"].append({
            "Resource": target, "SourceSHA256": source["SHA256"],
            "SHA256": digest(path), "Size": list(image.size), "Frames": frames,
        })

    for career, (actives, passives, icon) in ICONS.items():
        assignments = [(icon, "trait/MclslCombatCareer_" + career, "职业图标")]
        for tier, (active, passive) in enumerate(zip(actives, passives), 1):
            assignments.extend([
                (active, f"ui/CareerSkills/CP_{career}_{tier}A", "主动图标"),
                (passive, f"trait/MclslCombatPassive_{career}_{tier}", "被动图标"),
            ])
        for index, target, use in assignments:
            image = Image.open(paths[index]).convert("RGBA")
            image = image.resize((64, 64), Image.Resampling.LANCZOS)
            output(index, target, image, "0", use)

    for career, indices in EFFECTS.items():
        for tier, index in enumerate(indices, 1):
            if index is None:
                continue
            image = Image.open(paths[index]).convert("RGBA")
            if image.size != (1024, 1536):
                raise ValueError(f"Unexpected effect grid: {paths[index].name}")
            image = image.resize((768, 1152), Image.Resampling.LANCZOS)
            output(index, f"effects/Careers/CP_{career}_{tier}A", image, "0-7/8-15/16-19/20-23",
                   "蓄力8、飞行或区域8、命中4、取消4；四列六行")

    for skin, indices in CHARACTERS.items():
        for suffix, index in zip(("", "_front", "_back"), indices):
            image = Image.open(paths[index]).convert("RGBA")
            output(index, "actors/Xuanhuang/" + skin + suffix,
                   image.resize((112, 56), Image.Resampling.NEAREST),
                   "walk_0-3/swim_0-3", "基本人物与方向，保留已有切帧规格")
    for record in data["Sources"]:
        i = record["Index"]
        if i == 69:
            record["Note"] = "码头四幅已审看：左上施工、右上完成、左下停用、右下废墟；不是四帧动画。"
        elif i in (1, 38):
            record["Status"] = STATUSES[3]
            record["Note"] = "同类剑修候选已参与比较；对应技能采用现有更清楚的完整图集，不加入运行包。"
        elif record["Size"] == [1774, 887] and not record["Uses"]:
            record["Status"] = STATUSES[3]
            record["Note"] = "已比较人物身份；服色或细节密度不同，不作为新的国家服色或身份替身。"
    data["Missing"] = [
        {"Resource": f"effects/Careers/CP_{career}_{tier}A", "Status": STATUSES[2],
         "Reason": "全部29张动画图集已比较；缺少与本技能主体对应的完整图集。"}
        for career, indices in EFFECTS.items() for tier, index in enumerate(indices, 1) if index is None
    ]
    write_manifest(data)
    print(json.dumps({"sources": len(paths), "outputs": len(data["Outputs"]), "missingEffects": len(data["Missing"])}))


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--originals", type=Path)
    parser.add_argument("--robe-masks", action="store_true")
    parser.add_argument("--swimming")
    parser.add_argument("--career-titles", type=Path)
    parser.add_argument("--building")
    parser.add_argument("--effect")
    parser.add_argument("--icon")
    parser.add_argument("--target", action="append", default=[])
    parser.add_argument("--character", choices=("warrior", "king", "leader", "old", "child", "zombie"))
    parser.add_argument("--sex", choices=("male", "female"))
    parser.add_argument("--normalize-basic", type=Path)
    parser.add_argument("--ship", choices=("boat_fishing", "boat_trading_human", "boat_transport_human"))
    parser.add_argument("--spirit", type=int, choices=range(4))
    parser.add_argument("--repair", type=Path)
    parser.add_argument("--source", type=Path)
    parser.add_argument("--reference", type=Path, action="append", default=[])
    args = parser.parse_args()
    if args.originals:
        import_originals(args.originals)
    if args.robe_masks:
        robe_masks()
    if args.swimming:
        swimming(args.source,args.swimming,args.reference)
    if args.career_titles:
        career_titles(args.career_titles)
    if args.building:
        building(args.source, args.building, args.reference)
    if args.effect:
        effect(args.source, args.effect, args.reference)
    if args.icon:
        icon(args.source, args.icon, args.target, args.reference)
    if args.character:
        character(args.source, args.sex, args.character, args.reference)
    if args.normalize_basic:
        normalize_basic(args.normalize_basic)
    if args.ship:
        ship(args.source, args.ship, args.reference, args.repair)
    if args.spirit is not None:
        spirit(args.source, args.spirit, args.reference)
