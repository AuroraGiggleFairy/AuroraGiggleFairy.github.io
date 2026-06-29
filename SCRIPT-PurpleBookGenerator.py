"""Compatibility launcher for the Purple Book generator.

The actual generator now lives under:
_DLL-Projects/AGF-PurpleBookGenerator-v0.0.1/Generator/SCRIPT-PurpleBookGenerator.py
"""
from __future__ import annotations

import runpy
from pathlib import Path

TARGET = (
    Path(__file__).resolve().parent
    / "_DLL-Projects"
    / "AGF-PurpleBookGenerator-v0.0.1"
    / "Generator"
    / "SCRIPT-PurpleBookGenerator.py"
)

if __name__ == "__main__":
    runpy.run_path(str(TARGET), run_name="__main__")
