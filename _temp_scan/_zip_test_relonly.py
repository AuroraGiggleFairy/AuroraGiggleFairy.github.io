import os
import zipfile

src = r"03_ReleaseSource/AGF-NoEAC-AutoRun-v2.0.1"
out = r"_temp_scan/AGF-NoEAC-AutoRun-py-relonly.zip"
os.makedirs("_temp_scan", exist_ok=True)

with zipfile.ZipFile(out, "w", zipfile.ZIP_DEFLATED) as z:
    for root, _, files in os.walk(src):
        for file_name in files:
            file_path = os.path.join(root, file_name)
            arcname = os.path.relpath(file_path, src).replace("\\", "/")
            z.write(file_path, arcname)

print(out)
