"""Generate deterministic point-filtered spell sheets without external image libraries."""

from __future__ import annotations

import math
import struct
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "GameResources"
EFFECTS = ROOT / "effects" / "Spells"
ICONS = ROOT / "ui" / "Spells"
EFFECTS.mkdir(parents=True, exist_ok=True)
ICONS.mkdir(parents=True, exist_ok=True)

PALETTES = [
    (236, 195, 88), (92, 211, 127), (82, 192, 230), (247, 100, 62),
    (183, 151, 93), (111, 210, 188), (162, 151, 244), (165, 103, 205),
    (254, 215, 119), (166, 211, 147), (157, 145, 235), (239, 223, 157),
    (116, 217, 244), (103, 224, 163), (100, 215, 203), (100, 195, 112),
    (178, 160, 249), (95, 191, 239), (249, 118, 67), (197, 166, 114),
    (175, 110, 223), (134, 173, 241), (159, 151, 231), (121, 212, 137),
    (149, 230, 157), (255, 219, 151),
]


def png(path: Path, width: int, height: int, pixels: bytearray) -> None:
    rows = b"".join(b"\0" + pixels[y * width * 4:(y + 1) * width * 4] for y in range(height))

    def chunk(kind: bytes, data: bytes) -> bytes:
        return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xffffffff)

    path.write_bytes(
        b"\x89PNG\r\n\x1a\n"
        + chunk(b"IHDR", struct.pack(">IIBBBBB", width, height, 8, 6, 0, 0, 0))
        + chunk(b"IDAT", zlib.compress(rows, 9))
        + chunk(b"IEND", b"")
    )


def read_rgba(path: Path) -> bytearray:
    """Read this asset set's non-interlaced 8-bit RGBA PNGs (filter type 0)."""
    data = path.read_bytes()
    width, height, depth, color_type, compression, filtering, interlace = struct.unpack(">IIBBBBB", data[16:29])
    if depth != 8 or color_type != 6 or compression != 0 or filtering != 0 or interlace != 0:
        raise ValueError(f"Unsupported PNG format: {path}")
    chunks: list[bytes] = []
    offset = 8
    while offset < len(data):
        length = struct.unpack(">I", data[offset:offset + 4])[0]
        kind = data[offset + 4:offset + 8]
        if kind == b"IDAT": chunks.append(data[offset + 8:offset + 8 + length])
        offset += length + 12
    raw = zlib.decompress(b"".join(chunks))
    stride = width * 4
    if len(raw) != height * (stride + 1) or any(raw[y * (stride + 1)] != 0 for y in range(height)):
        raise ValueError(f"Unsupported PNG scanline filter: {path}")
    return bytearray().join(raw[y * (stride + 1) + 1:(y + 1) * (stride + 1)] for y in range(height))


def layer_old_spell_texture(foreground: bytearray, source_number: int, opacity: float = 0.48) -> bytearray:
    """Blend in subdued color clusters from a shipped spell icon for visual continuity."""
    background = read_rgba(ICONS / f"S{source_number:03}.png")
    result = bytearray(len(foreground))
    for pos in range(0, len(result), 4):
        fr, fg, fb, fa = foreground[pos:pos + 4]
        br, bg, bb, ba = background[pos:pos + 4]
        ba = round(ba * opacity)
        fa_ratio, ba_ratio = fa / 255.0, ba / 255.0
        out_alpha = fa_ratio + ba_ratio * (1.0 - fa_ratio)
        if out_alpha <= 0:
            continue
        for channel, (front, back) in enumerate(((fr, br), (fg, bg), (fb, bb))):
            result[pos + channel] = round((front * fa_ratio + back * ba_ratio * (1.0 - fa_ratio)) / out_alpha)
        result[pos + 3] = round(out_alpha * 255)
    return result


def shade(rgb: tuple[int, int, int], scale: float, alpha: int = 255) -> tuple[int, int, int, int]:
    return tuple(min(255, round(v * scale)) for v in rgb) + (alpha,)


def paint(buf: bytearray, width: int, height: int, x: int, y: int, color: tuple[int, int, int, int]) -> None:
    if 0 <= x < width and 0 <= y < height:
        pos = (y * width + x) * 4
        buf[pos:pos + 4] = bytes(color)


