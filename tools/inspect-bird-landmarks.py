"""Read-only color components for manual per-frame landmark review."""
from PIL import Image
from pathlib import Path
import json

root = Path(__file__).resolve().parents[1]
result = {}
standard = Image.open(root / "output/actor-redraw/frames/mclsl_beast_qingluan/main/walk_0.png").convert("RGBA")
offset = (standard.width - 128) // 2
template = [[standard.getpixel((79 + offset + x, 61 + offset + y)) for x in range(6)] for y in range(7)]
for index in range(14):
    path = root / f"output/actor-redraw/frames/mclsl_beast_qingluan/main/walk_{index}.png"
    image = Image.open(path).convert("RGBA")
    pixels = set()
    for y in range(40 + offset, 85 + offset):
        for x in range(55 + offset, 110 + offset):
            r, g, b, a = image.getpixel((x, y))
            if a >= 128 and r > 145 and g > 90 and b < 110:
                pixels.add((x, y))
    components = []
    while pixels:
        point = pixels.pop()
        component, queue = [point], [point]
        while queue:
            x, y = queue.pop()
            for dx in (-1, 0, 1):
                for dy in (-1, 0, 1):
                    neighbor = (x + dx, y + dy)
                    if neighbor in pixels:
                        pixels.remove(neighbor)
                        component.append(neighbor)
                        queue.append(neighbor)
        xs, ys = zip(*component)
        bounds = [min(xs), min(ys), max(xs) + 1, max(ys) + 1]
        if len(component) >= 5 and bounds[2] - bounds[0] >= 4 and bounds[3] - bounds[1] <= 10:
            components.append({"bounds": bounds, "pixels": len(component)})
    samples = image.load()
    best = (float("inf"), None)
    for top in range(35 + offset, 76 + offset):
        for left in range(55 + offset, 96 + offset):
            error = 0
            for y in range(7):
                for x in range(6):
                    sample, reference = samples[left + x, top + y], template[y][x]
                    error += (sample[3] - reference[3]) ** 2
                    for channel in range(3):
                        error += (sample[channel] * sample[3] / 255 - reference[channel] * reference[3] / 255) ** 2
            if error < best[0]:
                best = (error, [left, top])
    result[str(index)] = {"goldComponents": components, "headTemplateTopLeft": best[1], "headTemplateError": round(best[0])}
print(json.dumps(result))
