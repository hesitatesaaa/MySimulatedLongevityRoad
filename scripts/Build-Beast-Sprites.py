"""Build compact WorldBox sprites from transparent action pose sheets."""
import json
import hashlib
import sys
from pathlib import Path
from PIL import Image, ImageDraw
import numpy as np
from scipy import ndimage

ROOT = Path(__file__).resolve().parents[1]
ACTORS = ROOT / "GameResources" / "actors"
SOURCE = ROOT / "scripts" / "beast_sprite_sources"
CANVAS = 130
COUNTS = {"idle": 4, "attack": 6, "walk": 4, "hit": 4, "death": 4}

def clean(image):
    image = image.convert("RGBA")
    alpha = image.getchannel("A").point(lambda a: 255 if a >= 112 else 0)
    # Atlas rows occasionally overlap. Keep the actual body instead of a
    # disconnected feather or wave spilling over from the preceding cell.
    labels, count = ndimage.label(np.asarray(alpha) > 0)
    if count > 1:
        sizes = np.bincount(labels.ravel())
        sizes[0] = 0
        keep = int(sizes.argmax())
        alpha = Image.fromarray(np.where(labels == keep, 255, 0).astype("uint8"), "L")
    image.putalpha(alpha)
    box = alpha.getbbox()
    if not box:
        raise ValueError("transparent pose")
    return image.crop(box)

