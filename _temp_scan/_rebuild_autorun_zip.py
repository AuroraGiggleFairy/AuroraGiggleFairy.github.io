import os
import zipfile

src = r"03_ReleaseSource/AGF-NoEAC-AutoRun-v2.0.1"
mod_folder = "AGF-NoEAC-AutoRun-v2.0.1"
out = r"_temp_scan/AGF-NoEAC-AutoRun-rebuilt.zip"

os.makedirs(os.path.dirname(out), exist_ok=True)

with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as zf:
    for root, dirs, files in os.walk(src):
        dirs.sort()
        for file_name in sorted(files):
            file_path = os.path.join(root, file_name)
            rel = os.path.relpath(file_path, src).replace("\\", "/")
            arcname = f"{mod_folder}/{rel}".strip("/")
            zf.write(file_path, arcname)

print(out)
