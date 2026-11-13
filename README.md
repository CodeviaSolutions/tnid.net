# TNID.NET

A small C# implementation of the [TNID 1.0 core format](https://github.com/mkeedlinger/tnid/blob/e7a1d5913ef012dfb9aa3a731e9dba02c7628982/spec.md), authored and published by **Codevia Solutions s.r.o.** MIT licensed, single-targeted to `net10.0`, with zero third-party dependencies.

The specification and license are pinned, and the applicable official conformance vectors pass. The packed library is verified with .NET 10 and .NET 11 consumers; SDK 11.0.100 selects the package's `net10.0` asset for the .NET 11 consumer. See the [verification results](https://github.com/CodeviaSolutions/tnid.net/blob/main/docs/VERIFICATION.md).

## Use

Install [TNID.NET from NuGet](https://www.nuget.org/packages/TNID.NET):

```sh
dotnet add package TNID.NET --version 0.1.0
```

```csharp
using TnidNet;

Tnid id = Tnid.NewV0("lead");  // Time ordered within one name.
Tnid random = Tnid.NewV1("lead");

string text = id.ToString();  // Canonical TNID text for an API or URL.
Tnid parsed = Tnid.Parse(text);
bool accepted = Tnid.TryParse(text, out Tnid result);

Guid databaseValue = (Guid)id; // Explicit conversion for a PostgreSQL uuid parameter.
Tnid restored = (Tnid)databaseValue; // Validates TNID invariants.
// Equivalent named methods: id.ToGuid(), Tnid.FromGuid(databaseValue).
```

For typed names, implement a stable static marker. The marker name is part of stored identity; keep it constant for the lifetime of the data.

```csharp
using TnidNet;

Tnid<Lead> id = Tnid<Lead>.NewV0();
Tnid<Lead> parsed = Tnid<Lead>.Parse(id.ToString());
Guid databaseValue = (Guid)id;
Tnid<Lead> restored = (Tnid<Lead>)databaseValue;
Tnid untyped = (Tnid)id;
Tnid<Lead> checkedName = (Tnid<Lead>)untyped;

AcceptLead(id);
// AcceptLead(Tnid<Partner>.NewV1()); // Compile error CS1503.

static void AcceptLead(Tnid<Lead> value) { }
readonly struct Lead : ITnidName { public static string Name => "lead"; }
readonly struct Partner : ITnidName { public static string Name => "part"; }
```

`FromTnid`, `FromGuid`, `TryFromGuid`, `Parse`, and `TryParse` enforce the typed name. Typed equality and comparison apply to the same marker type. There are no implicit conversions between marker types. Applications that need an untyped boundary can explicitly convert through `Tnid` and validate a new marker.

## Format and compatibility contracts

Names contain 1–4 ASCII characters from `01234abcdefghijklmnopqrstuvwxyz`. Data contains exactly 17 characters from `-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz`, after one dot. Data letters are case sensitive: changing case changes the value. Uppercase names, whitespace, alternate alphabets, padding, Unicode lookalikes, and UUID hex text are rejected by canonical parsing. Nothing is trimmed or normalized.

`Parse(null)` throws `ArgumentNullException`; other invalid text throws `FormatException`. `TryParse` returns `false` and sets the result to `default` for null, invalid text, or a wrong typed name. `FromGuid` and `FromTnid` throw `ArgumentException` for invalid UUIDs, uninitialized input, or name mismatch as applicable. `TryFromGuid` returns `false` and `default` on failure. Invalid marker names fail generation with `ArgumentException` and make the `Try` methods return `false`.

UUID import requires UUID version 8, the RFC variant bits `10`, and a nonempty name with only trailing null padding. Canonical text and UUID import enforce the same TNID invariants. Reserved TNID variants 2 and 3 are accepted and preserved unchanged; generation exposes only variants 0 and 1. Text, UUID bytes, reserved-variant round trips, and this parsing behavior are compatibility contracts.

UUID conversion uses big-endian bytes explicitly. If an application handles raw bytes, use `id.ToGuid().ToByteArray(bigEndian: true)` and `new Guid(bytes, bigEndian: true)`. The parameterless `Guid.ToByteArray()` uses a different byte order for some fields.

Comparison matches `StringComparer.Ordinal` on canonical text, including across names. Use ordinal, case-sensitive comparison in application code and matching database collations for textual TNIDs.

## Generation and ordering

Both generators use `RandomNumberGenerator`. V0 contains a 43-bit Unix millisecond timestamp and 57 random bits; v1 contains 100 random bits. The overload `Tnid.NewV0(name, timestamp)` (and its typed equivalent) supports explicit timestamps. Timestamps wrap modulo `2^43`, including negative Unix timestamps. The first wrap after the Unix epoch occurs around 2248.

Within one name and one timestamp cycle, every v0 value from an earlier millisecond sorts before every v0 value from a later millisecond. Within a millisecond, order is random, with no monotonic counter or creation-order guarantee. Clock rollback and timestamp wrap can move new values before older ones. Across names, comparison groups by name first. Generation is probabilistically unique and does not coordinate processes.

## Default values

`default(Tnid)` and `default(Tnid<TName>)` are uninitialized sentinels. `IsInitialized` is `false`, `Name` and `ToString()` return empty strings, and `Variant` and `ToGuid()` throw `InvalidOperationException`. Defaults compare equal within the same value type and sort before initialized values. Exporting a typed default to untyped `Tnid` preserves the sentinel. Importing a default into a typed value is rejected. Empty text and `Guid.Empty` are never accepted as TNIDs.

## API and PostgreSQL boundaries

Use canonical text for API contracts and `Guid` for PostgreSQL `uuid` parameters and results. No serializer or database dependency is required by this library. For an existing `Guid` read from a column:

```csharp
if (Tnid.TryFromGuid(databaseValue, out Tnid tnid))
{
    string apiId = tnid.ToString();
    // Return apiId according to your application's contract.
}
else
{
    // Handle a legacy UUID through the consuming application's migration/boundary code.
}
```

Existing UUIDv4/v7 records can coexist with TNIDs in the same PostgreSQL `uuid` column. They fail TNID validation. Preserve legacy support in the application's migration or boundary code. This library does not reinterpret legacy identifiers as TNIDs.

## Build and verify

Install a .NET 10 SDK, then run:

```sh
bash scripts/verify.sh
```

This runs the dependency-free executable suite, checks that a wrong typed argument fails compilation, builds and packs Release, inspects the package metadata and contents, and runs a .NET 10 consumer restored exclusively from the actual local package into an isolated cache.

With the official .NET 11 SDK installed alongside .NET 10, run:

```sh
bash scripts/verify.sh --net11
```

This requires SDK 11.0.100, records its version, and runs a `net11.0` consumer against the same package while checking that NuGet selected `lib/net10.0/TNID.NET.dll` for compilation and execution. CI is configured to perform both consumer checks. To verify the existing archive without rebuilding or repacking it, run `bash scripts/verify-package.sh net11.0`. No older target is included; adding one requires a concrete consumer need and the same API and wire checks.

The first release covers the core format. Source generators, encryption, blocklist filtering, serializers, ORM integrations, and framework packages are deferred. See [provenance](https://github.com/CodeviaSolutions/tnid.net/blob/main/NOTICE.md), [external-vector instructions](https://github.com/CodeviaSolutions/tnid.net/blob/main/tests/EXTERNAL-VECTORS.md), and [release steps](https://github.com/CodeviaSolutions/tnid.net/blob/main/docs/RELEASING.md).
