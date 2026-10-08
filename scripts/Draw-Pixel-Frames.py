"""Direct pixel drawing for standalone actor frames (work in progress).

Each call paints a fresh transparent canvas from pose-specific coordinates.
It never cuts a sheet, copies an idle frame, resizes, or quantizes an image.
"""
from __future__ import annotations

from PIL import Image, ImageDraw

SIZE = 120


def color(value):
    return tuple(bytes.fromhex(value.lstrip("#"))) + (255,)


def mix(a, b, amount):
    return tuple(round(a[i]*(1-amount)+b[i]*amount) for i in range(3)) + (255,)


class Canvas:
    def __init__(self):
        self.image = Image.new("RGBA", (SIZE, SIZE), (0, 0, 0, 0))
        self.pen = ImageDraw.Draw(self.image)

    def polygon(self, points, fill, stroke=None):
        self.pen.polygon(points, fill=fill)
        if stroke:
            self.pen.line(points + [points[0]], fill=stroke, width=1, joint="curve")

    def ellipse(self, box, fill, stroke=None):
        self.pen.ellipse(box, fill=fill, outline=stroke, width=1)

    def line(self, points, fill, width=1):
        self.pen.line(points, fill=fill, width=width, joint="curve")

    def rect(self, box, fill):
        self.pen.rectangle(box, fill=fill)


QUAD = {
    "baihu": ("#ececf0", "#ffffff", "#b9bcc8", "#3f4656", "#d9b86e", "cat"),
    "hongmao": ("#b8533d", "#e7975f", "#803e36", "#452e36", "#f0c77b", "cat"),
    "changliao": ("#886348", "#c7a075", "#664c3e", "#372f30", "#dfcfaf", "dog"),
    "goushou": ("#726454", "#b79a7a", "#534438", "#292a2d", "#d5b99a", "dog"),
    "qilin": ("#ad8060", "#ddae81", "#7f584e", "#3d3d43", "#eee1a0", "deer"),
    "moqilin": ("#444354", "#8c7891", "#302e43", "#242330", "#bd8ed0", "deer"),
    "mo": ("#706574", "#b6a7b5", "#4d4355", "#302f3f", "#e2d0d1", "tapir"),
    "tianshou": ("#857565", "#c3ac95", "#605449", "#3b3e45", "#e5d6b9", "deer"),
}

OTHER = {
    "kunpeng": ("whale","#355779","#8db6c2","#243e5e","#162c42","#e2dba0"),
    "qinglong": ("dragon","#4c9b78","#a6dabc","#327562","#244c51","#d4eabc"),
    "huanglong": ("dragon","#b98e52","#efcb82","#90613e","#614838","#ffe2a0"),
    "zhuque": ("bird","#bb4c37","#faa154","#873b38","#622e3c","#ffe185"),
    "xuanwu": ("turtle","#5d8171","#abd0a4","#3f5e59","#263f48","#c7e7d6"),
    "fenghuang": ("bird","#cd6341","#ffbb63","#a83c40","#702a39","#ffec8d"),
    "dafeng": ("bird","#758b9c","#d0e0dd","#54687d","#33475a","#e6e5b8"),
    "manman": ("bird","#98748e","#d5b0bb","#71576f","#484355","#e7deb4"),
    "xuanniao": ("bird","#52667e","#b6c4cc","#35485e","#282f4b","#cedbf1"),
    "qingluan": ("bird","#60a895","#bae1c0","#3b7c7f","#28596a","#dcf3b0"),
    "feiqiu": ("ball","#82a5a0","#d1e0bc","#5c7b83","#3e5864","#f5e3a8"),
    "yingying": ("shadow","#585277","#a293bc","#3b385c","#272a45","#d6bdff"),
    "liejiejing": ("whale","#456c8b","#a5c6d3","#30516f","#203b57","#dcecf0"),
}

NAMED_BEAST = {"DiYi":"fenghuang","DiSanMo":"mo","QingShenLong":"qinglong"}

