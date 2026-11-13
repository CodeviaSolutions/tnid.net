# Releasing TNID.NET

Author and publisher: **Codevia Solutions s.r.o.**

The `0.1.0` local package is a verification candidate, not a published release.
Before a public release:

1. Use the confirmed target repository
   `https://github.com/CodeviaSolutions/tnid.net.git` under `CodeviaSolutions`.
   Package repository and project URLs already match this repository. Inspect
   the remote's current branch/history before the separate publication step.
2. Retain the pinned TNID 1.0 revision `e7a1d5913ef012dfb9aa3a731e9dba02c7628982` and its
   MIT license review in `NOTICE.md`. Review future changes against that core
   format. The local official JSON is not approved for redistribution: establish
   its license before committing or packaging a copy, and include all required
   verbatim copyright and license notices wherever that material is distributed.
3. Check the requested `TNID.NET` ID on NuGet, including ownership/reservation.
   If unavailable, report the conflict and keep the requested name pending a
   decision. The public page/API HTTP 404 check is recorded; it does not establish publish rights.
4. Preserve the completed official-vector comparison and fixture hash recorded in
   `docs/VERIFICATION.md`; rerun it for implementation changes as described in
   `tests/EXTERNAL-VECTORS.md`. Encryption is an optional extension outside scope.
5. Preserve the completed SDK 11.0.100/net11.0 package-consumer evidence. For any
   candidate change, run `bash scripts/verify.sh --net11` and update the recorded
   results. To test an existing archive without repacking, use
   `bash scripts/verify-package.sh net11.0 /absolute/path/to/package.nupkg`.
   Inspect the package again after changes to bundled documentation.

GitHub publication is a separate step: create or use the public repository under
Codevia Solutions, push the reviewed source, let CI pass, then tag and publish a
GitHub release with the reviewed package attached. Do not include personal bios,
additional project authors, or tool credits. The MIT notice and provenance apply.

NuGet publication is another separate step: confirm package ownership, inspect the
exact package checksum and metadata, then publish that package to NuGet using an
authorized Codevia Solutions account. Neither GitHub nor NuGet publishing runs in
CI. Do not assume completing one publication authorizes the other.

Suggested release title: `TNID.NET 0.1.0`.
Suggested release body: “TNID.NET by Codevia Solutions s.r.o. implements the TNID 1.0
core format for .NET 10 with no third-party dependencies. Includes v0/v1 generation,
canonical text, explicit UUID conversion, and typed names. MIT licensed.” Attach
completed verification results, package checksum, and any known limitations.
