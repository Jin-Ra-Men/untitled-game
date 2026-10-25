"""Remove leftover magenta key background from front portraits (art itself untouched)."""
from PIL import Image
import os

dirs = [
    r"c:\untitled-game\Assets\Resources\Portraits",
    r"c:\untitled-game\untitled-game\Assets\Resources\Portraits",
]
names = ["intern.png", "minjae.png", "park.png"]


def is_magenta(r: int, g: int, b: int) -> bool:
    return r > 140 and b > 90 and g < min(r, b) * 0.55


def clean(path: str) -> int:
    im = Image.open(path).convert("RGBA")
    px = im.load()
    w, h = im.size
    removed = 0
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            if is_magenta(r, g, b):
                px[x, y] = (0, 0, 0, 0)
                removed += 1
    # Despill: semi-magenta edge pixels next to removed area
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            if r > g + 40 and b > g + 40:
                m = max(g, min(r, b) - 60)
                px[x, y] = (min(r, m + 40), g, min(b, m + 40), a)
    im.save(path, "PNG")
    return removed


for d in dirs:
    for n in names:
        p = os.path.join(d, n)
        if os.path.isfile(p):
            print(p, "removed", clean(p))
