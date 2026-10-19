"""Draw each authorized program-art frame on a fresh canvas, without loading image pixels.

All poses are geometry authored below. No previous sprite, image transform, sheet,
crop, resampling, or palette replacement is involved in production drawing.
"""
from __future__ import annotations
import argparse
from datetime import datetime, timezone
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import shutil
from PIL import Image, ImageDraw

spec = importlib.util.spec_from_file_location("authoring", Path(__file__).with_name("actor-redraw.py"))
A = importlib.util.module_from_spec(spec)
spec.loader.exec_module(A)
ROOT, WORK = A.ROOT, A.WORK
OUTLINE = "#101625"
SKIN = "#ead1b0"
GOLD = "#e9b54a"
WHITE = "#e5edf2"
PALE = "#b5c7d4"
# Main, shadow, highlight: individually authored identities from the inspected baseline.
PALETTES = {
    "kunpeng": ("#167bd2", "#102c67", "#71ddf6"),
    "qinglong": ("#14b4c9", "#155079", "#96ecdf"),
    "huanglong": ("#d88c1f", "#75441b", "#ffe184"),
    "zhuque": ("#d74026", "#791e2b", "#ffb643"),
    "baihu": ("#cbd4df", "#4d536f", "#f6f0df"),
    "qilin": ("#439777", "#244c49", "#a9deb1"),
    "moqilin": ("#584371", "#251e3e", "#b991d5"),
    "xuanwu": ("#268295", "#173f4d", "#a4baa0"),
    "fenghuang": ("#e26127", "#9d2537", "#ffd669"),
    "dafeng": ("#2e77c3", "#153b64", "#b1e8f1"),
    "manman": ("#a34239", "#283744", "#e6d4b1"),
    "xuanniao": ("#503769", "#251c3a", "#937caf"),
    "qingluan": ("#15a6b7", "#16465d", "#a5e7de"),
    "changliao": ("#835d58", "#3a3137", "#d7b8a0"),
    "mo": ("#6f558a", "#30273b", "#cbb4db"),
    "hongmao": ("#ad4b32", "#663329", "#e9bb7b"),
    "feiqiu": ("#e4b0ad", "#795486", "#fff0c3"),
    "yingying": ("#382b53", "#1c1930", "#755d9c"),
    "goushou": ("#662733", "#241f29", "#b8574d"),
    "liejiejing": ("#2375bb", "#143459", "#92daf1"),
    "tianshou": ("#d2c1a2", "#716655", "#f7ebd0"),
    "diyi": ("#b6522e", "#6b302c", "#efc184"),
    "disanmo": ("#766080", "#3e304e", "#c5adca"),
    "qingshenlong": ("#2876bd", "#163669", "#a9e3f1"),
    "lifan": ("#303f72", "#171f3d", "#c2d5e8"),
    "tianyi": ("#cecaaa", "#62674d", "#f5f0d9"),
    "moruibin": ("#938073", "#4b3a37", "#e0d1b9"),
    "xuantianwang": ("#50417b", "#211d39", "#a791c4"),
    "xuke": ("#bf883c", "#6a452b", "#f1d38b"),
    "yefeipeng": ("#397cc5", "#243d66", "#d0e9f3"),
    "xiaoheng": ("#84836a", "#434f42", "#d8d6af"),
    "xubai": ("#d4deec", "#596786", "#faf7e6"),
    "liping": ("#916548", "#4f302b", "#cfa982"),
    "shanfan": ("#d2973a", "#7c4e27", "#ffe9b7"),
    "zhangfan": ("#653044", "#281e34", "#b86b72"),
    "zhenshen": ("#d3a545", "#775033", "#fff4c5"),
    "Bai": ("#e2e6ef", "#55547b", "#fff8ed"),
    "Chuanfa": ("#353343", "#181b29", "#cea448"),
}
PERSONS = {
    "tianyi": {"hair": "white", "weapon": "staff", "collar": "sage", "hat": "bun", "beard": True},
    "moruibin": {"hair": "dark", "weapon": "fan", "collar": "scholar", "hat": "cap"},
    "xuantianwang": {"hair": "dark", "weapon": "sword", "collar": "armor", "hat": "crown"},
    "xuke": {"hair": "dark", "weapon": "staff", "collar": "scholar", "hat": "tall"},
    "yefeipeng": {"hair": "dark", "weapon": "sword", "collar": "feather", "hat": "band"},
    "xiaoheng": {"hair": "dark", "weapon": "gourd", "collar": "traveler", "hat": "bun"},
    "xubai": {"hair": "white", "weapon": "fan", "collar": "scholar", "hat": "band"},
    "liping": {"hair": "dark", "weapon": "axe", "collar": "short", "hat": "cap"},
    "shanfan": {"hair": "dark", "weapon": "staff", "collar": "sage", "hat": "crown"},
    "zhangfan": {"hair": "dark", "weapon": "sword", "collar": "armor", "hat": "band"},
    "zhenshen": {"hair": "white", "weapon": "staff", "collar": "celestial", "hat": "crown"},
    "Bai": {"hair": "dark", "weapon": "fan", "collar": "sage", "hat": "bun"},
    "Chuanfa": {"hair": "dark", "weapon": "book", "collar": "celestial", "hat": "tall"},
}

