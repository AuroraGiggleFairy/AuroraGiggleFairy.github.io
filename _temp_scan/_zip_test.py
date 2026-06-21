import os
import zipfile

src = r"03_ReleaseSource/AGF-NoEAC-AutoRun-v2.0.1"
mod = "AGF-NoEAC-AutoRun-v2.0.1"
out = r"_temp_scan/AGF-NoEAC-AutoRun-scriptstyle.zip"
os.makedirs("_temp_scan", exist_ok=True)


def arc(*parts):
    normalized_parts = []
    for part in parts:
        if part is None:
            continue
        text = str(part).replace("\\", "/").strip("/")
        if text:
            normalized_parts.append(text)
    return "/".join(normalized_parts)

with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
    for root, _, files in os.walk(src):
        for file_name in files:
            file_path = os.path.join(root, file_name)
            arcname = arc(mod, os.path.relpath(file_path, src))
            z.write(file_path, arcname)

print(out)