HUMAN = {
    "LiFan": ("#444b69","#a2a8b5","#d6bd80","disc"),
    "TianYi": ("#a3afa1","#edf0dd","#83c9a2","needle"),
    "Bai": ("#b5bbcc","#f5f5ed","#bcd0ed","sword"),
    "Chuanfa": ("#ae9c75","#e9dbb8","#b6d398","staff"),
    "MoRuBin": ("#565767","#bfc0bc","#c8aa86","scroll"),
    "XuanTianWang": ("#455478","#a2b0ce","#d8b66e","sword"),
    "XuKe": ("#806a55","#c7b58c","#89c9c6","machine"),
    "YeFeiPeng": ("#597993","#c2d9d9","#e2dca0","sword"),
    "XiaoHeng": ("#6e9a85","#c9dfb1","#efdaad","palm"),
    "XuBai": ("#b0b9c5","#f5f3e9","#c9e6f2","palm"),
    "LiPing": ("#827665","#d4bc8b","#f0d498","talisman"),
    "ShanFan": ("#7e9b79","#d7e5bd","#e7d987","disc"),
    "ZhangFan": ("#585a70","#a2aabb","#b8c9e2","dagger"),
    "ZhenShen": ("#9d8da8","#e5d6e8","#f3e6bd","disc"),
    "DiYi": ("#81583d","#dbab65","#ffde88","sword"),
    "DiSanMo": ("#635e7c","#c6b2ce","#ebd0e0","disc"),
    "QingShenLong": ("#4b8291","#a6ddd1","#d9f2d9","sword"),
}


def flecks(a, area, base, highlight, count=16):
    """Short patterned pixel clusters, never resampling or image noise."""
    x0,y0,x1,y1 = area
    for n in range(count):
        x = x0 + (n*17 + n*n*3) % max(1,x1-x0)
        y = y0 + (n*11 + n*n*5) % max(1,y1-y0)
        if a.image.getpixel((x,y))[3] and a.image.getpixel((x,y))[:3] == base[:3]:
            a.line([(x,y),(x+1,y)],highlight,1)


