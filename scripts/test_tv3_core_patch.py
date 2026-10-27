#!/usr/bin/env python3
"""Self-test TV3 patcher WITHOUT copying private project files to GitHub."""
import importlib.util
import subprocess
import sys
import tempfile
from pathlib import Path

SCRIPT = Path(__file__).with_name("tv3_core_patch.py")
spec = importlib.util.spec_from_file_location("tv3_patch", SCRIPT)
patch = importlib.util.module_from_spec(spec)
spec.loader.exec_module(patch)

with tempfile.TemporaryDirectory() as temporary:
    root = Path(temporary) / "VanPhongPham"
    root.mkdir()
    (root / "VanPhongPham.csproj").write_text("<Project/>")
    for relative, edits in patch.FIXES.items():
        target = root / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        # Only minimal anchors, no actual TV3 source code.
        target.write_text("\n".join(old for old, _ in edits) + "\n", encoding="utf-8")

    # PASS 1: validate all six targeted files and replacement output
    files = patch.compute(root)
    assert len(files) == 6
    assert all(old != new for old, new in files.values())
    for relative, edits in patch.FIXES.items():
        updated = files[root / relative][1]
        for _, after in edits:
            assert after in updated

    # PASS 2: dry-run does not touch file content
    before = {p: p.read_bytes() for p in files}
    subprocess.run([sys.executable, str(SCRIPT), str(root)], check=True, capture_output=True)
    assert all(p.read_bytes() == data for p, data in before.items())

    # PASS 3: fail closed when source differs
    missing = root / "Program.cs"
    missing.write_text("// mismatched current main version")
    try:
        patch.compute(root)
    except ValueError:
        pass
    else:
        raise AssertionError("Patch allowed unreviewed Program.cs")
    for p, data in before.items():
        p.write_bytes(data)

    # PASS 4: refuses dirty working tree
    subprocess.run(["git", "init", "-q", str(root)], check=True)
    dirty = subprocess.run([sys.executable, str(SCRIPT), str(root), "--apply"],
                           capture_output=True, text=True)
    assert dirty.returncode != 0 and "not clean" in dirty.stderr

    # PASS 5: clean branch applies, then refuses repeat application
    subprocess.run(["git", "-C", str(root), "add", "."], check=True)
    subprocess.run(["git", "-C", str(root), "-c", "user.name=CI",
                    "-c", "user.email=ci@example.invalid", "commit", "-qm", "base"],
                   check=True)
    subprocess.run([sys.executable, str(SCRIPT), str(root), "--apply"], check=True, capture_output=True)
    for path, (_, new) in files.items():
        assert path.read_text(encoding="utf-8") == new
    repeated = subprocess.run([sys.executable, str(SCRIPT), str(root), "--apply"],
                              capture_output=True, text=True)
    assert repeated.returncode != 0 and "Source differs" in repeated.stderr

print("PASS: TV3 patch preview, failsafe, clean-branch application and repeat protection.")