def lerp(a, b, t): return a + (b - a) * t
def clamp(x, lo=0., hi=1.): return min(hi, max(lo, x))
def rotate(point, angle):
    x, y = point
    return x * math.cos(angle) - y * math.sin(angle), x * math.sin(angle) + y * math.cos(angle)

class Pen:
    """A per-frame list of newly drawn primitives, independent of any bitmap."""
    def __init__(self): self.commands = []
    def poly(self, points, fill, outline=OUTLINE): self.commands.append(["polygon", points, fill, outline])
    def line(self, points, fill, width=1): self.commands.append(["line", points, fill, width])
    def oval(self, box, fill, outline=OUTLINE): self.commands.append(["ellipse", box, fill, outline])
    def limb(self, a, b, c, color, width=5):
        self.line([a,b,c], OUTLINE, width+2)
        self.line([a,b,c], color, width)
        self.line([a,(lerp(a[0],b[0],.7),lerp(a[1],b[1],.7))], WHITE, 1)
    def bounds(self):
        xs,ys=[],[]
        for kind, pts, _, extra in self.commands:
            if kind == "ellipse":
                coords=[(pts[0],pts[1]),(pts[2],pts[3])]; padding=1
            else: coords=pts; padding=extra/2+1 if kind=="line" else 1
            for x,y in coords: xs.extend((x-padding,x+padding)); ys.extend((y-padding,y+padding))
        return min(xs),min(ys),max(xs),max(ys)
    def render(self, canvas, anchor, scale):
        image=Image.new("RGBA", canvas)
        draw=ImageDraw.Draw(image)
        def point(p): return round(anchor[0]+p[0]*scale),round(anchor[1]+p[1]*scale)
        for kind, pts, fill, extra in self.commands:
            if kind=="ellipse":
                box=(*point((pts[0],pts[1])),*point((pts[2],pts[3])))
                draw.ellipse(box, fill=fill, outline=extra)
            elif kind=="polygon": draw.polygon([point(p) for p in pts],fill=fill,outline=extra)
            else: draw.line([point(p) for p in pts],fill=fill,width=max(1,round(extra*scale)),joint="curve")
        return image

def motion(action, index, count):
    cycle=2*math.pi*(index/count + (.125 if action=="walk" else 0))
    t=index/max(1,count-1)
    strike=math.sin(math.pi*clamp((t-.2)/.6)) if action=="attack" else 0
    # Attack poses contain anticipation, extension and recovery.
    return {"cycle":cycle,"t":t,"sway":math.sin(cycle),"breath":math.cos(cycle),
            "strike":strike,"recoil":(1-t) if action=="hit" else 0,
            "fall":t if action=="death" else 0,"action":action,"index":index,
            "dead":action=="death","moving":action=="walk","icon":action=="icon"}

