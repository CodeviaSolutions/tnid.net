#!/usr/bin/env python3
"""Check NuGet asset selection and the exact restored archive, with no dependencies."""
import hashlib
import json
from pathlib import Path
import sys

assets_path, package_path, tfm = sys.argv[1:]
assets = json.loads(Path(assets_path).read_text())
package = Path(package_path).resolve()
key = "TNID.NET/0.1.0"
assert set(assets["targets"]) == {tfm}, "Unexpected consumer framework"
assert set(assets["targets"][tfm]) == {key}, "Unexpected package dependencies"
target = assets["targets"][tfm][key]
for kind in ("compile", "runtime"):
    assert set(target[kind]) == {"lib/net10.0/TNID.NET.dll"}, f"Wrong {kind} asset"
assert set(assets["libraries"]) == {key}, "Consumer did not restore only TNID.NET"
assert assets["libraries"][key]["type"] == "package", "Consumer uses a project reference"
restored = [Path(folder) / "tnid.net/0.1.0/tnid.net.0.1.0.nupkg"
            for folder in assets["packageFolders"]]
restored = [path for path in restored if path.is_file()]
assert len(restored) == 1 and restored[0].read_bytes() == package.read_bytes(), "Restored archive differs from candidate"
print(f"PASS: {tfm} NuGet compile and runtime assets: lib/net10.0/TNID.NET.dll")
print(f"PASS: restored actual package SHA-256 {hashlib.sha256(package.read_bytes()).hexdigest()}")
