"""Synthetic validation fixtures only; these are not production art."""
import importlib.util
import tempfile
import unittest
from pathlib import Path
from PIL import Image

spec = importlib.util.spec_from_file_location('inspection', Path(__file__).with_name('beast-resource-inspection.py'))
inspection = importlib.util.module_from_spec(spec)
spec.loader.exec_module(inspection)


class ResourceInspectionTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.root = Path(self.temp.name)

    def tearDown(self):
        self.temp.cleanup()

    def save(self, image, name='fixture.png'):
        path = self.root / name
        image.save(path)
        return path

    def test_empty_and_opaque_rejected(self):
        for color in [(0, 0, 0, 0), (255, 0, 0, 255)]:
            with self.assertRaises(ValueError):
                inspection.inspect_png(self.save(Image.new('RGBA', (64, 64), color)), (64, 64))

    def test_dimensions_and_alpha_required(self):
        for image in [Image.new('RGBA', (32, 64)), Image.new('RGB', (64, 64))]:
            with self.assertRaises(ValueError):
                inspection.inspect_png(self.save(image), (64, 64))

    def test_invisible_rgb_does_not_hide_duplicate(self):
        hashes = []
        for color in [(0, 0, 0, 0), (255, 255, 255, 0)]:
            image = Image.new('RGBA', (64, 64), color)
            image.paste((20, 30, 40, 255), (24, 24, 40, 40))
            hashes.append(inspection.inspect_png(self.save(image), (64, 64)))
        self.assertEqual(*hashes)

    def test_tiny_placeholder_rejected(self):
        image = Image.new('RGBA', (64, 64))
        image.putpixel((32, 32), (20, 30, 40, 255))
        with self.assertRaises(ValueError):
            inspection.inspect_png(self.save(image), (64, 64))

    def test_atlas_needs_multiple_frames(self):
        image = Image.new('RGBA', (768, 384))
        image.putpixel((96, 96), (255, 0, 0, 255))
        with self.assertRaises(ValueError):
            inspection.inspect_png(self.save(image), image.size, 8)
        image.putpixel((288, 96), (255, 0, 0, 255))
        self.assertTrue(inspection.inspect_png(self.save(image), image.size, 8))
        with self.assertRaises(ValueError):
            inspection.inspect_png(self.save(image), image.size, 16)

    def test_truncated_png_rejected(self):
        path = self.save(Image.new('RGBA', (64, 64)))
        path.write_bytes(path.read_bytes()[:32])
        with self.assertRaises((OSError, ValueError)):
            inspection.inspect_png(path, (64, 64))


if __name__ == '__main__':
    unittest.main()