def human(p, cfg, m):
    main, dark, light=cfg["palette"]; species=cfg["species"]
    custom=cfg["humanDesign"]; a=m["action"]; t=m["t"]
    fall=m["fall"]; angle=fall*math.pi/2
    breath=.7*m["breath"] if a=="idle" else 0
    bounce=-abs(m["sway"])*1.5 if m["moving"] else 0
    lean=-4*m["recoil"] + 3*m["strike"]
    origin=(-fall*31, -18*(1-fall)-15*fall+bounce)
    def pt(x,y):
        q=rotate((x+lean*(1-y/-75),y+breath),angle)
        return q[0]+origin[0],q[1]+origin[1]
    step=5*m["sway"] if m["moving"] else 1.1*m["sway"]
    for z,color in ((-1,dark),(1,main)):
        p.limb(pt(z*6,-2),pt(z*6+step*z,7),pt(z*7-step*z,16),color,6)
        foot=pt(z*7-step*z,17)
        p.line([foot,(foot[0]+5,foot[1])],OUTLINE,3)
    cape_sway=2*math.sin(m["cycle"]+.6)
    p.poly([pt(-13,-54),pt(10,-50),pt(22+cape_sway,-7),pt(-19+cape_sway,8)],dark)
    hem=3*math.sin(m["cycle"]+.5) if not m["dead"] else -fall*3
    p.poly([pt(-10,-43),pt(10,-43),pt(15+hem,9),pt(2,12),pt(-17+hem,6)],main)
    p.poly([pt(-2,-40),pt(5,-38),pt(8+hem,8),pt(-2,8)],light,None)
    for x in (-11,-6,9): p.line([pt(x,-14),pt(x+hem,5)],dark,1)
    p.poly([pt(-11,-57),pt(10,-57),pt(12,-34),pt(-12,-34)],main)
    p.poly([pt(-11,-55),pt(-2,-43),pt(8,-54),pt(2,-55),pt(-2,-49)],light)
    p.line([pt(-11,-33),pt(11,-33)],GOLD,3)
    if custom["collar"]=="armor":
        for y in (-49,-44,-39): p.line([pt(-8,y),pt(8,y)],GOLD,1)
        p.poly([pt(-12,-57),pt(-19,-53),pt(-15,-44),pt(-8,-47)],dark)
    if custom["collar"] in ("feather","celestial"):
        for z in (-1,1):
            for k in range(4): p.poly([pt(z*(8+k*2),-55),pt(z*(14+k*2),-45+k*2),pt(z*(9+k*2),-44+k*2)],light)
    # Exactly two arms, each drawn as its own continuous shoulder/elbow/hand chain.
    farhand=(17,-28-8*m["strike"])
    nearhand=(19+13*m["strike"],-28-20*m["strike"])
    if a=="attack" and t<.2: nearhand=(2,-44)
    if a=="hit": nearhand=(8,-35-6*m["recoil"])
    if m["icon"]: nearhand=(23,-43)
    for shoulder, elbow, hand, color in [((-9,-53),(-15,-38),farhand,dark),((8,-52),(17,-40),nearhand,main)]:
        p.limb(pt(*shoulder),pt(*elbow),pt(*hand),color,7)
        q=pt(*hand); p.oval([q[0]-2,q[1]-2,q[0]+2,q[1]+2],SKIN)
    # Face and hair share the same pose geometry; they are never rotated bitmaps.
    p.oval([*pt(-7,-77),*pt(7,-62)] if not fall else _ellipse_box(pt,0,-69,7,8),SKIN)
    hair=WHITE if custom["hair"]=="white" else "#202532"
    p.poly([pt(-8,-69),pt(-8,-81),pt(2,-83),pt(8,-78),pt(7,-72),pt(0,-76),pt(-3,-69)],hair)
    p.poly([pt(-7,-75),pt(-12,-65),pt(-10+cape_sway,-45),pt(-4,-54)],hair)
    p.line([pt(2,-70),pt(5,-70)],OUTLINE,1)
    if not m["dead"] and not (a=="hit" and t<.5): p.line([pt(4,-70),pt(4,-69)],"#604e36",1)
    p.line([pt(4,-64),pt(6,-64)],"#936b59",1)
    if custom.get("beard"): p.poly([pt(-2,-62),pt(5,-62),pt(1,-52)],WHITE)
    hat=custom["hat"]
    if hat=="bun": p.oval(_ellipse_box(pt,-3,-84,4,3),hair); p.line([pt(-7,-83),pt(3,-83)],GOLD,1)
    elif hat=="crown": p.poly([pt(-7,-80),pt(-6,-89),pt(-2,-84),pt(1,-91),pt(4,-84),pt(7,-88),pt(7,-79)],GOLD)
    elif hat=="tall": p.poly([pt(-7,-78),pt(-6,-90),pt(6,-88),pt(7,-78)],dark); p.line([pt(-6,-82),pt(6,-82)],GOLD,1)
    elif hat=="cap": p.poly([pt(-8,-77),pt(-8,-83),pt(7,-83),pt(9,-77)],dark)
    else: p.line([pt(-7,-76),pt(7,-77)],GOLD,2)
    # Beast ascensions keep species-specific visible horns, feathers, tail or mask.
    if cfg.get("ascended"):
        family=cfg["beastFamily"]
        if family in ("dragon","quadruped","heavy"):
            for z in (-1,1): p.poly([pt(z*5,-79),pt(z*11,-90),pt(z*9,-78)],GOLD)
        if family=="bird":
            for k in range(3): p.poly([pt(-12,-57+k*3),pt(-27,-42+k*9),pt(-15,-34+k*3)],light)
        if family=="aquatic": p.poly([pt(-14,-51),pt(-26,-47),pt(-16,-29)],light)
        if species in ("mo","disanmo","yingying","feiqiu"):
            p.line([pt(-6,-70),pt(5,-70)],dark,3)
        p.line([pt(-13,-9),pt(-24,-7+cape_sway),pt(-27,-16+cape_sway)],main,4)
    h=pt(*nearhand); v=rotate((0,-1),angle)
    weapon=custom["weapon"]
    if weapon in ("sword","staff","axe"):
        tip=(h[0]+v[0]*25,h[1]+v[1]*25)
        p.line([h,tip],OUTLINE,4)
        p.line([h,tip],PALE if weapon=="sword" else GOLD,2)
        if weapon=="axe": p.poly([(tip[0]-7,tip[1]+2),(tip[0]+6,tip[1]-3),(tip[0]+4,tip[1]+7)],PALE)
        elif weapon=="staff": p.oval([tip[0]-3,tip[1]-3,tip[0]+3,tip[1]+3],light)
        else: p.line([(h[0]-4,h[1]-3),(h[0]+4,h[1]-3)],GOLD,2)
    elif weapon=="fan": p.poly([(h[0],h[1]),(h[0]-7,h[1]-11),(h[0]+4,h[1]-14),(h[0]+11,h[1]-6)],light)
    elif weapon=="book": p.poly([(h[0]-5,h[1]-10),(h[0]+8,h[1]-10),(h[0]+8,h[1]),(h[0]-5,h[1])],light); p.line([(h[0]+1,h[1]-8),(h[0]+1,h[1]-1)],GOLD,1)
    else: p.oval([h[0]-4,h[1]-3,h[0]+4,h[1]+6],GOLD)

