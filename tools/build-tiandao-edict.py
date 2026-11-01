"""Build the native 32px shared Heaven Edict icon with an exact pixel palette."""
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
PALETTE = {
    'outline': '#30251f', 'gold_dark': '#72502b', 'gold': '#b58739',
    'gold_light': '#e6bf62', 'gold_white': '#fff0ad', 'jade_dark': '#143b3b',
    'jade_shadow': '#225956', 'jade': '#3d8273', 'jade_light': '#72b49a',
    'jade_white': '#b3dec1', 'rune_shadow': '#92713b', 'rune': '#f5d87b',
    'depth': '#1a302e', 'depth_light': '#427064',
}


def build():
    image = Image.new('RGBA', (32, 32))
    d = ImageDraw.Draw(image)
    def rect(box, color): d.rectangle(box, fill=PALETTE[color])
    def line(points, color): d.line(points, fill=PALETTE[color], width=1)
    # Discrete star: no blur, alpha fringe or button background.
    rect((15, 3, 16, 8), 'gold_light')
    rect((13, 5, 18, 6), 'gold_light')
    rect((15, 5, 16, 6), 'gold_white')
    # Stepped gold handle sitting above the jade seal.
    rect((12, 9, 19, 10), 'outline')
    rect((10, 11, 21, 13), 'outline')
    rect((13, 9, 18, 10), 'gold')
    rect((11, 11, 20, 12), 'gold_light')
    rect((13, 10, 14, 11), 'gold_white')
    rect((17, 10, 18, 11), 'gold_dark')
    # Front-facing face, clipped corners, and bottom thickness.
    d.polygon([(7, 13), (24, 13), (26, 15), (26, 25), (24, 28),
               (7, 28), (5, 25), (5, 15)], fill=PALETTE['outline'])
    rect((7, 25, 24, 27), 'depth')
    rect((8, 26, 23, 26), 'depth_light')
    d.polygon([(7, 14), (24, 14), (25, 15), (25, 23), (24, 25),
               (7, 25), (6, 23), (6, 15)], fill=PALETTE['gold_dark'])
    rect((7, 14, 24, 15), 'gold_light')
    rect((6, 16, 7, 23), 'gold')
    rect((24, 16, 25, 23), 'gold')
    rect((8, 24, 23, 25), 'gold')
    rect((8, 16, 23, 23), 'jade_dark')
    rect((9, 16, 22, 22), 'jade')
    rect((10, 16, 21, 17), 'jade_light')
    line([(9, 18), (9, 21), (10, 22)], 'jade_shadow')
    rect((10, 22, 22, 23), 'jade_shadow')
    rect((22, 18, 23, 22), 'jade_shadow')
    # Large symmetrical abstract decree rune, deliberately not a text glyph.
    line([(12, 18), (19, 18)], 'rune_shadow')
    line([(15, 17), (15, 22)], 'rune_shadow')
    line([(16, 17), (16, 22)], 'rune_shadow')
    line([(12, 17), (19, 17)], 'rune')
    rect((15, 17, 16, 21), 'rune')
    line([(13, 20), (12, 21), (11, 21)], 'rune')
    line([(18, 20), (19, 21), (20, 21)], 'rune')
    rect((14, 22, 17, 22), 'rune')
    rect((7, 15, 8, 16), 'gold_white')
    rect((23, 23, 24, 24), 'gold_light')
    rect((10, 18, 10, 18), 'jade_white')
    return image


if __name__ == '__main__':
    icon = build()
    target = ROOT / 'GameResources/trait/MclslTiandaoEdict.png'
    target.parent.mkdir(parents=True, exist_ok=True)
    icon.save(target)
    preview = ROOT / 'build/heaven-grants/art'
    preview.mkdir(parents=True, exist_ok=True)
    icon.resize((256, 256), Image.Resampling.NEAREST).save(preview / 'edict-8x.png')
    icon.resize((16, 16), Image.Resampling.NEAREST).save(preview / 'edict-16.png')
    assert icon.mode == 'RGBA' and icon.size == (32, 32)
    assert set(icon.getchannel('A').getdata()) == {0, 255}
    colors = len({c for c in icon.getdata() if c[3]})
    assert 12 <= colors <= 16, colors
    print(f'RGBA 32x32; opaque palette={colors}; transparent border; {target}')
