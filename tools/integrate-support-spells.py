"""Integrate only the ten new support spells; preserve existing staged assets."""
import hashlib
import json
import shutil
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
WORK = ROOT / "build/support-spell-upgrade"
IDS = ("S002", "S003", "S005", "S006", "S011", "S014", "S018", "S020", "S024", "S025")
SIZE = 192
HIT_START = {"S002":4,"S003":4,"S005":4,"S006":4,"S020":4,"S024":8,"S025":4}

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def read_frames(record):
    source = Image.open(record["path"]).convert("RGBA")
    cols, rows = record["sourceColumns"], record["sourceRows"]
    frames = []
    for row in range(rows):
        for col in range(cols):
            cell = source.crop((round(col*source.width/cols), round(row*source.height/rows),
                                round((col+1)*source.width/cols), round((row+1)*source.height/rows)))
            # Discard low-alpha generation haze; resize in premultiplied alpha.
            cutoff = 128 if record["id"] == "S024" and record["phase"] == "hit" else 32
            cell.putalpha(cell.getchannel("A").point(lambda a: 0 if a <= cutoff else round((a-cutoff)*255/(255-cutoff))))
            cell = cell.convert("RGBa").resize((SIZE, SIZE), Image.Resampling.LANCZOS).convert("RGBA")
            cell.putalpha(cell.getchannel("A").point(lambda a: 0 if a < 4 else a))
            frames.append(cell)
    return frames

def main():
    records = json.loads((WORK/"final-generation-records.json").read_text(encoding="utf8"))
    baseline = json.loads((WORK/"baseline.json").read_text(encoding="utf8"))
    for name, digest in baseline["files"].items():
        assert sha(ROOT/name) == digest, name
    manifest_path = ROOT/"resources/beast-spell-effects.manifest.json"
    manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
    assert manifest["assets"][:48] == baseline["records"]
    original_dir = WORK/"generated-originals"
    original_dir.mkdir(exist_ok=True)
    preview_dir = WORK/"previews"
    preview_dir.mkdir(exist_ok=True)
    additions, checks, previews = [], [], {}
    for spell in IDS:
        selected = {r["phase"]:r for r in records if r["id"] == spell}
        phases = {p:read_frames(r) for p,r in selected.items()}
        assert len(phases["cast"]) == 8 and len(phases["hit"]) == 24
        # The cast already gathers the motif: hold/re-time the formed hit poses
        # instead of replaying their redundant initial gathering frames.
        start = HIT_START.get(spell, 0)
        original_hit = phases["hit"]
        phases["hit"] = [original_hit[start+round(i*(23-start)/23)] for i in range(24)]
        longest = max(max(b[2]-b[0], b[3]-b[1]) for fs in phases.values()
                      for f in fs if (b := f.getchannel("A").getbbox()))
        factor = min(1.0, 134/longest)
        output_frames = {}
        for phase in ("cast", "hit"):
            frames = []
            for cell in phases[phase]:
                bounds = cell.getchannel("A").getbbox()
                out = Image.new("RGBA",(SIZE,SIZE))
                if bounds:
                    body = cell.crop(bounds)
                    body = body.convert("RGBa").resize((max(1,round(body.width*factor)),
                           max(1,round(body.height*factor))),Image.Resampling.LANCZOS).convert("RGBA")
                    body.putalpha(body.getchannel("A").point(lambda a: 0 if a < 4 else a))
                    out.alpha_composite(body,((SIZE-body.width)//2,(SIZE-body.height)//2))
                frames.append(out)
            output_frames[phase] = frames
            atlas = Image.new("RGBA",(SIZE*4,SIZE*(len(frames)//4)))
            for i, frame in enumerate(frames):
                atlas.alpha_composite(frame,((i%4)*SIZE,(i//4)*SIZE))
                b=frame.getchannel("A").getbbox()
                assert b and min(b[0],b[1],SIZE-b[2],SIZE-b[3])>=27,(spell,phase,i,b)
            relative = f"GameResources/effects/Spells/Stages/{spell}/{phase}.png"
            target=ROOT/relative
            target.parent.mkdir(parents=True,exist_ok=True)
            atlas.save(target)
            raw=original_dir/f"{spell}_{phase}.png"
            shutil.copyfile(selected[phase]["path"],raw)
            additions.append(dict(id=f"{spell}_{phase}",spell=spell,phase=phase,path=relative,
                width=atlas.width,height=atlas.height,frameSize=SIZE,columns=4,frames=len(frames),
                anchor="beneficiary" if phase=="hit" else "caster",status="offline-reviewed",
                prompt=selected[phase]["prompt"],sha256=sha(target),
                provenance=dict(tool="built-in-imagegen",source=raw.relative_to(ROOT).as_posix(),
                    sourceSha256=sha(raw),prompt=selected[phase]["prompt"],
                    conversion=dict(tool="Pillow",alphaCutoff=128 if spell=="S024" and phase=="hit" else 32,premultipliedResize=True,
                        sharedSpellScale=factor,centerPivot=True,minimumPaddingPixels=27,
                        sourceTimelineStartFrame=start if phase=="hit" else 0)),
                unityRetested=False))
            checks.append(dict(spell=spell,phase=phase,frames=len(frames),minimumMarginPixels=27,sha256=sha(target)))
        previews[spell]=output_frames["cast"]+output_frames["hit"]
        view=[]
        for frame in previews[spell]:
            bg=Image.new("RGBA",(SIZE,SIZE),(35,39,32,255));bg.alpha_composite(frame);view.append(bg.convert("RGB"))
        view[0].save(preview_dir/f"{spell}.gif",save_all=True,append_images=view[1:],
                     duration=30,loop=0,disposal=2)
    existing = [a for a in manifest["assets"] if a["spell"] not in IDS]
    assert existing == baseline["records"]
    manifest["assets"]=existing+additions
    manifest_path.write_text(json.dumps(manifest,ensure_ascii=False,indent=2)+"\n",encoding="utf8")
    montage=Image.new("RGB",(SIZE*5,SIZE*2+24*2),(35,39,32))
    draw=ImageDraw.Draw(montage)
    for i,spell in enumerate(IDS):
        x,y=i%5*SIZE,i//5*(SIZE+24);draw.text((x+8,y+4),spell,fill="white")
        montage.paste(previews[spell][18],(x,y+24),previews[spell][18])
    montage.save(preview_dir/"overview.png")
    animated=[]
    for f in range(32):
        board=Image.new("RGB",montage.size,(35,39,32)); draw=ImageDraw.Draw(board)
        for i,spell in enumerate(IDS):
            x,y=i%5*SIZE,i//5*(SIZE+24);draw.text((x+8,y+4),spell,fill="white")
            board.paste(previews[spell][f],(x,y+24),previews[spell][f])
        animated.append(board)
    animated[0].save(preview_dir/"all-spells.gif",save_all=True,append_images=animated[1:],
                      duration=30,loop=0,disposal=2)
    for name,digest in baseline["files"].items():
        assert sha(ROOT/name)==digest,name
    report=dict(newAtlases=20,newFrames=320,existingAtlasesUnchanged=48,
                existingRecordsUnchanged=48,alphaBackgroundCleaned=True,checks=checks,
                offlinePreviewGenerated=True,unityRetested=False)
    (WORK/"asset-verification.json").write_text(json.dumps(report,indent=2),encoding="utf8")
    print(json.dumps({k:v for k,v in report.items() if k!="checks"}))

if __name__=="__main__":
    main()
