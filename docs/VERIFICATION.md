# Verification results

Date: 2026-11-13. Author and publisher: **Codevia Solutions s.r.o.**
Verified library: `TNID.NET` version `0.1.0`. Local host: macOS 26.6, arm64.

## Publication

The publisher confirmed successful NuGet publication through GitHub OIDC Trusted
Publishing on 2026-11-13:
[TNID.NET 0.1.0](https://www.nuget.org/packages/TNID.NET/0.1.0).
The local release tag `v0.1.0` resolves to commit
`df32a573d81bf03cd9ffa78fd6e792a651c5b7bc`.
The release workflow verifies and packs the package before its separate OIDC
publish job. Public NuGet and GitHub Actions retrieval remains unavailable from
this workspace, so the publication result is recorded from the publisher's
confirmation; hosted logs and the public package bytes were not independently
retrieved. The local package hashes below identify pre-publication verification
artifacts and must not be used as checksums of the CI-built published package.

Target repository confirmed by the user:
`https://github.com/CodeviaSolutions/tnid.net.git`.
Organization slug: `CodeviaSolutions`. Package repository and project metadata
already use the matching URL `https://github.com/CodeviaSolutions/tnid.net`.
The publisher initialized Git and pushed the source manually after sandbox
restrictions blocked the initial Git operation. Local `origin` now matches this
repository, and the release tag is present.

## Specification and licensing

The implementation is pinned to TNID 1.0 (Stable), commit
`e7a1d5913ef012dfb9aa3a731e9dba02c7628982`:
[specification](https://github.com/mkeedlinger/tnid/blob/e7a1d5913ef012dfb9aa3a731e9dba02c7628982/spec.md),
[license](https://github.com/mkeedlinger/tnid/blob/e7a1d5913ef012dfb9aa3a731e9dba02c7628982/LICENSE.txt).
Both pinned files were retrieved through the web tools and inspected in this
follow-up. The license is MIT, with `Copyright (c) 2026 Michael Edlinger`.
The existing codec was reviewed against the pinned core layout and encoding
rules; no production code changes were needed. The optional FF1 encryption
extension is outside the library's scope.

The implementation and checked-in fixtures are independently authored. No
upstream source or specification text is redistributed. The official JSON was
provided locally for external comparison; its redistribution license remains
unestablished. It has no license field, and the specification's MIT license is
not assumed to cover website fixtures. `docs/test-vectors.json` is ignored by
Git and excluded from packages. No copy is approved for committing. If upstream
material is later redistributed, preserve all required copyright and license
notices verbatim in the repository and each package containing it.

## Completed locally

| Check | Result |
| --- | --- |
| Original build SDK/runtime | .NET SDK **10.0.403**, Microsoft.NETCore.App **10.0.14** |
| Refreshed build SDK/runtime | .NET SDK **10.0.404**, Microsoft.NETCore.App **10.0.15** |
| Target | `net10.0` only |
| Release build | Passed; zero warnings and errors |
| Original executable conformance suite | **1,068,162 checks passed** |
| Suite including official JSON | **1,068,293 checks passed**, on .NET 10.0.14 and .NET 10.0.15 |
| Compile-time type separation | Expected **CS1503** for `Tnid<Partner>` passed to `Tnid<Lead>` |
| Original Release package | Preserved byte-for-byte in `artifacts/original/TNID.NET.0.1.0.nupkg` |
| Original and refreshed package inspection | Requested ID, sole author Codevia Solutions s.r.o., SPDX MIT, LICENSE/README/NOTICE, one net10.0 assembly, no NuGet dependencies |
| Original .NET 10 packed consumer | Passed on .NET 10.0.14, restored solely from the local package into a fresh isolated cache |
| Refreshed .NET 10 packed consumer | Passed on **10.0.15**; NuGet selected `lib/net10.0/TNID.NET.dll` for both compile and runtime; restored archive matched the candidate exactly |
| .NET 11 packed consumer | Passed with SDK **11.0.100** on runtime **11.0.0**, targeting `net11.0`; NuGet selected `lib/net10.0/TNID.NET.dll` for both compile and runtime, with restored archive bytes verified |

Original package SHA-256:
`bf22fa22760efc9e70dabbef08da7509fd97a48bc5bd368e7a712a31a5349e2a`.
The DLL exercised by the first official-vector run is byte-for-byte identical to
`lib/net10.0/TNID.NET.dll` in that package, SHA-256:
`fb06b6a2304c7bc9dd37df9247af2a72b12b6cecb5970583324f45fb58355894`.

The preserved archive contains the README and NOTICE from its original packing;
they predate this follow-up's pinned provenance and vector results. Source
materials now contain the updated evidence. The refreshed local candidate at
`artifacts/TNID.NET.0.1.0.nupkg` includes these updated README/NOTICE files; the
original archive remains separate for traceability. These archives were used for
local verification; the published package was built separately by GitHub Actions.

Refreshed package SHA-256:
`ea686ca0c09211f62e4579716010dcf6e20dda4cd0ff08f3a0c6b619acaf25e7`.
Its DLL, rebuilt with SDK 10.0.404 and covered by the repeated official-vector
suite, has SHA-256
`4ab6fa2dd24a4d49e97842a665002d6e450ba35990297075d38ccafaa04830ba`.
Both `net10.0` and `net11.0` consumers restored this same refreshed archive from
the local feed into separate fresh caches and verified the archive bytes.

## Official vectors

Fixture URL: https://tnid.info/test-vectors.json
Documentation: https://tnid.info/implementing/test-vectors/
JSON `spec_version`: 1. JSON `generated`: 2026-09-28.
SHA-256 verified against the supplied original bytes:
`cd2609f5abae0ad1c4bad010d9a88f03160046086348ad05d348e9a2f890b65c`.

| Section | Rows | Result |
| --- | ---: | --- |
| names | 8 | Passed: encoding, decoding, trailing name padding |
| variant_0 | 5 | Passed: text/UUID, deterministic layout, public generation timestamp including wrap |
| variant_1 | 4 | Passed: text/UUID, deterministic payload layout, variant |
| round_trips | 5 | Passed, including reserved variants 2 and 3 |
| uuid_case_insensitive | 1 | Passed through explicit `Guid.Parse` import |
| reject_strings | 13 | Passed for both `TryParse` and throwing `Parse` |
| reject_uuids | 7 | Passed: 2 malformed strings rejected by `Guid.TryParse`; 5 UUIDs rejected by both library import paths |
| left_open | 3 | Documented canonical parser rejects all three; BCL accepts unhyphenated/braced forms and rejects URN |
| encryption | 4 | Skipped: optional FF1 extension is outside the core-only scope |

Total: **43 core rows passed**, **3 left_open decisions verified**, **4 optional
encryption rows skipped**. The JSON is parsed directly without normalization.
The runner verifies the fixture hash before testing. Deterministic fixture
layouts are checked independently; production random generation is not modified
or supplied deterministic entropy.

Run: `dotnet run --project tests/TNID.NET.Conformance -c Release -- docs/test-vectors.json`.
Output: `artifacts/official-vectors.log` (original DLL/runtime 10.0.14) and
`artifacts/verification-refreshed.log` (refreshed DLL/runtime 10.0.15).

The independently authored suite additionally exhausts all 1,048,576 name
encodings and checks exact casing, name padding, byte order, typed-name rejection,
reserved variants, defaults, v0 timestamp wrap including negative epochs, and
ordinal comparison. Across 10,000 adjacent milliseconds it checks that the
maximum prior random tail precedes the minimum next tail. Within-millisecond
ordering remains random.

## NuGet public listing check

The user supplied an external public-listing check on 2026-11-13:
[package page](https://www.nuget.org/packages/TNID.NET) and
[flat-container API](https://api.nuget.org/v3-flatcontainer/tnid.net/index.json)
returned **HTTP 404**. This records that the requested ID was not publicly
listed at that time. It does **not** establish registration availability,
reservation status, or publishing rights. No ownership conflict is claimed;
the requested ID remains `TNID.NET`. This workspace could not independently
repeat the HTTP check because its shell network is disabled. This historical
pre-publication result is superseded by the publisher's successful-publication
confirmation recorded above.

## .NET 11 package consumer

SDK **11.0.100** became available during this follow-up. The existing archive
was then restored from a temporary local feed into an isolated package cache
and executed by the `net11.0` consumer on **.NET 11.0.0**. Both compilation and
runtime asset entries in `project.assets.json` selected
`lib/net10.0/TNID.NET.dll`. The restored `.nupkg` was byte-for-byte equal to the
original candidate with the SHA-256 recorded above. No project reference or
recompiled library was used by this consumer.

Command: `bash scripts/verify-package.sh net11.0` (before repacking).
Output: `artifacts/verification-net11.log`. The script enforces exact SDK
11.0.100, never rebuilds or repacks the library, and verifies the restored archive
and asset paths before running. Reproduce against the preserved original with
`bash scripts/verify-package.sh net11.0 artifacts/original/TNID.NET.0.1.0.nupkg`.
The full refreshed-candidate verification also passed using
`DOTNET11=/usr/local/share/dotnet/dotnet bash scripts/verify.sh --net11`.
The explicit path selects the official .NET 11 installation alongside the
Homebrew .NET 10 executable on this host.
Microsoft's [official download page](https://dotnet.microsoft.com/en-us/download/dotnet/11.0)
lists SDK 11.0.100, released 2026-11-10. Earlier download/SDK resolution failures
are superseded by this successful execution; no network workaround was needed.

## Verification limits and future releases

- **Hosted logs and public package bytes:** successful Trusted Publishing was
  reported by the publisher. This workspace could not independently retrieve
  the hosted logs or the public package. The release workflow selects SDK
  11.0.100, compares the official JSON externally using its pinned hash, and
  checks both packed consumers before publishing. The fixture is not committed,
  packaged, or uploaded as an artifact.
- **Fixture redistribution:** no redistribution is authorized or needed for the
  completed external comparison. Keep the local JSON ignored. Establish its
  license and required notices before committing or packaging a copy.
- **Future releases:** retain the package owner and Trusted Publishing policy;
  update all pinned package versions before creating a new release tag.

`artifacts/verification-net10.log`, `artifacts/type-safety.log`,
`artifacts/package-only-net10.log`, and `artifacts/verification-net11.log` retain
local execution evidence. `artifacts/verification-refreshed.log` records the full
verification of the refreshed candidate. The environment reports
`CODEX_SANDBOX_NETWORK_DISABLED=1`. Public upstream verification does not require
a GitHub connector. Source publication to GitHub and NuGet Trusted Publishing
were completed by the publisher as separate steps. Creating a GitHub Release
entry remains a separate action, as described in `docs/RELEASING.md`.