def _ellipse_box(pt,x,y,rx,ry):
    center=pt(x,y); return [center[0]-rx,center[1]-ry,center[0]+rx,center[1]+ry]

def claws(p,foot,color=WHITE, direction=1):
    x,y=foot
    for k in range(3): p.poly([(x+k*2-2,y),(x+k*2-1,y+3),(x+k*2+1,y+1)],color)

def quadruped(p,cfg,m):
    main,dark,light=cfg["palette"]; s=cfg["species"]
    thick=cfg["bodyWidth"]; fall=m["fall"]
    cy=lerp(-25,-11,fall)+(-2*abs(m["sway"]) if m["moving"] else -.7*m["breath"])
    length=23+(5 if s in ("changliao","goushou","yingying") else 0)
    tail=[(-length,cy-4),(-length-9,cy-8+3*m["sway"]),(-length-12,cy-17+4*m["breath"])]
    p.line(tail,OUTLINE,5); p.line(tail,main,3)
    # Diagonal pairs alternate for walk; attack collects the hindlegs before pouncing.
    for z in (-1,1):
        for front in (False,True):
            rootx=(length-3 if front else -length+5)+z*2
            stride=(5*math.sin(m["cycle"]+(0 if front==(z==1) else math.pi))) if m["moving"] else z*.5*m["sway"]
            foot=(rootx+stride+6*m["strike"],lerp(0,-1,fall))
            knee=(rootx+stride*.4,cy+thick*.7)
            p.limb((rootx,cy+4),knee,foot,dark if z==-1 else main,5)
            claws(p,foot)
    p.oval([-length,cy-thick,length,cy+thick],main)
    p.oval([-length+3,cy+1,length-3,cy+thick-2],light,None)
    for x in range(-length+5,length-2,7):
        if s in ("qilin","moqilin","disanmo"):
            p.poly([(x,cy-5),(x+3,cy-1),(x,cy+3),(x-3,cy-1)],dark,None)
        else: p.line([(x,cy-thick+2),(x+3,cy-3)],dark,2)
    hx=length-1+7*m["strike"]-3*m["recoil"]
    hy=cy-8+fall*7
    p.poly([(hx-5,hy-9),(hx+9,hy-8),(hx+15,hy+1),(hx+7,hy+10),(hx-6,hy+5)],main)
    muzzle=light if s not in ("yingying","goushou") else dark
    p.poly([(hx+6,hy+1),(hx+16,hy+2),(hx+15,hy+7),(hx+5,hy+7)],muzzle)
    p.poly([(hx-5,hy-6),(hx-7,hy-16),(hx+1,hy-9)],dark)
    p.poly([(hx+4,hy-8),(hx+7,hy-15),(hx+10,hy-7)],dark)
    p.line([(hx+6,hy-1),(hx+10,hy-1)],GOLD if s!="yingying" else "#d784ba",1)
    if m["dead"]: p.line([(hx+6,hy-1),(hx+10,hy+1)],OUTLINE,1)
    p.oval([hx+12,hy+1,hx+15,hy+3],OUTLINE)
    jaw=hy+7+5*m["strike"]
    p.line([(hx+7,hy+7),(hx+15,jaw)],OUTLINE,2)
    if s in ("changliao","goushou"): p.poly([(hx+10,hy+5),(hx+9,jaw+5),(hx+13,hy+6)],WHITE)
    if s in ("qilin","moqilin","disanmo","tianshou"):
        p.poly([(hx-2,hy-9),(hx-4,hy-21),(hx+2,hy-14),(hx+3,hy-10)],GOLD)
        for k in range(6): p.poly([(hx-9,hy-10+k*4),(hx-16,hy-5+k*4),(hx-8,hy-1+k*4)],light)
    if s in ("hongmao","diyi"):
        for x in range(-length,length,4): p.poly([(x,cy-thick+2),(x+2,cy-thick-5),(x+5,cy-thick+3)],light)
    if s in ("mo","disanmo"):
        p.poly([(hx+12,hy+2),(hx+17,hy+6),(hx+16,hy+16),(hx+12,hy+14)],main)
        p.line([(hx+14,hy+6),(hx+14,hy+13)],light,1)
    if s=="yingying":
        for x in (-18,-9,0): p.poly([(x,cy-thick),(x-4,cy-thick-7),(x+5,cy-thick+1)],dark)

