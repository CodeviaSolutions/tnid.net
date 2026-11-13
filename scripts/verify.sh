#!/usr/bin/env bash
set -euo pipefail
root=$(cd "$(dirname "$0")/.." && pwd)
cd "$root"
export DOTNET_CLI_HOME="${DOTNET_CLI_HOME:-${TMPDIR:-/tmp}/tnid-dotnet}"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_GENERATE_ASPNET_CERTIFICATE=false
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=true
mkdir -p artifacts

dotnet --version | tee artifacts/sdk-net10.txt
dotnet build src/TNID.NET -c Release --disable-build-servers
vectors=${TNID_TEST_VECTORS:-docs/test-vectors.json}
if [[ -f "$vectors" ]]; then
  dotnet run --project tests/TNID.NET.Conformance -c Release --disable-build-servers -- "$vectors"
elif [[ -n "${TNID_TEST_VECTORS:-}" ]]; then
  echo "Official fixture not found: $vectors" >&2
  exit 1
else
  echo 'NOT RUN: official JSON vectors. Supply a local file or TNID_TEST_VECTORS.'
  dotnet run --project tests/TNID.NET.Conformance -c Release --disable-build-servers
fi
if dotnet build tests/TypeSafety -c Release --disable-build-servers > artifacts/type-safety.log 2>&1; then
  echo 'FAIL: wrong typed argument compiled.' >&2
  exit 1
fi
if ! python3 -c 'import pathlib, re, sys; sys.exit(not re.search(r"error CS1503:.*TnidNet.Tnid<Partner>.*TnidNet.Tnid<Lead>", pathlib.Path("artifacts/type-safety.log").read_text()))'; then
  cat artifacts/type-safety.log
  echo 'FAIL: compilation failed for an unexpected reason.' >&2
  exit 1
fi
echo 'PASS: Tnid<Partner> cannot be passed to a Tnid<Lead> parameter (CS1503).'
dotnet pack src/TNID.NET -c Release --no-build -o artifacts --disable-build-servers
python3 scripts/inspect-package.py
python3 -c 'import hashlib, pathlib; p = pathlib.Path("artifacts/TNID.NET.0.1.0.nupkg"); line = hashlib.sha256(p.read_bytes()).hexdigest() + "  " + p.name + "\n"; p.with_suffix(p.suffix + ".sha256").write_text(line); print(line, end="")'
# Fresh package caches prove the consumers use this package rather than stale globals.
cache10=$(mktemp -d "${TMPDIR:-/tmp}/tnid-package10.XXXXXX")
cache11=''
trap 'rm -rf "$cache10" "${cache11:-}"' EXIT
dotnet restore tests/PackageConsumer --source "$root/artifacts" --packages "$cache10" --force --disable-build-servers
python3 scripts/check-consumer-assets.py tests/PackageConsumer/obj/project.assets.json artifacts/TNID.NET.0.1.0.nupkg net10.0
dotnet run --project tests/PackageConsumer -c Release --no-restore --disable-build-servers
if [[ "${1:-}" == '--net11' ]]; then
  cache11=$(mktemp -d "${TMPDIR:-/tmp}/tnid-package11.XXXXXX")
  cd tests/PackageConsumer11
  "${DOTNET11:-dotnet}" --version | tee "$root/artifacts/sdk-net11.txt"
  "${DOTNET11:-dotnet}" restore --source "$root/artifacts" --packages "$cache11" --force --disable-build-servers
  python3 "$root/scripts/check-consumer-assets.py" obj/project.assets.json "$root/artifacts/TNID.NET.0.1.0.nupkg" net11.0
  "${DOTNET11:-dotnet}" run -c Release --no-restore --disable-build-servers
elif [[ $# -gt 0 ]]; then
  echo 'Usage: bash scripts/verify.sh [--net11]' >&2
  exit 1
else
  echo 'NOT RUN: net11.0 consumer. Install the official .NET 11 SDK and use --net11.'
fi
