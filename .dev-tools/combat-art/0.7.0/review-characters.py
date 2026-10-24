from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
SOURCE = ROOT / "GameResources/actors/Xuanhuang"
OUTPUT = Path(__file__).with_name("review") / "characters.png"
NAMES = ("male_1", "male_1_front", "male_1_back",
         "female_1", "female_1_front", "female_1_back")

sheet = Image.new("RGB", (6 * 112, 2 * 56), (225, 225, 215))
for index, name in enumerate(NAMES):
    image = Image.open(SOURCE / (name + ".png")).convert("RGBA")
    background = Image.new("RGBA", image.size, (225, 225, 215, 255))
    background.alpha_composite(image)
    sheet.paste(background.convert("RGB"), (index * 112, 0))
    for row in range(2):
        for column in range(4):
            cell = image.crop((column * 28, row * 28, (column + 1) * 28, (row + 1) * 28))
            print(name, row, column, cell.getchannel("A").getbbox())

OUTPUT.parent.mkdir(parents=True, exist_ok=True)
sheet.resize((6 * 224, 2 * 112), Image.Resampling.NEAREST).save(OUTPUT)
print(OUTPUT)