def magic_burst(a, x, y, tone, power, action, index):
    if action == "attack" and index in (2,3,4):
        radius = 2+power*2
        a.ellipse((x-radius,y-radius,x+radius,y+radius),tone)
        for dx,dy in ((-8,0),(8,0),(0,-8),(0,8)):
            a.line([(x+dx//2,y+dy//2),(x+dx,y+dy)],tone,1)
    elif action == "hit" and index in (1,2):
        a.line([(x-4,y-7),(x+3,y),(x,y+5)],tone,2)


def draw_other(name, action, index):
    species = NAMED_BEAST.get(name,name)
    kind, main, light, shade, outline, magic = OTHER[species]
    main,light,shade,outline,magic = map(color,(main,light,shade,outline,magic))
    p = quad_pose(action,index)
    b,l,s,w,r,d = (p[k] for k in ("bob","lean","front","tail","jaw","ear"))
    a = Canvas()
    mid = mix(main,light,.47)
    darkmid = mix(main,outline,.43)
    if action == "death" and index >= 2:
        a.polygon([(21,103),(34,96),(67,93),(91,98),(106,106),
                   (101,110),(30,110)],main,outline)
        a.polygon([(36,98),(58,95),(79,100),(70,104),(41,103)],light)
        a.polygon([(72,99),(91,99),(99,106),(77,107)],shade)
        for x in (44,57,72,84):
            a.line([(x,99),(x+3,103)],darkmid,2)
        a.ellipse((91,100,94,102),outline)
        return a.image
    if kind == "dragon":
        spine = [(23,97+b),(29,83+b+w//3),(40,69+b),(56,63+b),
                 (69,76+b),(79,84+b),(88+l,72+b),(95+l,58+b)]
        a.line(spine,outline,19)
        a.line(spine,main,16)
        a.line(spine[:-2],light,4)
        for x,y in spine[1:-1]:
            a.polygon([(x-4,y-9),(x-2,y-17),(x+3,y-10)],shade,outline)
        a.polygon([(25,90+b),(19,79+b+w//2),(16,82+b+w//2),(21,99+b)],magic,outline)
        for x,y in ((49,79),(76,87)):
            a.line([(x,y+b),(x-3,y+16+b),(x-9,y+18+b)],outline,6)
            a.line([(x,y+b),(x-3,y+16+b),(x-9,y+18+b)],main,4)
            a.line([(x-9,y+18+b),(x-13,y+18+b)],magic,2)
        hx,hy = 91+l,55+b
        a.polygon([(hx-9,hy+2),(hx+7,hy-3),(hx+16+r//2,hy+4),
                   (hx+17+r//2,hy+15),(hx+2,hy+19),(hx-10,hy+13)],main,outline)
        a.polygon([(hx-6,hy+3),(hx+5,hy-1),(hx+12,hy+6),(hx+3,hy+7)],light)
        a.polygon([(hx+5,hy+12),(hx+18+r//2,hy+13),(hx+16+r//2,hy+17+r),
                   (hx+5,hy+16)],shade,outline)
        for dx in (-5,6):
            a.line([(hx+dx,hy),(hx+dx-2,hy-13+d),(hx+dx+2,hy-18+d)],magic,3)
        a.rect((hx+7,hy+7,hx+10,hy+9),outline)
        a.rect((hx+8,hy+7,hx+8,hy+7),magic)
        flecks(a,(32,60,81,91),main,mid,19)
        magic_burst(a,min(112,hx+24),hy+12,magic,p["spell"],action,index)
    elif kind == "bird":
        a.line([(53,89+b),(55+s,106)],outline,5)
        a.line([(75,87+b),(77-s,106)],outline,5)
        for footx in (55+s,77-s):
            for claw in (-4,0,4):
                a.line([(footx,105),(footx+claw,108)],magic,2)
        tailtip = 107 if species in ("fenghuang","zhuque") else 103
        for dx in (-8,0,8):
            a.polygon([(47+dx,80+b),(31+dx,tailtip),
                       (38+dx,87+b),(55+dx,85+b)],shade,outline)
            a.line([(41+dx,88+b),(35+dx,tailtip-3)],magic,2)
        a.ellipse((46,55+b,88,91+b),main,outline)
        a.ellipse((59,61+b,80,80+b),light)
        wingtip = 23+w if action != "attack" else 18+w
        a.polygon([(64,70+b),(42,42+b+w),(wingtip,41+b+w),
                   (30,72+b),(52,84+b)],shade,outline)
        a.polygon([(69,68+b),(86,36+b+w),(105,38+b+w),
                   (103,75+b),(77,87+b)],main,outline)
        for x in (38,47,56):
            a.line([(x,56+b+w//2),(x-7,70+b+w//2)],light,2)
        for x in (88,96,103):
            a.line([(x,57+b+w//2),(x+3,70+b+w//2)],mid,2)
        a.ellipse((73+l,43+b,95+l,67+b),main,outline)
        a.polygon([(77+l,48+b),(83+l,43+b),(90+l,46+b),(92+l,53+b)],light)
        a.polygon([(91+l,55+b),(108+l+r//2,58+b),
                   (92+l,64+b)],magic,outline)
        a.polygon([(78+l,46+b),(82+l,34+b+d),(87+l,43+b)],magic,outline)
        a.rect((86+l,54+b,89+l,56+b),outline)
        if species == "manman":
            a.ellipse((38,69+b,51,83+b),mid,outline)
            a.rect((46,73+b,48,75+b),outline)
        flecks(a,(45,49,89,85),main,mid,15)
        magic_burst(a,min(111,105+l),58+b,magic,p["spell"],action,index)
    elif kind == "whale":
        a.polygon([(23,70+b),(35,58+b),(56,52+b),(80,57+b),
                   (96+l,66+b),(101+l,80+b),(82,89+b),(48,90+b),
                   (28,83+b)],main,outline)
        a.polygon([(30,76+b),(52,76+b),(76,81+b),(91,79+b),
                   (85,86+b),(49,87+b)],light)
        a.polygon([(24,70+b),(14,55+b+w),(16,75+b+w),
                   (14,92+b+w),(30,83+b)],shade,outline)
        a.polygon([(43,83+b),(59,95+b),(69,98+b),(72,78+b)],shade,outline)
        a.polygon([(58,57+b),(55,39+b+w),(73,56+b)],main,outline)
        a.line([(39,65+b),(76,65+b)],mid,3)
        a.line([(43,68+b),(67,68+b)],shade,1)
        a.rect((87+l,69+b,91+l,72+b),outline)
        a.rect((89+l,69+b,89+l,69+b),magic)
        flecks(a,(31,58,84,84),main,mid,15)
        magic_burst(a,min(111,101+l+r),72+b,magic,p["spell"],action,index)
    elif kind == "turtle":
        for x,step in ((39,s),(54,-s),(82,-s),(94,s)):
            a.polygon([(x-6,78+b),(x+5,80+b),(x+6+step,103),
                       (x-8+step,105)],main,outline)
            a.line([(x-8+step,104),(x+6+step,104)],magic,2)
        a.ellipse((30,55+b,96,92+b),main,outline)
        a.ellipse((35,45+b,91,80+b),outline)
        a.ellipse((39,49+b,88,77+b),shade,mid)
        for x in (48,61,75):
            a.line([(x,51+b),(x+5,66+b),(x,75+b)],mid,2)
        a.ellipse((85+l,68+b,106+l+r//2,86+b),main,outline)
        a.ellipse((89+l,70+b,100+l+r//2,76+b),light)
        a.rect((99+l,76+b,101+l,78+b),outline)
        a.line([(29,77+b),(20,88+b+w)],shade,6)
        magic_burst(a,min(111,106+l),69+b,magic,p["spell"],action,index)
    elif kind in ("ball","shadow"):
        if kind == "ball":
            a.ellipse((35+l,44+b,96+l,102+b),main,outline)
            a.ellipse((41+l,48+b,84+l,81+b),light)
            a.polygon([(48+l,49+b),(40+l,35+b+d),(57+l,43+b)],main,outline)
            a.polygon([(81+l,48+b),(95+l,35+b+d),(94+l,62+b)],main,outline)
            for x in (48,60,74,85):
                a.line([(x+l,89+b),(x+3+l,91+b)],shade,2)
        else:
            a.polygon([(25,101),(35,76+b),(41,53+b),(57,42+b),
                       (75,50+b),(92,74+b),(101,101),(85,94+b),
                       (66,107),(46,96+b)],main,outline)
            a.polygon([(41,57+b),(26,49+b+w),(32,78+b)],shade,outline)
            a.polygon([(80,56+b),(99,49+b+w),(94,83+b)],shade,outline)
            a.line([(48,75+b),(51,91+b)],mid,3)
            a.line([(75,72+b),(80,95+b)],mid,3)
        for x in (57,78):
            a.ellipse((x+l,68+b,x+5+l,73+b),magic,outline)
        flecks(a,(38,52,91,96),main,mid,17)
        magic_burst(a,min(111,98+l+r),79+b,magic,p["spell"],action,index)
    if name in NAMED_BEAST:
        a.ellipse((58,37+b,68,44+b),magic,outline)
        a.line([(63,37+b),(63,30+b+d)],magic,2)
    return a.image


def human_colors(name):
    if name in HUMAN:
        robe,light,magic,weapon = HUMAN[name]
        return color(robe),color(light),color(magic),weapon
    species = NAMED_BEAST.get(name,name)
    palette = QUAD.get(species) or OTHER.get(species)
    if species in QUAD:
        robe,light,_,_,magic,kind = palette
    else:
        kind,robe,light,_,_,magic = palette
    return color(robe),color(light),color(magic),"spell"


def draw_human(name, action, index):
    robe,light,magic,weapon = human_colors(name)
    dark = mix(robe,color("#121c2d"),.65)
    shade = mix(robe,dark,.45)
    mid = mix(robe,light,.48)
    skin = color("#dfbd9b")
    hair = color("#252739") if name != "Bai" else color("#1d2742")
    p = quad_pose(action,index)
    b,l,s,w,r,d = (p[k] for k in ("bob","lean","front","tail","jaw","ear"))
    a = Canvas()
    if action == "death" and index >= 2:
        a.polygon([(23,102),(36,94),(54,96),(72,101),(97,105),
                   (99,110),(26,110)],robe,dark)
        a.polygon([(36,95),(47,94),(72,101),(55,103)],light)
        a.ellipse((18,92,35,105),skin,dark)
        a.polygon([(19,94),(21,85),(32,88),(35,98),(27,94)],hair,dark)
        a.line([(80,101),(94,101),(103,108)],magic,2)
        for x in (49,59,69):
            a.line([(x,101),(x+2,106)],shade,1)
        return a.image

    cx = 61+l
    top = 54+b
    hip = 82+b
    foot = 106
    stride = s if action == "walk" else s//2
    # Far arm and two articulated legs appear behind a fresh robe silhouette.
    a.polygon([(cx-11,top+8),(cx-23,top+20+d),(cx-21,top+34+d),
               (cx-8,top+25)],shade,dark)
    a.polygon([(cx-9,hip),(cx-3,hip),(cx-8+stride,foot-4),
               (cx-16+stride,foot-2),(cx-13,hip+6)],dark)
    a.polygon([(cx+3,hip),(cx+10,hip),(cx+13-stride,foot-4),
               (cx+21-stride,foot-2),(cx+11,hip+5)],dark)
    a.polygon([(cx-16+stride,foot-5),(cx-7+stride,foot-5),
               (cx-6+stride,foot),(cx-17+stride,foot)],shade,dark)
    a.polygon([(cx+11-stride,foot-5),(cx+21-stride,foot-5),
               (cx+22-stride,foot),(cx+10-stride,foot)],shade,dark)
    a.polygon([(cx-12,top+6),(cx+11,top+6),(cx+16,hip+10),
               (cx+19,foot-5),(cx-18,foot-5),(cx-16,hip+10)],robe,dark)
    a.polygon([(cx-4,top+7),(cx+3,top+8),(cx+6,foot-7),
               (cx-8,foot-7)],light)
    a.polygon([(cx-14,top+10),(cx-8,top+10),(cx-11,foot-10),
               (cx-17,foot-7)],mid)
    for fold in (-10,-3,7,12):
        a.line([(cx+fold,hip+5),(cx+fold+(1 if fold>0 else -1),foot-10)],shade,1)
    a.line([(cx-13,hip),(cx+13,hip)],magic,3)
    a.rect((cx-2,hip-1,cx+3,hip+2),dark)
    # Near sleeve reaches to cast or strike; hands and weapon move with it.
    wristx = cx+21+r
    wristy = top+29-r//2+d
    a.polygon([(cx+8,top+10),(cx+16+r//3,top+12-r//3),
               (wristx+5,wristy-5),(wristx,wristy+3),(cx+6,top+26)],robe,dark)
    a.polygon([(cx+12,top+13),(cx+16+r//2,top+17-r//3),
               (wristx,wristy-2),(cx+11,top+23)],mid)
    a.ellipse((wristx-2,wristy,wristx+4,wristy+5),skin,dark)
    a.ellipse((cx-24,top+32+d,cx-19,top+37+d),skin,dark)
    # Face, independently bobbing hair and headpiece.
    a.ellipse((cx-10,34+b,cx+9,58+b),skin,dark)
    a.polygon([(cx-11,46+b),(cx-11,38+b),(cx-6,32+b),
               (cx+2,31+b),(cx+10,38+b),(cx+10,47+b),
               (cx+5,43+b),(cx-3,42+b)],hair,dark)
    a.polygon([(cx-9,45+b),(cx-14,51+b),(cx-12,63+b),(cx-8,58+b)],hair)
    a.rect((cx+3,48+b,cx+5,49+b),dark)
    a.line([(cx-3,53+b),(cx+3,53+b)],mix(skin,dark,.30),1)
    if name in ("XuanTianWang","DiYi","ZhenShen"):
        a.polygon([(cx-8,33+b),(cx-6,27+b),(cx-1,30+b),
                   (cx+3,25+b),(cx+8,33+b)],magic,dark)
    elif name in ("TianYi","Chuanfa","ShanFan"):
        a.line([(cx-8,38+b),(cx+9,38+b)],magic,2)
        a.ellipse((cx+7,35+b,cx+11,39+b),magic)
    elif name in ("MoRuBin","XuKe","LiPing"):
        a.polygon([(cx-12,40+b),(cx-11,32+b),(cx+10,32+b),
                   (cx+12,40+b)],dark,magic)
    else:
        a.line([(cx-5,35+b),(cx+6,33+b)],mid,1)

    species = NAMED_BEAST.get(name,name)
    if species in QUAD or species in OTHER:
        kind = QUAD[species][-1] if species in QUAD else OTHER[species][0]
        if kind in ("deer","dragon"):
            for sx in (-6,7):
                a.line([(cx+sx,34+b),(cx+sx-2,25+b+d),(cx+sx-5,20+b+d)],magic,2)
                a.line([(cx+sx-2,27+b+d),(cx+sx+3,24+b+d)],magic,1)
        elif kind in ("cat","dog","tapir"):
            a.polygon([(cx-10,41+b),(cx-14,30+b+d),(cx-4,36+b)],robe,dark)
            a.polygon([(cx+10,41+b),(cx+14,30+b+d),(cx+4,36+b)],robe,dark)
            a.line([(cx+17,91+b),(cx+25,88+b+w//2),(cx+27,77+b+w//2)],robe,3)
        elif kind == "bird":
            a.polygon([(cx-16,75+b),(cx-30,55+b+w),(cx-29,88+b),
                       (cx-17,93+b)],shade,dark)
            a.polygon([(cx+16,75+b),(cx+31,55+b+w),(cx+30,88+b),
                       (cx+17,93+b)],robe,dark)
            for dx in (-24,24):
                a.line([(cx+dx,67+b+w//2),(cx+dx,83+b+w//2)],magic,1)
        elif kind == "whale":
            a.polygon([(cx+11,55+b),(cx+17,49+b+d),(cx+14,61+b)],magic)
            a.line([(cx+18,86+b),(cx+26,90+b+w//2)],robe,3)
        elif kind == "turtle":
            a.ellipse((cx-13,top+5,cx+12,hip+2),shade,magic)
    if weapon in ("sword","dagger","needle","staff"):
        length = 24 if weapon == "sword" else (13 if weapon == "dagger" else 19)
        if weapon == "needle": length = 16
        a.line([(wristx+3,wristy+1),(wristx+7+r//3,wristy-length-r//4)],
               magic,3 if weapon == "staff" else 2)
        if weapon == "sword":
            a.line([(wristx,wristy-3),(wristx+8,wristy-3)],dark,2)
    elif weapon == "scroll":
        a.polygon([(wristx,wristy-5),(wristx+13,wristy-8),
                   (wristx+14,wristy+2),(wristx+1,wristy+5)],light,dark)
        for iy in (-2,1):
            a.line([(wristx+3,wristy+iy),(wristx+10,wristy+iy-2)],dark,1)
    elif weapon == "machine":
        a.rect((wristx,wristy-7,wristx+12,wristy+3),dark)
        a.rect((wristx+7,wristy-3,wristx+16,wristy),magic)
    elif weapon == "talisman":
        a.polygon([(wristx,wristy-8),(wristx+8,wristy-8),
                   (wristx+7,wristy+7),(wristx+1,wristy+8)],magic,dark)
        a.line([(wristx+3,wristy-3),(wristx+6,wristy+2)],dark,1)
    else:
        a.ellipse((wristx-2,wristy-10,wristx+9,wristy+1),magic,dark)
    if action == "attack":
        magic_burst(a,min(112,wristx+16),max(13,wristy-13),magic,p["spell"],action,index)
    elif action == "hit" and index in (1,2):
        magic_burst(a,cx+16,top-5,magic,p["spell"],action,index)
    flecks(a,(cx-13,top+9,cx+12,foot-8),robe,mid,13)
    return a.image


def quad_pose(action, index):
    if action == "idle":
        return dict(bob=(0,-1,-2,-1)[index], rear=(0,1,0,-1)[index],
                    front=(0,0,-1,0)[index], lean=0, tail=(0,-2,0,2)[index],
                    jaw=0, ear=(0,-1,0,1)[index], spell=0)
    if action == "walk":
        return dict(bob=(0,-2,0,-2)[index], rear=(-6,0,6,0)[index],
                    front=(6,0,-6,0)[index], lean=(0,2,1,-1)[index],
                    tail=(-3,0,3,0)[index], jaw=0, ear=0, spell=0)
    if action == "attack":
        return dict(bob=(1,0,-2,-4,-2,0)[index], rear=(0,-2,-5,-6,-3,0)[index],
                    front=(0,1,5,11,7,1)[index], lean=(0,1,4,10,6,1)[index],
                    tail=(0,-3,-7,-9,-4,0)[index], jaw=(0,1,3,6,5,1)[index],
                    ear=(-1,-2,-4,-5,-2,0)[index], spell=(0,0,1,2,1,0)[index])
    if action == "hit":
        return dict(bob=(0,2,4,2)[index], rear=(0,-1,-3,-1)[index],
                    front=(0,-3,-7,-2)[index], lean=(0,-5,-9,-3)[index],
                    tail=(0,4,8,3)[index], jaw=(0,2,3,1)[index],
                    ear=(0,3,5,1)[index], spell=(-1,-2,-1,0)[index])
    return dict(bob=(0,3,8,12)[index], rear=(0,-3,-8,-12)[index],
                front=(0,-4,-10,-15)[index], lean=(0,-8,-15,-22)[index],
                tail=(0,4,9,13)[index], jaw=(0,2,4,2)[index],
                ear=(0,2,6,8)[index], spell=(-1,-1,-1,-1)[index])


def draw_quad(name, action, index):
    main, highlight, shade, outline, magic, kind = map(
        lambda x: x if x in ("cat","dog","deer","tapir") else color(x), QUAD[name])
    mid_dark = mix(main,outline,.46)
    mid_light = mix(main,highlight,.56)
    warm = mix(magic,main,.35)
    p = quad_pose(action,index)
    a = Canvas()
    b, lean, rear, front, tail, jaw, ear = (
        p[k] for k in ("bob","lean","rear","front","tail","jaw","ear"))
    # Falling is a dedicated low-to-ground pose, drawn from new coordinates.
    if action == "death" and index >= 2:
        a.polygon([(20,99),(30,93),(53,91),(77,96),(101,103),(97,109),(27,109)],main,outline)
        a.polygon([(34,95),(55,93),(70,96),(77,100),(42,101)],highlight)
        a.polygon([(38,100),(52,97),(69,99),(73,104),(49,104)],shade)
        a.polygon([(78,97),(91,93),(105,99),(110,104),(102,109),(80,106)],main,outline)
        a.line([(26,97),(16,105),(19,109)],shade,5)
        a.line([(41,105),(38,110),(45,110)],shade,4)
        a.line([(77,105),(76,110),(85,110)],shade,4)
        for x in (39,51,64,76):
            a.line([(x,94),(x+3,100),(x+1,102)],mid_dark,2)
        a.polygon([(88,95),(96,96),(101,101),(88,101)],highlight)
        a.rect((96,98,98,99),outline)
        a.line([(88,101),(96,101)],outline,1)
        return a.image

    # Curved tail: dark outer line, inner highlight, bands or flame tip.
    tail_curve = [(34,82+b),(23,75+b+tail),(18,61+b+tail),(23,52+b+tail)]
    a.line(tail_curve,outline,12)
    a.line(tail_curve,main,9)
    a.line([(34,81+b),(24,73+b+tail),(20,61+b+tail)],highlight,3)
    if kind == "cat":
        a.line([(20,60+b+tail),(25,58+b+tail)],shade,4)
        a.line([(22,69+b+tail),(27,68+b+tail)],shade,3)
    if kind == "deer":
        a.polygon([(21,52+b+tail),(17,46+b+tail),(25,50+b+tail)],magic,outline)

    # Back feet and hips; their joint positions change independently.
    a.polygon([(39,76+b),(53,76+b),(51+rear//2,93+b),(44+rear,105),
               (33+rear,105),(35+rear//2,93+b)],shade,outline)
    a.polygon([(64,76+b),(75,76+b),(73-front//3,94+b),(70-front,105),
               (60-front,105),(61,91+b)],shade,outline)
    a.polygon([(39+rear,100),(48+rear,100),(50+rear,106),(37+rear,106)],main,outline)
    a.polygon([(62-front,100),(72-front,100),(75-front,106),(60-front,106)],main,outline)

    # Fur body, shoulder ridge and chest are directly redrawn for each pose.
    a.polygon([(33,78+b),(35,65+b),(43,57+b),(57,54+b),(73,56+b),
               (84,63+b),(88,77+b),(82,88+b),(62,91+b),(47,87+b)],main,outline)
    a.polygon([(38,70+b),(44,61+b),(59,57+b),(73,59+b),(79,65+b),
               (68,69+b),(54,67+b)],highlight)
    a.polygon([(39,73+b),(44,68+b),(51,70+b),(47,77+b),(39,79+b)],mid_light)
    a.polygon([(57,62+b),(65,59+b),(74,61+b),(72,65+b),(62,66+b)],mid_light)
    a.polygon([(42,84+b),(51,88+b),(70,89+b),(79,83+b),(74,89+b),(57,93+b)],shade)
    for x in (48,56,64,73):
        a.polygon([(x,57+b),(x+3,54+b),(x+4,60+b)],shade)
    a.polygon([(76,65+b),(85,62+b),(91,75+b),(85,90+b),
               (77,85+b)],highlight,outline)
    a.polygon([(80,71+b),(90,70+b),(93,82+b),(88,88+b),(83,83+b)],main)
    a.polygon([(78,78+b),(84,76+b),(88,81+b),(87,89+b),(82,88+b)],mid_light)
    for x in (47,54,62,70):
        a.line([(x,88+b),(x+2,91+b)],mid_dark,1)

    # Near legs and paws.  The attack reaches forward rather than sliding body art.
    for x,step in ((47,rear),(82,front)):
        a.polygon([(x-6,78+b),(x+4,78+b),(x+7,85+b),(x+2+step,99),
                   (x+10+step,102),(x+10+step,106),(x-6+step,106),
                   (x-7+step,100),(x-5,88+b)],main,outline)
        a.polygon([(x-5,80+b),(x+2,81+b),(x+1+step,99),(x-5+step,100)],highlight)
        a.polygon([(x+3,83+b),(x+6,85+b),(x+3+step,99),(x+1+step,99)],shade)
        a.line([(x-4+step,101),(x+6+step,101)],mid_dark,1)
        for claw in (-2,2,6):
            a.rect((x+step+claw,104,x+step+claw+1,107),outline)

    # Neck, ears, cheeks and muzzle are distinct components with their own motion.
    hx = 78+lean
    hy = 58+b
    a.polygon([(76,69+b),(82,58+b),(88,52+b),(95,50+b),
               (102,54+b),(105,62+b),(99,75+b),(88,77+b)],main,outline)
    a.polygon([(80,67+b),(87,55+b),(96,53+b),(100,57+b),
               (91,61+b),(85,73+b)],highlight)
    a.polygon([(82,70+b),(89,64+b),(96,66+b),(91,75+b),(85,75+b)],mid_light)
    a.polygon([(hx-4,55+b),(hx-6,43+b+ear),(hx+3,50+b)],main,outline)
    a.polygon([(hx+8,53+b),(hx+14,42+b+ear),(hx+15,58+b)],main,outline)
    a.polygon([(hx-3,51+b),(hx-4,46+b+ear),(hx+1,50+b)],magic)
    a.polygon([(hx+10,50+b),(hx+13,46+b+ear),(hx+13,53+b)],magic)
    a.polygon([(hx+4,66+b),(hx+18+jaw//2,65+b+jaw//3),
               (hx+24+jaw//2,70+b+jaw//2),(hx+20+jaw//2,76+b+jaw//2),
               (hx+7,75+b)],highlight,outline)
    a.polygon([(hx+11,72+b+jaw//2),(hx+21+jaw//2,75+b+jaw//2),
               (hx+17,78+b+jaw),(hx+8,77+b)],main,outline)
    a.line([(hx+8,74+b),(hx+16,74+b+jaw//2)],mid_dark,1)
    if jaw:
        a.polygon([(hx+14,75+b),(hx+17,80+b+jaw),(hx+20,75+b)],magic)
    a.polygon([(hx+18+jaw//2,68+b+jaw//3),(hx+22+jaw//2,70+b+jaw//3),
               (hx+20+jaw//2,72+b+jaw//3)],outline)
    a.rect((hx+5,60+b,hx+9,61+b),outline)
    a.rect((hx+7,60+b,hx+8,60+b),magic)
    a.line([(hx+19,74+b),(hx+26,73+b)],outline,1)
    a.line([(hx+18,76+b),(hx+26,79+b)],outline,1)

    # Species details are authored into each pose, not palette-only variants.
    if kind == "cat":
        for x in (44,55,68):
            a.polygon([(x,61+b),(x+2,67+b),(x+6,69+b),(x+4,72+b)],mid_dark)
        for x in (48,62,75):
            a.polygon([(x,78+b),(x+3,84+b),(x,89+b),(x-2,84+b)],mid_dark)
        a.line([(hx-1,57+b),(hx+1,63+b),(hx-2,67+b)],mid_dark,2)
        a.line([(hx+12,57+b),(hx+10,63+b),(hx+13,66+b)],mid_dark,2)
        a.line([(hx+2,53+b),(hx+7,56+b)],mid_dark,2)
        for x in (51,58,67,74):
            a.line([(x,73+b),(x+2,75+b)],shade,1)
        a.line([(49+rear//2,96),(48+rear,102)],mid_dark,2)
        a.line([(84+front//2,96),(82+front,102)],mid_dark,2)
    elif kind == "dog":
        a.polygon([(hx-5,52+b),(hx-10,60+b),(hx-6,70+b),(hx,59+b)],shade,outline)
        a.polygon([(hx+14,70+b),(hx+22,72+b+jaw//2),(hx+18,75+b+jaw//2)],shade)
        if name == "changliao":
            a.polygon([(hx+16,73+b),(hx+18,80+b+jaw),(hx+20,74+b)],magic)
            a.polygon([(hx+21,73+b),(hx+23,79+b+jaw),(hx+24,73+b)],magic)
    elif kind == "deer":
        for sx in (-4,10):
            a.line([(hx+sx,49+b),(hx+sx-2,37+b+ear),(hx+sx-7,30+b+ear)],magic,3)
            a.line([(hx+sx-2,38+b+ear),(hx+sx+5,33+b+ear)],magic,2)
        for x in (49,62,75):
            a.ellipse((x,68+b,x+3,72+b),magic)
        if name == "tianshou":
            a.ellipse((hx+2,54+b,hx+10,62+b),magic,outline)
    else:
        a.line([(hx+18,72+b),(hx+26,82+b+jaw),(hx+26,87+b+jaw)],main,5)
        a.line([(hx+18,72+b),(hx+25,81+b+jaw)],highlight,2)
    if p["spell"] > 0:
        sx = min(114,hx+30+p["spell"]*2)
        sy = 64+b
        a.ellipse((sx-3,sy-3,sx+3,sy+3),magic)
        a.line([(sx-8,sy),(sx-5,sy)],magic,2)
        a.line([(sx+5,sy),(sx+8,sy)],magic,2)
    if action == "hit" and index in (1,2):
        a.line([(hx+23,49+b),(hx+29,43+b),(hx+26,53+b)],magic,2)
    return a.image


if __name__ == "__main__":
    from pathlib import Path
    out = Path(__file__).with_name("pixel_preview")
    out.mkdir(exist_ok=True)
    for action,count in {"idle":4,"attack":6,"walk":4,"hit":4,"death":4}.items():
        for i in range(count):
            draw_quad("baihu",action,i).save(out/f"baihu_{action}_{i}.png")