def line(buf: bytearray, width: int, height: int, x0: int, y0: int, x1: int, y1: int,
         color: tuple[int, int, int, int]) -> None:
    dx, dy = abs(x1 - x0), -abs(y1 - y0)
    sx, sy = (1 if x0 < x1 else -1), (1 if y0 < y1 else -1)
    err = dx + dy
    while True:
        paint(buf, width, height, x0, y0, color)
        if x0 == x1 and y0 == y1:
            return
        e2 = 2 * err
        if e2 >= dy:
            err += dy
            x0 += sx
        if e2 <= dx:
            err += dx
            y0 += sy


def ring(buf: bytearray, width: int, height: int, cx: int, cy: int, radius: int,
         color: tuple[int, int, int, int], phase: float = 0.0, segments: int = 64) -> None:
    for i in range(segments):
        theta = i * math.tau / segments + phase
        paint(buf, width, height, cx + round(math.cos(theta) * radius),
              cy + round(math.sin(theta) * radius), color)


def frame(buf: bytearray, width: int, height: int, xoff: int, number: int, tick: int,
          color: tuple[int, int, int]) -> None:
    cx, cy = xoff + 12, height // 2
    fade = 255 if tick < 6 else 185 if tick == 6 else 100
    base = shade(color, 0.60, fade)
    bright = shade(color, 1.0, fade)
    light = (247, 247, 224, fade)
    radius = (2, 3, 5, 7, 8, 9, 8, 6)[tick]
    phase = (number % 9) * 0.12 + tick * 0.17
    ring(buf, width, height, cx, cy, radius, base, phase)
    if tick >= 2:
        ring(buf, width, height, cx, cy, max(2, radius - 3), bright, -phase, 48)
    rays = 3 + number % 5
    for i in range(rays):
        angle = math.tau * i / rays + phase
        inner = 3 if tick < 2 else radius - 2
        outer = min(11, radius + (2 if tick >= 4 else 1))
        x0, y0 = cx + round(math.cos(angle) * inner), cy + round(math.sin(angle) * inner)
        x1, y1 = cx + round(math.cos(angle) * outer), cy + round(math.sin(angle) * outer)
        line(buf, width, height, x0, y0, x1, y1, bright)
        paint(buf, width, height, x1, y1, light)
    motif = number % 8
    if motif == 0:  # blade
        line(buf, width, height, cx - 3, cy - 4, cx + 4, cy + 4, light)
        line(buf, width, height, cx - 4, cy - 2, cx + 2, cy + 5, bright)
    elif motif == 1:  # leaves
        for dx, dy in ((-3, 0), (3, 0), (0, -3), (0, 3)):
            line(buf, width, height, cx, cy, cx + dx, cy + dy, light)
            paint(buf, width, height, cx + dx, cy + dy, bright)
    elif motif == 2:  # mirror
        for d in range(-2, 3):
            paint(buf, width, height, cx + d, cy - 2, light)
            paint(buf, width, height, cx + d, cy + 2, light)
        line(buf, width, height, cx - 2, cy - 2, cx - 2, cy + 2, bright)
        line(buf, width, height, cx + 2, cy - 2, cx + 2, cy + 2, bright)
    elif motif == 3:  # flame
        line(buf, width, height, cx, cy - 4, cx - 2, cy + 2, light)
        line(buf, width, height, cx - 2, cy + 2, cx + 2, cy + 4, bright)
        line(buf, width, height, cx + 2, cy + 4, cx + 3, cy - 2, light)
    elif motif == 4:  # mountain
        line(buf, width, height, cx - 4, cy + 3, cx, cy - 4, light)
        line(buf, width, height, cx, cy - 4, cx + 4, cy + 3, light)
        line(buf, width, height, cx - 4, cy + 3, cx + 4, cy + 3, bright)
    elif motif == 5:  # wind
        for dy in (-2, 0, 2):
            line(buf, width, height, cx - 4, cy + dy, cx + 3 - dy // 2, cy + dy, light)
    elif motif == 6:  # lightning
        line(buf, width, height, cx + 2, cy - 4, cx - 1, cy, light)
        line(buf, width, height, cx - 1, cy, cx + 2, cy, light)
        line(buf, width, height, cx + 2, cy, cx - 2, cy + 4, bright)
    else:  # seal
        line(buf, width, height, cx, cy - 4, cx + 4, cy, light)
        line(buf, width, height, cx + 4, cy, cx, cy + 4, bright)
        line(buf, width, height, cx, cy + 4, cx - 4, cy, light)
        line(buf, width, height, cx - 4, cy, cx, cy - 4, bright)
    paint(buf, width, height, cx, cy, light)


def polygon(buf: bytearray, size: int, points: list[tuple[int, int]], color: tuple[int, int, int, int]) -> None:
    ymin, ymax = min(y for _, y in points), max(y for _, y in points)
    for y in range(ymin, ymax + 1):
        crossings: list[float] = []
        for i, (x1, y1) in enumerate(points):
            x2, y2 = points[(i + 1) % len(points)]
            if (y1 <= y < y2) or (y2 <= y < y1):
                crossings.append(x1 + (y - y1) * (x2 - x1) / (y2 - y1))
        crossings.sort()
        for i in range(0, len(crossings) - 1, 2):
            for x in range(round(crossings[i]), round(crossings[i + 1]) + 1):
                paint(buf, size, size, x, y, color)


def thick_line(buf: bytearray, size: int, a: tuple[int, int], b: tuple[int, int],
               color: tuple[int, int, int, int], thickness: int = 2) -> None:
    line(buf, size, size, a[0], a[1], b[0], b[1], color)
    for offset in range(1, thickness):
        line(buf, size, size, a[0] + offset, a[1], b[0] + offset, b[1], color)
        line(buf, size, size, a[0], a[1] + offset, b[0], b[1] + offset, color)


def ellipse(buf: bytearray, size: int, cx: int, cy: int, rx: int, ry: int,
            color: tuple[int, int, int, int], fill: bool = True) -> None:
    for y in range(cy - ry, cy + ry + 1):
        for x in range(cx - rx, cx + rx + 1):
            d = ((x - cx) / max(1, rx)) ** 2 + ((y - cy) / max(1, ry)) ** 2
            if (d <= 1.0 if fill else 0.72 <= d <= 1.12):
                paint(buf, size, size, x, y, color)


def icon_art(number: int, color: tuple[int, int, int]) -> bytearray:
    """Paint spell-like energy symbols in the same loose pixel style as S001-S012."""
    size = 64
    buf = bytearray(size * size * 4)
    cx, cy = 32, 33
    shadow = shade(color, 0.34)
    mid = shade(color, 0.75)
    bright = shade(color, 1.0)
    glint = (255, 247, 211, 255)
    gold = (247, 198, 92, 255)
    ice = (202, 246, 255, 255)

    # Soft, irregular light fills the icon area behind the spell. The old set
    # has dense colored glows and fragments, so avoid leaving new symbols bare.
    for y in range(9, 58):
        for x in range(8, 57):
            dx, dy = x - cx, y - cy
            radial = math.exp(-((dx / 23.0) ** 2 + (dy / 18.0) ** 2) * 1.35)
            noise = ((x * 73 + y * 151 + number * 97) ^ (x * y * 11)) % 256 / 255.0
            if noise < radial * 0.66:
                alpha = min(82, 16 + int(radial * 55) + (x + y + number) % 13)
                paint(buf, size, size, x, y, shade(color, 0.64 + noise * 0.35, alpha))

    def slash(points: list[tuple[int, int]], outer: tuple[int, int, int, int],
              inner: tuple[int, int, int, int], width: int = 3) -> None:
        for a, b in zip(points, points[1:]):
            thick_line(buf, size, a, b, shade(color, 0.40, 48), width + 8)
            thick_line(buf, size, a, b, shadow, width + 3)
            thick_line(buf, size, a, b, outer, width + 1)
            if width > 1:
                thick_line(buf, size, a, b, inner, max(2, width - 1))

    def sparkle(x: int, y: int, c: tuple[int, int, int, int] = glint) -> None:
        thick_line(buf, size, (x - 3, y), (x + 3, y), c, 1)
        thick_line(buf, size, (x, y - 3), (x, y + 3), c, 1)
        paint(buf, size, size, x, y, (255, 255, 239, 255))

    if number == 13:  # flying frost needles
        for shift in (0, 10, 20):
            points = [(12 + shift, 47), (20 + shift, 35), (25 + shift, 20),
                      (27 + shift, 31), (24 + shift, 43)]
            slash(points, shadow, ice if shift == 10 else bright, 5)
            thick_line(buf, size, (21 + shift, 39), (24 + shift, 27), glint, 1)
        sparkle(48, 17, ice)
    elif number == 14:  # gathering spiritual dew
        slash([(11, 43), (19, 34), (29, 31), (38, 34), (49, 21)], shadow, bright, 5)
        slash([(13, 50), (23, 42), (33, 43), (45, 35)], mid, glint, 4)
        ellipse(buf, size, 32, 30, 12, 14, shadow)
        ellipse(buf, size, 31, 29, 9, 11, bright)
        ellipse(buf, size, 28, 26, 5, 6, glint)
        sparkle(47, 20, gold)
    elif number == 15:  # cutting gust
        slash([(8, 44), (17, 39), (25, 31), (34, 27), (43, 19), (55, 14)], shadow, bright, 7)
        slash([(10, 51), (22, 45), (31, 39), (39, 32), (54, 26)], mid, glint, 5)
        slash([(14, 25), (24, 20), (35, 17)], shadow, bright, 4)
        sparkle(46, 16)
    elif number == 16:  # entangling vines
        slash([(11, 51), (18, 43), (16, 31), (26, 23), (39, 27), (48, 38), (40, 50)], shadow, bright, 6)
        slash([(18, 53), (29, 44), (31, 34), (40, 24), (53, 19)], mid, glint, 4)
        for pts in ([(20, 35), (14, 28), (14, 23), (22, 27), (25, 32)],
                    [(37, 33), (42, 26), (49, 24), (46, 32), (40, 37)],
                    [(27, 43), (25, 50), (29, 54), (33, 47)]):
            polygon(buf, size, pts, bright)
            thick_line(buf, size, pts[0], pts[2], mid, 1)
        sparkle(12, 19, (199, 255, 162, 255))
    elif number == 17:  # violet armor-breaking lightning
        slash([(14, 13), (32, 22), (24, 32), (47, 38), (27, 48), (41, 55)], shadow, bright, 8)
        slash([(11, 26), (20, 31), (15, 40)], mid, glint, 4)
        slash([(42, 12), (37, 22), (53, 29)], bright, gold, 4)
        sparkle(47, 44, glint)
    elif number == 18:  # returning spring water
        ellipse(buf, size, 32, 33, 18, 15, shadow, False)
        ellipse(buf, size, 32, 33, 13, 11, bright, False)
        slash([(10, 41), (17, 30), (27, 23), (38, 24), (48, 32), (54, 42)], shadow, bright, 6)
        slash([(13, 48), (23, 40), (32, 41), (42, 37), (51, 43)], mid, ice, 4)
        ellipse(buf, size, 31, 31, 9, 10, bright)
        ellipse(buf, size, 28, 27, 5, 5, glint)
        sparkle(20, 20, ice)
    elif number == 19:  # revolving fire wheel
        for radius, c in ((20, shadow), (16, bright), (11, gold)):
            ellipse(buf, size, 32, 34, radius, radius - 2, c, False)
        for pts in ([(26, 43), (21, 34), (25, 27), (29, 31), (31, 21), (38, 31), (43, 26), (41, 39), (35, 46)],
                    [(28, 40), (25, 34), (29, 30), (32, 34), (34, 28), (39, 35), (37, 41), (33, 44)]):
            polygon(buf, size, pts, bright if pts[0][0] == 26 else gold)
        sparkle(32, 31, glint)
    elif number == 20:  # mountain ward
        slash([(8, 49), (18, 30), (25, 37), (33, 15), (40, 31), (46, 23), (57, 49)], shadow, mid, 8)
        slash([(12, 51), (22, 39), (29, 43), (35, 29), (41, 40), (50, 32), (53, 51)], bright, glint, 5)
        ellipse(buf, size, 32, 46, 25, 8, gold, False)
        sparkle(33, 19, (255, 227, 142, 255))
    elif number == 21:  # moon soul snare
        ellipse(buf, size, 32, 33, 22, 21, shadow)
        ellipse(buf, size, 41, 24, 19, 18, (24, 35, 66, 255))
        ellipse(buf, size, 34, 30, 14, 15, bright)
        ellipse(buf, size, 41, 24, 16, 16, (24, 35, 66, 255))
        slash([(11, 43), (19, 49), (29, 47)], mid, glint, 4)
        sparkle(18, 20, gold)
        sparkle(48, 43, ice)
    elif number == 22:  # star sword array
        for a, b in (((15, 44), (43, 16)), ((18, 19), (45, 44)), ((10, 32), (52, 31))):
            slash([a, ((a[0] + b[0]) // 2, (a[1] + b[1]) // 2), b], shadow, bright, 4)
            sparkle(b[0], b[1], gold)
        sparkle(31, 31, glint)
    elif number == 23:  # suppressing domain
        ellipse(buf, size, 32, 35, 24, 19, shadow, False)
        ellipse(buf, size, 32, 35, 18, 13, bright, False)
        slash([(32, 8), (40, 26), (56, 34), (40, 42), (32, 57), (24, 42), (8, 34), (24, 26), (32, 8)], mid, glint, 4)
        ellipse(buf, size, 32, 34, 9, 9, bright)
        sparkle(32, 33, gold)
    elif number == 24:  # five element transformation
        elements = [(gold, (32, 18)), ((100, 211, 128, 255), (44, 28)),
                    ((93, 190, 240, 255), (40, 43)), ((238, 105, 72, 255), (24, 43)),
                    ((157, 132, 225, 255), (20, 28))]
        for c, (x, y) in elements:
            ellipse(buf, size, x, y, 8, 9, shade(c[:3], 0.45))
            ellipse(buf, size, x - 1, y - 1, 5, 6, c)
            ellipse(buf, size, x - 3, y - 4, 2, 2, glint)
        ellipse(buf, size, 32, 34, 7, 7, shadow)
        ellipse(buf, size, 31, 33, 4, 4, gold)
        sparkle(31, 32, glint)
    elif number == 25:  # healing light over allies
        slash([(12, 43), (19, 37), (25, 40), (32, 34), (39, 39), (47, 34), (53, 40)], shadow, bright, 4)
        slash([(16, 48), (24, 44), (32, 46), (40, 43), (49, 46)], mid, glint, 2)
        for x, y in ((19, 24), (32, 17), (45, 24)):
            thick_line(buf, size, (x, y + 6), (x, y - 6), gold, 3)
            thick_line(buf, size, (x - 6, y), (x + 6, y), glint, 2)
        sparkle(32, 34, (224, 255, 220, 255))
    else:  # origin radiance and brief stillness
        for i, angle in enumerate((0, math.pi / 4, math.pi / 2, 3 * math.pi / 4)):
            x = 32 + round(math.cos(angle) * 23)
            y = 33 + round(math.sin(angle) * 23)
            thick_line(buf, size, (32, 33), (x, y), shade(color, 0.52, 170), 3)
        ellipse(buf, size, 32, 33, 17, 17, shadow)
        ellipse(buf, size, 31, 32, 13, 13, gold)
        ellipse(buf, size, 28, 29, 8, 8, (255, 231, 153, 255))
        ellipse(buf, size, 26, 27, 4, 4, glint)
        for x, y in ((15, 18), (49, 19), (17, 47), (47, 46)):
            sparkle(x, y, bright)
    return buf


for number, color in enumerate(PALETTES, 1):
    sheet = bytearray(192 * 24 * 4)
    for tick in range(8):
        frame(sheet, 192, 24, tick * 24, number, tick, color)
    png(EFFECTS / f"S{number:03}.png", 192, 24, sheet)
    if number >= 13:
        texture_sources = {13: 11, 14: 9, 15: 6, 16: 8, 17: 7, 18: 3, 19: 4,
                           20: 5, 21: 12, 22: 1, 23: 12, 24: 10, 25: 2, 26: 9}
        foreground = icon_art(number, color)
        layered = layer_old_spell_texture(foreground, texture_sources[number])
        png(ICONS / f"S{number:03}.png", 64, 64, layered)

insight = bytearray(32 * 32 * 4)
violet = (186, 143, 245, 255)
gold = (244, 213, 129, 255)
for radius in (8, 11):
    ring(insight, 32, 32, 16, 16, radius, violet)
for dx, dy in ((0, -6), (5, -3), (5, 3), (0, 6), (-5, 3), (-5, -3)):
    line(insight, 32, 32, 16, 16, 16 + dx, 16 + dy, gold)
    paint(insight, 32, 32, 16 + dx, 16 + dy, (255, 255, 226, 255))
ring(insight, 32, 32, 16, 16, 2, (255, 255, 226, 255))
png(ROOT / "ui" / "Icons" / "WuXing.png", 32, 32, insight)