def bird(p,cfg,m):
    main,dark,light=cfg["palette"]; s=cfg["species"]; fall=m["fall"]
    flying=m["moving"] or (m["action"]=="attack" and m["strike"]>.1)
    cy=lerp(-55 if flying else -29,-11,fall) + (-3*m["sway"] if flying else -.7*m["breath"])
    head=(10+8*m["strike"],lerp(cy-24,cy-2,fall))
    wing_angle=(-.75+1.1*m["sway"]) if flying else -.1+.12*m["breath"]
    if m["dead"]: wing_angle=.8+fall*.4
    # Opposite wings share joint lengths but retain near/far layering.
    for z,color in ((-1,dark),(1,main)):
        root=(z*4,cy-3)
        elbow=(root[0]+z*13*math.cos(wing_angle),root[1]+13*math.sin(wing_angle))
        wrist=(elbow[0]+z*16*math.cos(wing_angle+.3),elbow[1]+16*math.sin(wing_angle+.3))
        if m["dead"]:
            elbow=(root[0]+z*13,cy-8)
            wrist=(root[0]+z*25,cy-2)
        p.poly([root,elbow,wrist,(wrist[0]-z*4,wrist[1]+12),(root[0],root[1]+15)],color)
        for k in range(5):
            end=(wrist[0]+z*(7-k),wrist[1]+10+k*3)
            if m["dead"]: end=(wrist[0]+z*(7-k),wrist[1]-k*2)
            p.poly([(elbow[0]+z*k*2,elbow[1]+3+k),end,(end[0]-z*4,end[1]+2)],light if k%2==0 else color)
    tail_swing=3*math.sin(m["cycle"]+.7)
    for k in range(3):
        start=(-5+k*4,cy+12)
        end=(-25+k*13+tail_swing,lerp(-1,-3,fall)+k%2*3)
        p.poly([start,(end[0]-4,end[1]-8),end,(end[0]+4,end[1]-8),(start[0]+3,start[1])],dark)
        p.line([start,end],light,2)
        if s in ("fenghuang","zhuque","qingluan"): p.oval([end[0]-3,end[1]-8,end[0]+3,end[1]-2],GOLD)
    p.oval([-12,cy-16,13,cy+16],main)
    p.oval([-3,cy-8,10,cy+10],light,None)
    p.line([(3,cy-8),head],main,11)
    p.oval([head[0]-7,head[1]-7,head[0]+8,head[1]+7],main)
    p.poly([(head[0]+5,head[1]-1),(head[0]+16,head[1]+3),(head[0]+5,head[1]+4)],GOLD)
    p.line([(head[0]+2,head[1]-2),(head[0]+5,head[1]-2)],OUTLINE if m["dead"] else GOLD,1)
    for k in range(3): p.poly([(head[0]-6+k*3,head[1]-5),(head[0]-12+k*4,head[1]-15-k%2*3),(head[0]-2+k*2,head[1]-6)],light)
    for z in (-1,1):
        root=(z*5,cy+12)
        foot=(z*8, -7 if flying else -1)
        p.line([root,foot],OUTLINE,3);p.line([root,foot],GOLD,1)
        p.line([(foot[0]-3,foot[1]),(foot[0]+4,foot[1])],GOLD,2)
    if s=="manman":
        # The paired-bird identity is expressed by a second small head/neck.
        second=(-4,cy-20)
        p.line([(-6,cy-4),second],dark,7)
        p.oval([second[0]-5,second[1]-5,second[0]+5,second[1]+4],light)
        p.poly([(second[0]+3,second[1]),(second[0]+10,second[1]+2),(second[0]+3,second[1]+3)],GOLD)

def dragon(p,cfg,m):
    main,dark,light=cfg["palette"]; fall=m["fall"]
    # A newly drawn continuous serpentine body, sampled from authored joint positions.
    upright=[(-10,-10),(20,-23),(26,-49),(-25,-68),(-32,-102),(10,-123),(6,-150),(29,-162)]
    prone=[(-64,-3),(-34,-14),(0,-6),(-21,-29),(5,-42),(30,-28),(52,-12),(72,-7)]
    points=[]
    for k,(u,v) in enumerate(zip(upright,prone)):
        wave=3*math.sin(m["cycle"]-k*.7) if m["moving"] else 1.8*math.sin(m["cycle"]-k*.4)
        points.append((lerp(u[0],v[0],fall)+wave+12*m["strike"]*(k/7)**2-6*m["recoil"]*(k/7),lerp(u[1],v[1],fall)))
    p.line(points,OUTLINE,20);p.line(points,main,17)
    p.line([(x+3,y+3) for x,y in points],light,5)
    for k,(x,y) in enumerate(points[:-1]):
        p.poly([(x-8,y-3),(x-16,y-11),(x-5,y-7)],GOLD)
        for q in range(3): p.line([(x-3+q*3,y-5),(x+q*3,y-2)],dark,1)
    for k in (3,5):
        root=points[k]
        for z in (-1,1):
            end=(root[0]+z*(18-7*m["strike"]),root[1]+16*(1-fall)+4)
            p.limb(root,(root[0]+z*11,root[1]+8),end,dark if z==-1 else main,5)
            claws(p,end)
    hx,hy=points[-1]
    p.poly([(hx-11,hy-10),(hx+8,hy-12),(hx+25,hy-3),(hx+23,hy+7),(hx+3,hy+10),(hx-10,hy+4)],main)
    p.line([(hx+5,hy+8),(hx+22,hy+8+7*m["strike"])],light,5)
    p.line([(hx+5,hy+5),(hx+22,hy+7)],OUTLINE,2)
    p.line([(hx+8,hy-3),(hx+12,hy-3)],OUTLINE if m["dead"] else GOLD,2)
    for z in (-1,1):
        p.line([(hx+z*4,hy-8),(hx+z*8,hy-21),(hx+z*14,hy-27)],OUTLINE,4)
        p.line([(hx+z*4,hy-8),(hx+z*8,hy-21),(hx+z*14,hy-27)],GOLD,2)
    for k in range(5): p.poly([(hx-8,hy-8+k*4),(hx-21,hy-14+k*6),(hx-8,hy-3+k*4)],light)
    for k in (-1,1):
        p.line([(hx+20,hy+4),(hx+32,hy+k*8),(hx+34,lerp(hy+15+k*5,-2,fall)),(hx+24,lerp(hy+18+k*5,-1,fall))],light,2)
    tail=points[0]
    p.poly([tail,(tail[0]-15,tail[1]-8),(tail[0]-20,tail[1]+2),(tail[0]-5,tail[1]+5)],light)

