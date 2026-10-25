from PIL import Image
import os

src_dir = r"C:\Users\myg17\.cursor\projects\c-untitled-game\assets"
out_dirs = [
    r"c:\untitled-game\Assets\Resources\Portraits",
    r"c:\untitled-game\untitled-game\Assets\Resources\Portraits",
]
names = [
    "intern_right",
    "intern_left",
    "intern_phone",
    "minjae_right",
    "minjae_left",
    "minjae_phone",
    "park_right",
    "park_left",
    "park_phone",
]
TARGET = (864, 1152)


def key_black(im: Image.Image) -> Image.Image:
    im = im.convert("RGBA")
    im.thumbnail(TARGET, Image.Resampling.LANCZOS)
    canvas = Image.new("RGBA", TARGET, (0, 0, 0, 0))
    x0 = (TARGET[0] - im.width) // 2
    y0 = (TARGET[1] - im.height) // 2
    canvas.paste(im, (x0, y0))
    px = canvas.load()
    w, h = canvas.size
    for y in range(h):
        for x in range(w):
            r, g, b, a = px[x, y]
            lum = (r + g + b) / 3.0
            if r < 28 and g < 28 and b < 28:
                px[x, y] = (r, g, b, 0)
            elif r < 45 and g < 45 and b < 45:
                t = (lum - 10) / 35.0
                t = 0.0 if t < 0 else 1.0 if t > 1 else t
                px[x, y] = (r, g, b, int(a * t))
    return canvas


def main() -> None:
    for name in names:
        src = None
        for ext in (".jpg", ".jpeg", ".png"):
            p = os.path.join(src_dir, name + ext)
            if os.path.isfile(p):
                src = p
                break
        if not src:
            print("MISSING", name)
            continue
        out = key_black(Image.open(src))
        for d in out_dirs:
            os.makedirs(d, exist_ok=True)
            dest = os.path.join(d, name + ".png")
            out.save(dest, "PNG")
            print("saved", dest, out.mode, out.size)
    print("done")


if __name__ == "__main__":
    main()
