"""Green-screen key for generated SD portraits -> transparent 864x1152 PNGs in both Unity projects.

Usage: python tools/key_chroma.py
Sources: <assets>/{PREFIX}{name}.jpg (generated on flat green). Missing mirror poses are made by flipping.
"""
from PIL import Image, ImageOps
import os

SRC_DIR = r"C:\Users\myg17\.cursor\projects\c-untitled-game\assets"
OUT_DIRS = [
    r"c:\untitled-game\Assets\Resources\Portraits",
    r"c:\untitled-game\untitled-game\Assets\Resources\Portraits",
]
TARGET = (864, 1152)
PREFIX = "cb2_"

# output name -> source name (<PREFIX><source>.jpg)
SOURCES = {
    "intern": "intern",
    "intern_right": "intern_right",
    "intern_chat": "intern_chat",
    "minjae": "minjae",
    "minjae_left": "minjae_left",
    "minjae_chat": "minjae_chat",
    "park": "park",
    "park_left": "park_left",
}
# output name -> output name to mirror
MIRRORS = {
    "intern_left": "intern_right",
    "minjae_right": "minjae_left",
    "park_right": "park_left",
}


def key_green(im: Image.Image) -> Image.Image:
    im = im.convert("RGBA")
    im.thumbnail(TARGET, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", TARGET, (0, 0, 0, 0))
    canvas.paste(im, ((TARGET[0] - im.width) // 2, TARGET[1] - im.height))
    px = canvas.load()
    w, h = canvas.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            spill = g - max(r, b)
            if spill <= 12:
                continue
            t = (spill - 12) / 70.0
            alpha = 1.0 - (1.0 if t > 1 else t)
            g2 = max(r, b)
            px[x, y] = (r, g2, b, int(a * alpha))
    return canvas


def main() -> None:
    made = {}
    for out_name, src_name in SOURCES.items():
        path = os.path.join(SRC_DIR, PREFIX + src_name + ".jpg")
        if not os.path.isfile(path):
            print("MISSING", path)
            continue
        made[out_name] = key_green(Image.open(path))
    for out_name, base in MIRRORS.items():
        if base in made:
            made[out_name] = ImageOps.mirror(made[base])
    for name, img in made.items():
        for d in OUT_DIRS:
            os.makedirs(d, exist_ok=True)
            img.save(os.path.join(d, name + ".png"), "PNG")
        print("saved", name)
    print("done", len(made))


if __name__ == "__main__":
    main()