def aquatic(p,cfg,m):
    main,dark,light=cfg["palette"]; fall=m["fall"]; cy=-52+7*fall
    wave=(7*m["sway"] if m["moving"] else 2*m["sway"])*(1-fall)
    hx=50+8*m["strike"]-5*m["recoil"]
    outline=[(-60,cy-4),(-44,cy-24),(-15,cy-38),(24,cy-37),(hx+22,cy-22),
             (hx+25,cy+4),(hx+13,cy+24),(15,cy+31),(-17,cy+20),(-42,cy+7)]
    p.poly(outline,main)
    p.poly([(-37,cy+6),(-8,cy+19),(24,cy+24),(hx+17,cy+10),(hx+10,cy+25),(15,cy+31),(-17,cy+20)],PALE)
    for x in range(21,70,8):p.line([(x,cy+6),(x-10,cy+25)],WHITE,2)
    tail=(-76,cy+wave)
    p.poly([(-50,cy-5),tail,(-106,cy-23+wave),(-108,cy-10+wave),(-85,cy+5+wave),(-109,cy+27+wave),(-99,cy+30+wave),(-68,cy+10+wave),(-45,cy+6)],main)
    p.line([tail,(-99,cy-15+wave)],light,3);p.line([tail,(-99,cy+23+wave)],light,3)
    finwave=9*math.sin(m["cycle"]+.6) if m["moving"] else 3*m["sway"]
    for z,col in ((-1,dark),(1,main)):
        root=(22+z*13,cy+15)
        tip=(root[0]-25+finwave*.5,cy+50+finwave*(1-fall))
        p.poly([root,(root[0]+15,cy+23),tip,(tip[0]-8,tip[1]-5)],col)
        p.line([root,tip],light,3)
    for x in (-32,-6,23): p.poly([(x,cy-29),(x-8,cy-44),(x+5,cy-46),(x+16,cy-29)],light)
    for x in range(-35,55,18):p.line([(x,cy-17),(x+8,cy-10),(x+4,cy+1)],light,2)
    p.line([(hx+3,cy-9),(hx+9,cy-9)],OUTLINE if m["dead"] else GOLD,2)
    p.line([(hx-4,cy+2),(hx+23,cy+5+12*m["strike"])],OUTLINE,2)
    for x in (hx-1,hx+9):p.poly([(x,cy-23),(x-5,cy-44),(x+2,cy-31)],GOLD)
    if cfg["species"]=="kunpeng":
        for k in range(5): p.poly([(-4,cy-18),(-22+k*10,cy-62-7*math.sin(m["cycle"])*(1-fall)),(8+k*5,cy-22)],light if k%2 else main)

def heavy(p,cfg,m):
    if cfg["species"]=="tianshou":
        quadruped(p,{**cfg,"bodyWidth":18},m)
        main,dark,light=cfg["palette"]
        for x in (-19,-8,3,14): p.poly([(x,-33),(x-3,-45),(x+5,-39),(x+8,-30)],light)
        return
    main,dark,light=cfg["palette"]; fall=m["fall"]; cy=lerp(-25,-13,fall)-m["breath"]*(1-fall)
    for x in (-23,19):
        for z,col in ((-1,dark),(1,light)):
            foot=(x+3*m["sway"]*(1 if x>0 else -1) if m["moving"] else x, -1)
            p.limb((x,cy+5),(x+z*3,cy+15),foot,col,7);claws(p,foot)
    p.oval([-31,cy-22,30,cy+13],dark)
    p.poly([(-27,cy-9),(-14,cy-21),(8,cy-23),(27,cy-8),(20,cy+10),(-18,cy+12)],main)
    for x,y in ((-11,cy-7),(9,cy-7),(0,cy+6)):
        p.poly([(x-8,y-5),(x,y-9),(x+8,y-5),(x+8,y+4),(x,y+8),(x-8,y+4)],dark)
        p.line([(x-5,y-4),(x,y-6),(x+5,y-4)],GOLD,1)
    hx=31+5*m["strike"]
    p.limb((24,cy+4),(hx,cy+4),(hx+6,cy),light,8)
    p.oval([hx-1,cy-7,hx+14,cy+5],light)
    p.line([(hx+7,cy-2),(hx+10,cy-2)],OUTLINE if m["dead"] else GOLD,1)
    # Xuanwu's coiled snake is separate head anatomy, not a duplicated turtle limb.
    snake=[(-25,cy+5),(-36,cy-7),(-28,cy-31),(-7+2*m["sway"],cy-33),(-2+2*m["sway"],cy-25),(-9+2*m["sway"],cy-23)]
    p.line(snake,OUTLINE,5);p.line(snake,main,3)
    p.oval([-13+2*m["sway"],cy-27,-3+2*m["sway"],cy-20],main)
    p.line([(-5+2*m["sway"],cy-22),(0+2*m["sway"],cy-21)],GOLD,1)

