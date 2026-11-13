#!/usr/bin/env python3
"""Verify the shipped NuGet contract using only the Python standard library."""
from pathlib import Path
import sys
import xml.etree.ElementTree as ET
import zipfile

root = Path(__file__).resolve().parent.parent
package = Path(sys.argv[1]) if len(sys.argv) > 1 else root / "artifacts/TNID.NET.0.1.0.nupkg"
with zipfile.ZipFile(package) as z:
    names = z.namelist()
    metadata = ET.fromstring(z.read("TNID.NET.nuspec"))
    ns = {"n": metadata.tag.split("}")[0].removeprefix("{")}
    def field(name):
        return metadata.find(f"n:metadata/n:{name}", ns)
    assert field("id").text == "TNID.NET"
    assert field("version").text == "0.1.0"
    assert field("authors").text == "Codevia Solutions s.r.o."
    license_node = field("license")
    assert license_node.attrib["type"] == "expression" and license_node.text == "MIT"
    assert field("repository").attrib["url"] == "https://github.com/CodeviaSolutions/tnid.net"
    assert not metadata.findall(".//n:dependency", ns), "Third-party dependencies present"
    assert [p for p in names if p.startswith("lib/")] == ["lib/net10.0/TNID.NET.dll"]
    for file in ("LICENSE", "README.md", "NOTICE.md"):
        assert z.read(file) == (root / file).read_bytes(), f"Incorrect packaged {file}"
    assert not any(p.startswith(("tests/", "src/")) for p in names)
    print(f"PASS: {package.name}: TNID.NET, Codevia Solutions s.r.o., MIT, net10.0 only, zero dependencies.")
    print("Package contents:")
    for name in names:
        print(f"  {name}")
