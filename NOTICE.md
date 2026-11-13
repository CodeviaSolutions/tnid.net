# Provenance

TNID.NET is an original implementation by Codevia Solutions s.r.o. of the
[TNID 1.0 specification](https://github.com/mkeedlinger/tnid/blob/e7a1d5913ef012dfb9aa3a731e9dba02c7628982/spec.md),
pinned to commit `e7a1d5913ef012dfb9aa3a731e9dba02c7628982` (version 1.0, Stable).
The [upstream license at the same revision](https://github.com/mkeedlinger/tnid/blob/e7a1d5913ef012dfb9aa3a731e9dba02c7628982/LICENSE.txt)
was inspected on 2026-11-13: MIT, with `Copyright (c) 2026 Michael Edlinger`.

No upstream implementation source, specification text, or test fixture is
redistributed. Checked-in fixtures are independently authored from the format
rules. The official JSON supplied locally in `docs/test-vectors.json` is used for
external comparison, is ignored by Git, and is excluded from packages. Its
redistribution license has not been established from the available evidence;
the specification's MIT license is not assumed to license the website fixture.

See [verification results](docs/VERIFICATION.md) for the official fixture URL,
content hash, results, and the successful .NET 11 package consumer run.