def special(p,cfg,m):
    if cfg["species"]=="yingying": quadruped(p,{**cfg,"bodyWidth":12},m); return
    main,dark,light=cfg["palette"]; fall=m["fall"]
    cy=lerp(-26,-12,fall)+2*m["sway"]*(1-fall)
    # Flying ball: distinct compact body, ears, horns, short four feet and a thin tail.
    p.line([(-20,cy+2),(-29,cy+9),(-25+3*math.cos(m["cycle"]+.5),cy+17)],dark,3)
    for z in (-1,1):
        for y in (cy-4,cy+14): p.limb((z*13,y),(z*19,y+4),(z*22,y+6),dark,3)
    p.oval([-23,cy-22,23,cy+20],light)
    p.oval([-17,cy-13,18,cy+13],main,None)
    for z in (-1,1):
        p.poly([(z*12,cy-14),(z*24,cy-25),(z*22,cy-10)],dark)
        p.poly([(z*5,cy-17),(z*9,cy-28),(z*11,cy-18)],GOLD)
        p.line([(z*7-2,cy-4),(z*7+2,cy-4)],OUTLINE,2)
    p.oval([-6,cy,7,cy+7],light)
    p.line([(-3,cy+7),(4,cy+7+3*m["strike"])],dark,1)

DRAWERS={"human":human,"bird":bird,"dragon":dragon,"quadruped":quadruped,"aquatic":aquatic,"heavy":heavy,"special":special}

def configuration(form):
    species=form["species"]
    family=("bird" if species in A.BIRDS else "dragon" if species in A.DRAGONS else "aquatic" if species in A.AQUATIC else "heavy" if species in A.HEAVY else "special" if species in A.SPECIAL else "quadruped")
    human_design=PERSONS.get(species,{"hair":"white" if species in ("baihu","tianshou") else "dark",
                                   "weapon":"staff" if family in ("bird","aquatic") else "sword",
                                   "collar":"feather" if family=="bird" else "armor" if family in ("dragon","heavy") else "traveler",
                                   "hat":"crown" if family in ("dragon","heavy") else "bun"})
    return {"actorId":form["id"],"species":species,"name":form["name"],"family":form["family"],
            "palette":PALETTES[species],"humanDesign":human_design,"ascended":form["id"].endswith("_yuan"),
            "beastFamily":family,"bodyWidth":14 if species in ("mo","disanmo","hongmao","diyi") else 11,
            "canvas":form["canvas"],"anchor":form["anchor"],"pixelScale":None,
            "referenceCard":form["referenceCard"],"sourcePolicy":"new-primitives-per-frame-no-input-bitmap"}

def make_pen(cfg,action,index,count):
    p=Pen();DRAWERS[cfg["family"]](p,cfg,motion(action,index,count));return p

def shared_scale(form,cfg):
    pen=make_pen(cfg,"idle",0,form["actions"]["idle"]["count"])
    bounds=pen.bounds()
    scale=(form.get("bodyLongestEdge") or form["bodyHeight"])/(max(bounds[2]-bounds[0],bounds[3]-bounds[1]) if form.get("bodyLongestEdge") else bounds[3]-bounds[1])
    # One fixed geometry-to-pixel scale for the entire form; never per-frame fitting.
    for action,clip in form["actions"].items():
        for i in range(clip["count"]):
            l,t,r,b=make_pen(cfg,action,i,clip["count"]).bounds()
            cx,cy=form["anchor"];w,h=form["canvas"]
            for extent, room in ((-l,cx-3),(r,w-cx-3),(-t,cy-3),(b,h-cy-3)):
                if extent>0: scale=min(scale,room/extent)
    return scale

