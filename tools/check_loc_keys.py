"""Check that every Loc key referenced by episode code exists in each language file."""
import json
import re
import sys

EPISODE = r"c:\untitled-game\Assets\Scripts\Negotiation\Episodes\FamilyCertificateEpisode.cs"
LOC = r"c:\untitled-game\Assets\Resources\Loc\{lang}\ep_family_certificate.json"

src = open(EPISODE, encoding="utf-8").read()
keys = sorted(set(re.findall(r'"(fc\.[a-z0-9_.]+)"', src)))

ok = True
for lang in ("ko", "en"):
    data = json.load(open(LOC.format(lang=lang), encoding="utf-8"))
    missing = [k for k in keys if k not in data]
    print(f"{lang}: {len(keys)} keys referenced, missing={missing}")
    ok = ok and not missing

sys.exit(0 if ok else 1)
