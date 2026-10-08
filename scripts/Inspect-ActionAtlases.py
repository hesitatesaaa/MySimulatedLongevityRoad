"""Report connected sprite poses in generated atlas columns for crop QA."""
import sys
from pathlib import Path
import numpy as np
from PIL import Image
from scipy import ndimage

def inspect(path):
    image = np.asarray(Image.open(path).convert("RGBA"))
    cell_w = image.shape[1] // 5
    print(path.name, image.shape[:2])
    for col in range(5):
        alpha = image[:,col*cell_w:(col+1)*cell_w,3] >= 112
        joined = ndimage.binary_closing(alpha, structure=np.ones((5,5)))
        labels,count = ndimage.label(joined)
        comps = []
        for label in range(1,count+1):
            ys,xs = np.where(labels == label)
            if len(xs) < 180:
                continue
            comps.append((len(xs),(int(xs.min()),int(ys.min()),
                                  int(xs.max()+1),int(ys.max()+1))))
        comps.sort(key=lambda item:item[1][1])
        print(col,comps[:12])

if __name__ == "__main__":
    for arg in sys.argv[1:]:
        inspect(Path(arg))
