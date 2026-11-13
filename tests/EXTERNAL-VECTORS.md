# Official external conformance vectors

Fixture URL: https://tnid.info/test-vectors.json
Documentation: https://tnid.info/implementing/test-vectors/
Expected SHA-256: `cd2609f5abae0ad1c4bad010d9a88f03160046086348ad05d348e9a2f890b65c`.
The JSON declares spec version 1 and generation date 2026-09-28.

The user supplied the JSON locally as `docs/test-vectors.json`. Its hash was
verified before execution. The fixture has no license field, and its website
license could not be established. The pinned specification's MIT license does
not establish the fixture's redistribution license. Keep the supplied file local
and ignored by Git; it is never included in NuGet packages. Do not commit or
redistribute it until its license is confirmed and any required notices are
included verbatim. The test runner contains no copied fixture data.

Run from the repository root:

```sh
dotnet run --project tests/TNID.NET.Conformance -c Release -- docs/test-vectors.json
```

The runner verifies the original JSON bytes against the expected hash. It reads
JSON directly, preserving Unicode, newlines, exact casing, and empty values.
All 43 core rows run: 8 names, 5 v0 layouts/timestamps including wrap, 4 v1 layouts,
5 round trips including reserved variants, 1 uppercase UUID boundary case,
13 rejected TNID strings, and 7 rejected UUID inputs. Two malformed UUID strings
are rejected by the BCL's `Guid.TryParse` at the explicit application boundary;
the other five are syntactically valid UUIDs rejected by TNID.NET. Both `Try` and
throwing library paths are checked where applicable.

The 3 `left_open` cases verify the documented choice that canonical TNID parsing
rejects alternate UUID text. `Guid.TryParse` accepts the unhyphenated and braced
forms and rejects the URN form on the tested runtime. The 4 optional `encryption`
rows are explicitly skipped because this library implements only the core format.

Generation uses `RandomNumberGenerator`, so the deterministic fixture random
values are checked by reconstructing their UUID layout and round-tripping through
the library. Generated v0 values separately verify the exact timestamp, name,
UUID bits, and TNID variant while allowing their random tails to vary.

Results are recorded in `docs/VERIFICATION.md` and `artifacts/official-vectors.log`.
The executable also retains the older optional TSV input format (`valid`,
`invalid-text`, `invalid-uuid` rows); the official JSON path requires no conversion.
