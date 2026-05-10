from __future__ import annotations

import argparse
import os
import signal
import subprocess
import sys
from pathlib import Path


DEFAULT_HERMES = Path("/home/guegue/.local/bin/hermes")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Run a Hermes oneshot prompt from a file.")
    parser.add_argument("prompt_file", type=Path, help="Path to a UTF-8 prompt file.")
    parser.add_argument("--timeout", type=int, default=30, help="Timeout in seconds.")
    parser.add_argument(
        "--hermes",
        type=Path,
        default=DEFAULT_HERMES,
        help="Absolute path to the hermes executable.",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    prompt = args.prompt_file.read_text(encoding="utf-8").strip()
    if not prompt:
        print("Prompt file is empty.", file=sys.stderr)
        return 2

    env = os.environ.copy()
    env.setdefault("TERM", "xterm")
    env.setdefault("COLUMNS", "120")
    env.setdefault("LINES", "40")

    process = subprocess.Popen(
        [str(args.hermes), "-z", prompt],
        env=env,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        start_new_session=True,
    )

    try:
        stdout, stderr = process.communicate(timeout=args.timeout)
    except subprocess.TimeoutExpired:
        os.killpg(process.pid, signal.SIGKILL)
        stdout, stderr = process.communicate()
        if stdout:
            sys.stdout.write(stdout)
        if stderr:
            sys.stderr.write(stderr)
        print("Hermes oneshot timed out.", file=sys.stderr)
        return 124

    if stdout:
        sys.stdout.write(stdout)
    if stderr:
        sys.stderr.write(stderr)
    return process.returncode


if __name__ == "__main__":
    raise SystemExit(main())