def pixel_pose(image, human=False):
    pose = clean(image)
    max_w, max_h = (31, 48) if human else (48, 43)
    factor = min(max_w / pose.width, max_h / pose.height)
    size = (max(1, round(pose.width * factor)), max(1, round(pose.height * factor)))
    pose = pose.resize(size, Image.Resampling.NEAREST)
    alpha = pose.getchannel("A")
    rgb = pose.convert("RGB").quantize(colors=22, method=Image.Quantize.FASTOCTREE,
                                       dither=Image.Dither.NONE).convert("RGB")
    pose = rgb.convert("RGBA")
    pose.putalpha(alpha)
    result = Image.new("RGBA", (CANVAS, CANVAS))
    result.alpha_composite(pose, ((CANVAS - pose.width) // 2, 91 - pose.height))
    return result

def atlas_column(image, col, rows, human_rows):
    """Find complete pose components instead of chopping at assumed row lines."""
    cw = image.width // 5
    left,right = col*cw,(col+1)*cw
    alpha = np.asarray(image.crop((left,0,right,image.height)).getchannel("A")) >= 112
    connected = ndimage.binary_closing(alpha,structure=np.ones((5,5)))
    labels,count = ndimage.label(connected)
    components = []
    for label in range(1,count+1):
        ys,xs = np.where(labels == label)
        if len(xs) < 500:
            continue
        components.append((len(xs),(int(xs.min()),int(ys.min()),
                                    int(xs.max()+1),int(ys.max()+1))))
    if not components:
        raise ValueError(f"empty atlas column {col}")
    top_area = max(item[0] for item in components)
    components = [c for c in components if c[0] >= max(1200,top_area*.19)]
    components = sorted(sorted(components,reverse=True)[:rows],
                        key=lambda item:item[1][1])
    expected = ([.24,.75] if rows == 2 else [.22,.45,.69,.90])
    slots = [None]*rows
    for area,box in components:
        center = (box[1]+box[3])/(2*image.height)
        candidates = [r for r in range(rows) if slots[r] is None]
        row = min(candidates,key=lambda r:abs(center-expected[r]))
        x0,y0,x1,y1 = box
        pad = 3
        crop = image.crop((max(left,left+x0-pad),max(0,y0-pad),
                           min(right,left+x1+pad),min(image.height,y1+pad)))
        slots[row] = pixel_pose(crop,row in human_rows)
    for row in range(rows):
        if slots[row] is None:
            nearest = min((i for i in range(rows) if slots[i] is not None),
                          key=lambda i:abs(i-row))
            slots[row] = articulate(slots[nearest],"idle",2)
    return slots

def atlas_poses(name, human, category=None):
    folder = SOURCE / "action_atlases"
    if category:
        folder = folder / category
    path = folder / (name + ".png")
    if not path.exists():
        return None
    image = Image.open(path).convert("RGBA")
    rows = (2, 3) if human else (0, 1)
    result = {}
    for col, action in enumerate(COUNTS):
        poses = atlas_column(image,col,4,{2,3})
        result[action] = [poses[row] for row in rows]
        if name == "dafeng" and not human:
            result[action][1] = articulate(result[action][0],
                                           action if action != "attack" else "hit",2)
    return result

def character_atlas_poses(category, name):
    path = SOURCE / "action_atlases" / category / (name + ".png")
    if not path.exists():
        return None
    image = Image.open(path).convert("RGBA")
    return {action:atlas_column(image,col,2,{0,1})
            for col,action in enumerate(COUNTS)}

def shift_part(frame, box, dx, dy):
    out = frame.copy()
    part = frame.crop(box)
    out.paste(Image.new("RGBA", part.size), box[:2])
    out.alpha_composite(part, (box[0]+dx, box[1]+dy))
    return out

def articulate(frame, action, phase):
    box = frame.getchannel("A").getbbox()
    if not box:
        return frame.copy()
    x0, y0, x1, y1 = box
    mid = (x0+x1)//2
    if action == "idle":
        edge = (x0, y0+(y1-y0)//3, mid, y0+2*(y1-y0)//3)
        return shift_part(frame, edge, (-1, 0, 1, 0)[phase], (-1, 0, 0, 1)[phase])
    if action == "walk":
        split = y0+(y1-y0)*2//3
        half = (x0, split, mid, y1) if phase%2 == 0 else (mid, split, x1, y1)
        out = shift_part(frame, half, (-2, 1, 2, -1)[phase], (-1, 0, 0, -1)[phase])
        return shift_part(out, (x0,y0,x1,split), (0,1,0,-1)[phase], 0)
    if action == "hit":
        upper = (x0,y0,x1,y0+(y1-y0)*2//3)
        return shift_part(frame, upper, (-2,-1,1,0)[phase], (0,1,1,0)[phase])
    if action == "death":
        split = y0+(y1-y0)//2
        out = shift_part(frame, (x0,y0,x1,split), phase*2, phase*2)
        if phase >= 2:
            out = shift_part(out, (x0,split,x1,y1), phase-1, phase-1)
        return out
    return frame.copy()

def attack_effect(frame, phase):
    result = frame.copy()
    box = result.getchannel("A").getbbox()
    if not box:
        return result
    x0,y0,x1,y1 = box
    draw = ImageDraw.Draw(result)
    if phase in (1,2,3):
        x,y = min(CANVAS-4,x1+phase), max(3,y0+(y1-y0)//2-phase)
        draw.line([(x,y),(x+2,y-2)], fill=(242,227,140,255), width=1)
    elif phase in (4,5):
        draw.point((max(1,x0-3), y0+(y1-y0)//2+phase-4), fill=(242,227,140,255))
    return result

def frames_for(poses):
    result = {}
    seen = set()
    for action, count in COUNTS.items():
        a,b = poses[action]
        result[action] = []
        for index in range(count):
            if action == "attack":
                source = (a,a,b,b,b,a)[index]
                frame = attack_effect(articulate(source, "hit", index%4), index)
            elif action == "hit":
                source = (a,b,b,a)[index]
                frame = articulate(source,action,index)
            elif action == "death":
                source = (poses["hit"][1],a,b,b)[index]
                frame = articulate(source,"hit" if index == 0 else "death",
                                   1 if index == 0 else min(index,2))
                if index == 3:
                    frame = frame.copy()
                    box = frame.getchannel("A").getbbox()
                    if box:
                        ImageDraw.Draw(frame).point((box[0]-2,box[3]-2),
                                                    fill=(156,153,142,255))
            else:
                frame = a if index%2 == 0 else b
                frame = articulate(frame, action, index%4)
            digest = hashlib.sha256(frame.tobytes()).digest()
            if digest in seen:
                box = frame.getchannel("A").getbbox()
                if box:
                    x0,y0,x1,y1 = box
                    frame = shift_part(frame, (x0,y0,x1,y0+max(2,(y1-y0)//3)),
                                       2+index%2, 1)
                    digest = hashlib.sha256(frame.tobytes()).digest()
            if digest in seen:
                frame = frame.copy()
                box = frame.getchannel("A").getbbox()
                if box:
                    ImageDraw.Draw(frame).point((box[0]+index+1,box[1]+1),
                                                fill=(242,227,140,255))
                    digest = hashlib.sha256(frame.tobytes()).digest()
            seen.add(digest)
            result[action].append(frame)
    return result

def mark_named_human(poses, name):
    color = {"DiYi":(243,201,73,255), "DiSanMo":(172,135,185,255),
             "QingShenLong":(59,214,243,255)}[name]
    for pair in poses.values():
        for index,frame in enumerate(pair):
            box = frame.getchannel("A").getbbox()
            if not box:
                continue
            x0,y0,x1,_ = box
            center = (x0+x1)//2
            draw = ImageDraw.Draw(frame)
            draw.point((center,y0+3), fill=color)
            if name == "DiYi":
                draw.point((center,y0+2), fill=(255,240,174,255))
            elif name == "DiSanMo":
                draw.line([(center-4,y0+2),(center-5,y0+4)], fill=color)
            else:
                draw.line([(center-4,y0+1),(center-4,y0-2)], fill=color)
                draw.line([(center+4,y0+1),(center+4,y0-2)], fill=color)

def write_form(folder, poses):
    main = folder/"main"
    main.mkdir(parents=True,exist_ok=True)
    for action,frames in frames_for(poses).items():
        for index,frame in enumerate(frames):
            frame.save(main/f"{action}_{index}.png", optimize=True)

def main(manifest_path):
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    for name in manifest["beasts"]:
        for human in (False,True):
            poses = atlas_poses(name,human)
            if poses is None:
                raise FileNotFoundError(f"missing five-action atlas: {name}")
            write_form(ACTORS/"Beasts"/name/("Human" if human else "Beast"),poses)
    for name in manifest["named"]:
        if name in ("DiYi","DiSanMo","QingShenLong"):
            beast_poses = atlas_poses(name,False,"NamedBeasts")
            human_poses = atlas_poses(name,True,"NamedBeasts")
            if beast_poses is None or human_poses is None:
                raise FileNotFoundError(f"missing named beast five-action atlas: {name}")
            write_form(ACTORS/"Named"/name/"Beast",beast_poses)
            mark_named_human(human_poses,name)
            write_form(ACTORS/"Named"/name/"Human",human_poses)
        elif name != "BaiXianSheng":
            poses = character_atlas_poses("Named",name)
            if poses is None:
                raise FileNotFoundError(f"missing named character five-action atlas: {name}")
            write_form(ACTORS/"Named"/name,poses)
    for name in ("Bai","Chuanfa"):
        folder = ACTORS/"Immortals"/name
        poses = character_atlas_poses("Immortals",name)
        if poses is None:
            raise FileNotFoundError(f"missing immortal five-action atlas: {name}")
        write_form(folder,poses)

if __name__ == "__main__":
    raise SystemExit(
        "Deprecated atlas generator: it crops pose sheets and would overwrite "
        "individual animation frames. Keep it only as a record of the legacy art."
    )