def write_frame(form,cfg,action,index,count,icon=False):
    p=make_pen(cfg,action,index,count)
    canvas=[128,128] if icon else form["canvas"]
    if icon:
        l,t,r,b=p.bounds(); scale=94/max(r-l,b-t)
        anchor=[64-(l+r)*scale/2,65-(t+b)*scale/2]
    else: scale=cfg["pixelScale"];anchor=form["anchor"]
    image=p.render(canvas,anchor,scale)
    if not image.getbbox():raise ValueError("Empty directly drawn frame")
    visible=image.getchannel("A").getbbox()
    if min(visible[:2])<2 or visible[2]>canvas[0]-2 or visible[3]>canvas[1]-2:
        raise ValueError(f"Drawing reaches canvas margin: {form['id']}/{action}_{index}: {visible}")
    output=A.within(WORK,f"icons/{form['id']}.png" if icon else f"frames/{form['id']}/main/{action}_{index}.png")
    if output.exists():raise ValueError("Refuse to overwrite authored frame: "+str(output))
    output.parent.mkdir(parents=True,exist_ok=True)
    image.save(output)
    original=A.within(WORK,f"originals/{form['id']}/{action}_{index}.png")
    original.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(output,original)
    recipe=A.within(WORK,f"pixel-recipes/{form['id']}/{action}_{index}.json")
    record={"actorId":form["id"],"action":action,"index":index,"count":count,"pose":motion(action,index,count),
            "canvas":canvas,"anchor":anchor,"fixedGeometryScale":scale,"commands":p.commands,
            "renderer":Path(__file__).relative_to(ROOT).as_posix(),"rendererSha256":A.digest(Path(__file__)),
            "inputBitmapUsed":False,"spritesheetSlicing":False,"frameIndependentCanvas":True}
    A.save_json(recipe,record)
    proof={"actorId":form["id"],"action":action,"index":index,"method":"program-independent-pixel-draw",
           "sourceFile":original.relative_to(ROOT).as_posix(),"sourceSha256":A.digest(original),
           "outputFile":output.relative_to(ROOT).as_posix(),"outputSha256":A.digest(output),
           "createdAtUtc":datetime.now(timezone.utc).isoformat(),"recipeFile":recipe.relative_to(ROOT).as_posix(),
           "recipeSha256":A.digest(recipe),"references":[form["referenceCard"]],
           "normalization":{"authorized":True,"resampling":"none-native-pixel-drawing","cropped":False,
                            "spritesheetSlicing":False,"canvas":canvas,"inputBitmapUsed":False},
           "review":{k:False for k in ("identity","pixelEdges","anchor","continuity")}}
    A.save_json(output.with_suffix(".provenance.json"),proof)

def archive_staged(actor):
    manifest=A.load_json(A.RESOURCES/"actor-frames.manifest.json")
    if any(g["actorId"]==actor and g.get("approved") for g in manifest["groups"]):
        raise ValueError("Refuse to rebuild an installed and reviewed form: "+actor)
    destination=A.within(WORK,f"rejected/{actor}/program-stage-{datetime.now(timezone.utc).strftime('%Y%m%dT%H%M%S%f')}")
    roots=[A.within(WORK,f"frames/{actor}"),A.within(WORK,f"originals/{actor}"),A.within(WORK,f"pixel-recipes/{actor}")]
    files=[file for folder in roots if folder.exists() for file in folder.rglob("*") if file.is_file()]
    files.extend(file for file in (A.within(WORK,f"icons/{actor}.png"),A.within(WORK,f"icons/{actor}.provenance.json")) if file.exists())
    for file in files:
        resolved=file.resolve()
        if not resolved.is_relative_to(WORK.resolve()): raise ValueError("Unexpected staged source")
        target=A.within(destination,resolved.relative_to(WORK.resolve()).as_posix())
        target.parent.mkdir(parents=True,exist_ok=True)
        shutil.move(str(resolved),str(target))
    print(json.dumps({"archivedStage":actor,"files":len(files),"destination":str(destination)}))

def draw_actor(form,overwrite_stage=False):
    policy=A.load_json(A.RESOURCES/"actor-redraw.targets.json").get("qualityPolicy",{})
    if policy.get("simplifiedProgramArtAccepted") is False:
        raise ValueError("Simplified program art was rejected; use detailed independent imagegen frames.")
    if overwrite_stage: archive_staged(form["id"])
    actor=form["id"];cfg=configuration(form);cfg["pixelScale"]=shared_scale(form,cfg)
    A.save_json(A.RESOURCES/f"actor-pixel-designs/{actor}.json",cfg)
    for action,clip in form["actions"].items():
        for index in range(clip["count"]):write_frame(form,cfg,action,index,clip["count"])
    targets=A.load_json(A.RESOURCES/"actor-redraw.targets.json")
    if any(i["actorId"]==actor for i in targets["icons"]):write_frame(form,cfg,"icon",0,1,True)
    print(json.dumps({"actorId":actor,"frames":sum(c["count"] for c in form["actions"].values()),"pixelScale":cfg["pixelScale"]}))

def main():
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument("--actor",action="append",required=True)
    parser.add_argument("--rebuild-stage",action="store_true")
    args=parser.parse_args();targets=A.load_json(A.RESOURCES/"actor-redraw.targets.json")
    for actor in args.actor:
        form=next(f for f in targets["forms"] if f["id"]==actor);draw_actor(form,args.rebuild_stage)

if __name__=="__main__":main()
