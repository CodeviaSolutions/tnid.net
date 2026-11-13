using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using TnidNet;

internal static class OfficialVectors
{
    internal const string ExpectedSha256 = "cd2609f5abae0ad1c4bad010d9a88f03160046086348ad05d348e9a2f890b65c";

    internal static void Run(string path, Action<bool, string> check)
    {
        byte[] json = File.ReadAllBytes(path);
        string hash = Convert.ToHexStringLower(SHA256.HashData(json));
        check(hash == ExpectedSha256, "Official fixture SHA-256 mismatch");
        using var document = JsonDocument.Parse(json);
        JsonElement root = document.RootElement;
        check(root.GetProperty("spec_version").GetInt32() == 1, "Official spec version");
        string[] sections = ["spec_version", "generated", "source", "notes", "names", "variant_0", "variant_1",
            "round_trips", "uuid_case_insensitive", "reject_strings", "reject_uuids", "left_open", "encryption"];
        foreach (var property in root.EnumerateObject())
            check(sections.Contains(property.Name), $"Unreviewed official section: {property.Name}");

        static string Text(JsonElement row, string key) => row.GetProperty(key).GetString()!;
        static UInt128 Hex(string text) => UInt128.Parse("0" + text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
        static UInt128 Bits(Tnid id) => BinaryPrimitives.ReadUInt128BigEndian(id.ToGuid().ToByteArray(bigEndian: true));
        static Guid Uuid(UInt128 bits)
        {
            Span<byte> bytes = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128BigEndian(bytes, bits);
            return new Guid(bytes, bigEndian: true);
        }
        void RoundTrip(JsonElement row)
        {
            string text = Text(row, "string");
            Guid uuid = Guid.Parse(Text(row, "uuid"));
            check(Tnid.Parse(text).ToGuid() == uuid, $"Official text -> UUID: {text}");
            check(Tnid.FromGuid(uuid).ToString() == text, $"Official UUID -> text: {text}");
        }

        var names = new Dictionary<string, UInt128>(StringComparer.Ordinal);
        foreach (var row in root.GetProperty("names").EnumerateArray())
        {
            string name = Text(row, "name");
            UInt128 encoded = Hex(Text(row, "bits"));
            names.Add(name, encoded);
            check(Bits(Tnid.NewV1(name)) >> 108 == encoded, $"Official name bits: {name}");
            var decoded = Tnid.FromGuid(Uuid((encoded << 108) | ((UInt128)8 << 76) | ((UInt128)2 << 62)));
            check(decoded.Name == name, $"Official name padding: {name}");
        }
        int core = names.Count;
        foreach (var row in root.GetProperty("variant_0").EnumerateArray())
        {
            RoundTrip(row);
            string name = Text(row, "name");
            long ms = row.GetProperty("ms").GetInt64();
            ulong stored = unchecked((ulong)ms) & ((1UL << 43) - 1);
            if (row.TryGetProperty("stored_ms", out var wrapped))
                check(stored == wrapped.GetUInt64(), "Official wrapped timestamp");
            UInt128 random = Hex(Text(row, "random"));
            UInt128 expected = (names[name] << 108) | ((UInt128)(stored >> 15) << 80) |
                ((UInt128)8 << 76) | ((UInt128)(stored >> 3 & 4095) << 64) |
                ((UInt128)2 << 62) | ((UInt128)(stored & 7) << 57) | random;
            check(Uuid(expected) == Guid.Parse(Text(row, "uuid")), "Official deterministic v0 layout");
            var generated = Tnid.NewV0(name, DateTimeOffset.FromUnixTimeMilliseconds(ms));
            check(Bits(generated) >> 57 == expected >> 57, "Official v0 generation timestamp/name/version/variants");
            core++;
        }
        foreach (var row in root.GetProperty("variant_1").EnumerateArray())
        {
            RoundTrip(row);
            string name = Text(row, "name");
            UInt128 payload = Hex(Text(row, "payload"));
            UInt128 nameBits = 0;
            foreach (char c in name)
                nameBits = (nameBits << 5) | (uint)(c is >= '0' and <= '4' ? c - '0' + 1 : c - 'a' + 6);
            nameBits <<= (4 - name.Length) * 5;
            UInt128 expected = (nameBits << 108) | ((payload >> 72) << 80) |
                ((UInt128)8 << 76) | (((payload >> 60) & 4095) << 64) |
                ((UInt128)2 << 62) | ((UInt128)1 << 60) | (payload & (((UInt128)1 << 60) - 1));
            check(Uuid(expected) == Guid.Parse(Text(row, "uuid")), "Official deterministic v1 layout");
            check(Tnid.Parse(Text(row, "string")).Variant == 1, "Official v1 variant");
            core++;
        }
        foreach (var row in root.GetProperty("round_trips").EnumerateArray())
        {
            RoundTrip(row);
            check(Tnid.Parse(Text(row, "string")).Variant == row.GetProperty("variant").GetInt32(), "Official reserved variant");
            core++;
        }
        foreach (var row in root.GetProperty("uuid_case_insensitive").EnumerateArray())
        {
            RoundTrip(row); // Guid.Parse is the explicit application's UUID-text boundary.
            core++;
        }
        foreach (var row in root.GetProperty("reject_strings").EnumerateArray())
        {
            string input = Text(row, "input");
            check(!Tnid.TryParse(input, out var value) && value == default, $"Official invalid text: {Text(row, "reason")}");
            bool threw = false;
            try { Tnid.Parse(input); } catch (FormatException) { threw = true; }
            check(threw, "Official invalid text Parse must throw");
            core++;
        }
        int malformedUuids = 0;
        foreach (var row in root.GetProperty("reject_uuids").EnumerateArray())
        {
            string input = Text(row, "input");
            if (!Guid.TryParse(input, out Guid uuid))
            {
                malformedUuids++; // No UUID-text parser is shipped by TNID.NET.
            }
            else
            {
                check(!Tnid.TryFromGuid(uuid, out var value) && value == default, $"Official invalid TNID UUID: {Text(row, "reason")}");
                bool threw = false;
                try { Tnid.FromGuid(uuid); } catch (ArgumentException) { threw = true; }
                check(threw, "Official invalid UUID FromGuid must throw");
            }
            check(!Tnid.TryParse(input, out _), "Official UUID input is never canonical TNID text");
            core++;
        }
        int leftOpen = 0;
        foreach (var row in root.GetProperty("left_open").EnumerateArray())
        {
            string input = Text(row, "input");
            check(!Tnid.TryParse(input, out _), "Documented canonical-only parser rejects alternate UUID text");
            Console.WriteLine($"left_open: canonical parser rejects {JsonSerializer.Serialize(input)}; Guid.TryParse={Guid.TryParse(input, out _)}");
            leftOpen++;
        }
        int encryption = root.GetProperty("encryption").GetArrayLength();
        Console.WriteLine($"Official fixture: https://tnid.info/test-vectors.json; SHA-256 {hash}");
        Console.WriteLine($"PASS: {core} core rows (including {malformedUuids} malformed UUID rows rejected at the BCL boundary); {leftOpen} left_open choices verified.");
        Console.WriteLine($"SKIP: {encryption} optional encryption rows; encryption is outside this core-only library.");
    }
}
