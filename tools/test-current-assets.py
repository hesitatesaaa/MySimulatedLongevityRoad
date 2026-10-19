"""Synthetic alpha-edge and full-canvas regression checks; does not review character art."""
import importlib.util
from pathlib import Path
from PIL import Image, ImageDraw

spec = importlib.util.spec_from_file_location('resize_actors', Path(__file__).with_name('resize-actors.py'))
resize = importlib.util.module_from_spec(spec)
spec.loader.exec_module(resize)
image = Image.new('RGBA', (40, 24), (255, 0, 255, 0))
ImageDraw.Draw(image).rectangle((8, 4, 31, 19), fill=(20, 180, 240, 255))
result = resize.resize(image, (20, 12))
assert result.mode == 'RGBA' and result.size == (20, 12)
assert result.getpixel((0, 0))[3] == 0
assert result.getchannel('A').getbbox() is not None
for red, green, blue, alpha in result.getdata():
    if alpha >= 32:
        assert abs(red - 20) <= 8 and abs(green - 180) <= 8 and abs(blue - 240) <= 8, 'Transparent RGB contaminated a visible edge'
edge = Image.new('RGBA', (40, 24), (80, 160, 240, 255))
scaled = resize.resize(edge, (20, 12))
assert scaled.getchannel('A').getbbox() == (0, 0, 20, 12), 'Full-canvas edge was cropped'
assert image.size == (40, 24), 'Input image was mutated'
print('PASS: premultiplied alpha edges, full-canvas scaling, RGBA output and unchanged input')
