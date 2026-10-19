"""User-requested, preview-only pixel grid conversion; never installs world frames."""
from pathlib import Path
import json
import hashlib
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
SOURCE = Path(r"C:\Users\Arthur.lu\.codex\generated_images\01a11e44-419a-7201-8f89-c1458794a997\exec-dd703b56-f6b3-432d-b0a3-ba884f8098e3.png")
OUTPUT = ROOT / "output" / "pixel-grid-preview"
OUTPUT.mkdir(parents=True, exist_ok=True)
image = Image.open(SOURCE).convert("RGBA")
# The user authorized this conversion demonstration, not production conversion.
alpha = image.getchannel("A")
mask = alpha.point(lambda value: 255 if value >= 200 else 0)
bounds = mask.getbbox()
if not bounds:
    raise ValueError("The candidate contains no solid foreground")
body = image.copy()
body.putalpha(mask)
body = body.crop(bounds)
canvas_size = (192, 192)
samples = []
for height in (64, 96, 128):
    width = round(body.width * height / body.height)
    # NEAREST retains sampled original RGB; no palette quantization is applied.
    converted = body.resize((width, height), Image.Resampling.NEAREST)
    frame = Image.new("RGBA", canvas_size, (0, 0, 0, 0))
    frame.alpha_composite(converted, ((192 - width) // 2, 164 - height))
    destination = OUTPUT / f"qingluan-grid-{height}.png"
    frame.save(destination)
    samples.append((height, frame, destination))

font_path = r"C:\Windows\Fonts\msyh.ttc"
font = ImageFont.truetype(font_path, 24)
small_font = ImageFont.truetype(font_path, 18)
title_font = ImageFont.truetype(font_path, 30)
sheet = Image.new("RGB", (1260, 806), "#172125")
draw = ImageDraw.Draw(sheet)
draw.text((26, 16), "青鸾像素网格转换预览 · 非实机截图", font=title_font, fill="#EDF4F5")
draw.text((26, 60), "每个样图均为 192×192 透明 PNG；主体高度不同。放大显示采用最近邻。", font=small_font, fill="#B9C8CB")
draw.text((26, 88), "处理：透明度阈值 200 → 单帧主体边界 → 像素网格转换；未限制为 22 色。", font=small_font, fill="#B9C8CB")
for column, (height, frame, destination) in enumerate(samples):
    left = 26 + column * 412
    draw.text((left, 132), f"主体高度 {height} px", font=font, fill="#EDF4F5")
    for y in range(176, 560, 16):
        for x in range(left, left + 384, 16):
            fill = "#425348" if ((x - left) // 16 + (y - 176) // 16) % 2 else "#4A5E4E"
            draw.rectangle((x, y, x + 15, y + 15), fill=fill)
    sheet.paste(frame.resize((384, 384), Image.Resampling.NEAREST), (left, 176), frame.resize((384, 384), Image.Resampling.NEAREST))
    draw.text((left, 572), "上方：2 倍放大 / 下方：原始像素大小", font=small_font, fill="#B9C8CB")
    draw.rectangle((left, 602, left + 383, 795), fill="#425348")
    # The native-size sprite shows readability without any display enlargement.
    sheet.paste(frame, (left + 96, 602), frame)
sheet_path = OUTPUT / "comparison.png"
sheet.save(sheet_path)
record = {
    "purpose": "preview-only; no production world frames replaced",
    "source": str(SOURCE),
    "sourceSha256": hashlib.sha256(SOURCE.read_bytes()).hexdigest(),
    "sourceSize": list(image.size),
    "alphaThreshold": 200,
    "foregroundBounds": list(bounds),
    "resampling": "nearest-neighbor",
    "paletteQuantization": False,
    "canvas": list(canvas_size),
    "samples": [{"bodyHeight": height, "file": str(path), "sha256": hashlib.sha256(path.read_bytes()).hexdigest()} for height, _, path in samples],
    "limitations": "Threshold cleanup and resampling alter source pixels. This preview does not validate anatomy, action continuity, foot anchor or in-game size.",
}
(OUTPUT / "conversion-record.json").write_text(json.dumps(record, ensure_ascii=False, indent=2), encoding="utf-8")
print(json.dumps({"comparison": str(sheet_path), "samples": record["samples"]}, ensure_ascii=False))
