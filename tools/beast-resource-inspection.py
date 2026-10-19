"""Read-only PNG inspection shared by the resource gate and fixture tests."""
import hashlib
from PIL import Image


def inspect_png(path, expected, frames=None, frame_size=192, columns=4):
    with Image.open(path) as source:
        source.load()  # Decode every row; an IHDR alone is not an image.
        if source.format != 'PNG' or source.size != expected or 'A' not in source.getbands():
            raise ValueError('Expected a PNG of the declared dimensions with an alpha channel')
        image = source.convert('RGBA')
        alpha = image.getchannel('A')
        low, high = alpha.getextrema()
        if high == 0 or low > 0:
            raise ValueError('Image must have visible content and a transparent background')
        if frames is None:
            bounds = alpha.getbbox()
            occupied = sum(alpha.histogram()[1:])
            if occupied < 32 or bounds[2] - bounds[0] < 8 or bounds[3] - bounds[1] < 8:
                raise ValueError('Inventory icon content is too small to be a meaningful asset')
        if frames is not None:
            rows = (frames + columns - 1) // columns
            if expected != (columns * frame_size, rows * frame_size):
                raise ValueError('Atlas geometry differs from its declared frame grid')
            visible = 0
            for index in range(frames):
                x, y = index % columns * frame_size, index // columns * frame_size
                if alpha.crop((x, y, x + frame_size, y + frame_size)).getbbox():
                    visible += 1
            if visible < 2:
                raise ValueError('An animation atlas must contain multiple visible frames')
        # Invisible RGB bytes do not make two otherwise identical assets independent.
        pixels = bytearray(image.tobytes())
        for index in range(0, len(pixels), 4):
            if pixels[index + 3] == 0:
                pixels[index:index + 3] = b'\0\0\0'
        return hashlib.sha256(bytes(pixels)).hexdigest()
