#!/usr/bin/env python3
# Python 3.9+

from pathlib import Path
from shutil import copyfile
from time import sleep

COUNT = 100

SRC_117_TMP = Path("telemetry_20251103_152615_V-117.tmp")
SRC_127_TMP = Path("telemetry_20251103_152615_V-127.tmp")
SRC_117_META = Path("telemetry_20251103_152615_V-117.jsonl.meta.json")
SRC_127_META = Path("telemetry_20251103_152615_V-127.jsonl.meta.json")

def require_exists(p: Path) -> None:
    if not p.is_file():
        raise FileNotFoundError(f"Missing file: {p}")

def insert_index_before_extension(p: Path, index: int) -> Path:
    name = p.name
    dot = name.find(".")
    if dot == -1:
        new_name = f"{name} ({index})"
    else:
        new_name = f"{name[:dot]} ({index}){name[dot:]}"
    return p.with_name(new_name)

def duplicate_series(src_data: Path, src_meta: Path, count: int) -> list[Path]:
    created_tmps: list[Path] = []
    for i in range(1, count + 1):
        dst_data = insert_index_before_extension(src_data, i)
        dst_meta = insert_index_before_extension(src_meta, i)

        if dst_data.exists() or dst_meta.exists():
            raise FileExistsError(f"Target already exists: {dst_data} or {dst_meta}")

        copyfile(src_data, dst_data)
        copyfile(src_meta, dst_meta)
        created_tmps.append(dst_data)
    return created_tmps

def rename_tmp_to_jsonl(paths: list[Path]) -> None:
    for p in paths:
        if p.suffix.lower() != ".tmp":
            continue
        if p.exists():
            target = p.with_suffix(".jsonl")
            if target.exists():
                raise FileExistsError(f"Refusing to overwrite: {target}")
            p.rename(target)

def main() -> None:
    for src in (SRC_117_TMP, SRC_127_TMP, SRC_117_META, SRC_127_META):
        require_exists(src)

    created_117_tmps = duplicate_series(SRC_117_TMP, SRC_117_META, COUNT)
    created_127_tmps = duplicate_series(SRC_127_TMP, SRC_127_META, COUNT)

    sleep(1)  # 5-second delay between renames
    # Only rename duplicates, not the originals
    rename_tmp_to_jsonl(created_117_tmps + created_127_tmps)

if __name__ == "__main__":
    main()
