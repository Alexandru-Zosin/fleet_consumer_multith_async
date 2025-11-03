import os
from pathlib import Path

BASE_DIR = Path(__file__).parent

def cleanup():
    keep = {
        # main data files
        "telemetry_20251103_152615_V-117.tmp",
        "telemetry_20251103_152615_V-127.tmp",
        "telemetry_20251103_152615_V-117.jsonl",
        "telemetry_20251103_152615_V-127.jsonl",
        # metadata
        "telemetry_20251103_152615_V-117.jsonl.meta.json",
        "telemetry_20251103_152615_V-127.jsonl.meta.json",
        # scripts
        "script.py",
        "script_clean.py",
        #logfile
        "file_metrics.log"
    }

    for file in BASE_DIR.iterdir():
        if file.is_file() and file.name not in keep:
            try:
                file.unlink()
            except Exception as e:
                print(f"Failed to delete {file.name}: {e}")

if __name__ == "__main__":
    cleanup()
