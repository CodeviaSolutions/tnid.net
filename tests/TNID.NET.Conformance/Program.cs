using System.Buffers.Binary;
using System.Globalization;
using TnidNet;

int checks = 0;
void Check(bool condition, string label)
{
    checks++;
    if (!condition) throw new Exception(label);
}
void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { checks++; return; }
    throw new Exception($"Expected {typeof(T).Name}.");
}
// Independent byte/bit-walking oracle; it does not call the production codec.
string ReferenceText(byte[] bytes)
{
    const string names = "\0" + "01234abcdefghijklmnopqrstuvwxyz";
    const string alphabet = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
    int ReadBit(int bit) => bytes[bit / 8] >> (7 - bit % 8) & 1;
    string name = "";
    for (int i = 0; i < 4; i++)
    {
        int n = 0;
        for (int b = 0; b < 5; b++) n = n * 2 + ReadBit(i * 5 + b);
        if (n == 0) break;
        name += names[n];
    }
    string data = "";
    int chunk = 0, count = 0;
    for (int bit = 20; bit < 128; bit++)
    {
        if (bit is >= 48 and <= 51 or 64 or 65) continue;
        chunk = chunk * 2 + ReadBit(bit);
        if (++count % 6 == 0) { data += alphabet[chunk]; chunk = 0; }
    }
    return name + "." + data;
}
Guid Uuid(UInt128 bits)
{
    Span<byte> bytes = stackalloc byte[16];
    BinaryPrimitives.WriteUInt128BigEndian(bytes, bits);
    return new Guid(bytes, bigEndian: true);
}
// Independently authored fixed vectors.
var fixedVectors = new (string Text, string Uuid)[]
{
    ("a.-----------------", "30000000-0000-8000-8000-000000000000"),
    ("0.-----------------", "08000000-0000-8000-8000-000000000000"),
    ("ab.-----------------", "31c00000-0000-8000-8000-000000000000"),
    ("zzzz.zzzzzzzzzzzzzzzzz", "ffffffff-ffff-8fff-bfff-ffffffffffff"),
    ("a.------1----------", "30000000-0000-8000-a000-000000000000"),
    ("a.------2----------", "30000000-0000-8000-b000-000000000000")
};
foreach (var (text, uuid) in fixedVectors)
{
    var id = Tnid.Parse(text);
    Check(id.ToGuid() == Guid.Parse(uuid), "Fixed text -> UUID");
    Check(Tnid.FromGuid(Guid.Parse(uuid)).ToString() == text, "Fixed UUID -> text");
    Check(ReferenceText(id.ToGuid().ToByteArray(bigEndian: true)) == text, "Byte order oracle");
}
string[] invalid = ["", "a", ".-----------------", "aaaaa.-----------------", "A.-----------------",
    "5.-----------------", "a.----------------", "a.------------------", " a.-----------------",
    "a.-----------------\n", "a.---------------=", "a.---------------+", "a.---------------/",
    "a.---------------é", "a.---------------\0", "a..----------------", "a.--------------- "];
foreach (string text in invalid)
{
    Check(!Tnid.TryParse(text, out var bad) && bad == default, "Invalid text accepted");
    Throws<FormatException>(() => Tnid.Parse(text));
}
Check(!Tnid.TryParse(null, out _), "Null TryParse");
Throws<ArgumentNullException>(() => Tnid.Parse(null!));
foreach (string name in new[] { "", "abcde", "A", "5", "a\0", "ä", "a.b" })
{
    Throws<ArgumentException>(() => Tnid.NewV0(name));
    Throws<ArgumentException>(() => Tnid.NewV1(name));
}
Throws<ArgumentNullException>(() => Tnid.NewV1(null!));
// Exhaust all 20-bit name encodings, including every forbidden padding arrangement.
for (uint n = 0; n < (1U << 20); n++)
{
    int[] chars = [(int)(n >> 15), (int)(n >> 10 & 31), (int)(n >> 5 & 31), (int)(n & 31)];
    bool valid = chars[0] != 0;
    for (int i = 1; i < 4; i++) if (chars[i - 1] == 0 && chars[i] != 0) valid = false;
    Check(Tnid.TryFromGuid(Uuid(((UInt128)n << 108) | ((UInt128)8 << 76) | ((UInt128)2 << 62)), out _) == valid,
        "Name padding invariants");
}
UInt128 baseBits = ((UInt128)6 << 123) | ((UInt128)8 << 76) | ((UInt128)2 << 62);
for (int version = 0; version < 16; version++)
for (int variant = 0; variant < 4; variant++)
{
    var uuid = Uuid((baseBits & ~((UInt128)15 << 76) & ~((UInt128)3 << 62)) |
        ((UInt128)(uint)version << 76) | ((UInt128)(uint)variant << 62));
    Check(Tnid.TryFromGuid(uuid, out _) == (version == 8 && variant == 2), "UUID invariants");
}
Check(!Tnid.TryFromGuid(Guid.Empty, out _), "Empty UUID");
Throws<ArgumentException>(() => Tnid.FromGuid(Guid.Empty));
var upper = Tnid.Parse("a.A----------------");
var lower = Tnid.Parse("a.a----------------");
Check(upper != lower && upper < lower, "Exact data casing");
Check(!Tnid.TryParse("A.A----------------", out _), "Exact name casing");
var lead = Tnid<Lead>.NewV1();
Check(Tnid<Lead>.Parse(lead.ToString()) == lead && Tnid<Lead>.FromGuid(lead.ToGuid()) == lead, "Typed round trip");
Check(!Tnid<Partner>.TryParse(lead.ToString(), out var wrong) && wrong == default, "Wrong typed text");
Check(!Tnid<Partner>.TryFromGuid(lead.ToGuid(), out _), "Wrong typed UUID");
Throws<FormatException>(() => Tnid<Partner>.Parse(lead.ToString()));
Throws<ArgumentException>(() => Tnid<Partner>.FromGuid(lead.ToGuid()));
Throws<ArgumentException>(() => Tnid<Lead>.FromTnid(default));
Throws<ArgumentException>(() => Tnid<BadName>.NewV1());
Check(!Tnid<BadName>.TryParse(lead.ToString(), out _), "Invalid marker");
Check(!default(Tnid).IsInitialized && default(Tnid).Name == "" && default(Tnid).ToString() == "", "Default raw");
Check(!default(Tnid<Lead>).IsInitialized && default(Tnid<Lead>).Name == "" && default(Tnid<Lead>).ToString() == "", "Default typed");
Check(default(Tnid) == default && default(Tnid) < (Tnid)lead, "Default equality and ordering");
Check(!default(Tnid<Lead>).Equals((object)default(Tnid<Partner>)), "Typed object equality");
Throws<InvalidOperationException>(() => default(Tnid).ToGuid());
Throws<InvalidOperationException>(() => _ = default(Tnid).Variant);
Throws<InvalidOperationException>(() => default(Tnid<Lead>).ToGuid());
Throws<InvalidOperationException>(() => _ = default(Tnid<Lead>).Variant);
long wrap = 1L << 43;
UInt128 GetBits(Tnid id) => BinaryPrimitives.ReadUInt128BigEndian(id.ToGuid().ToByteArray(bigEndian: true));
UInt128 TimeBits(Tnid id) => ((GetBits(id) >> 80 & 0xFFFFFFF) << 15) |
    ((GetBits(id) >> 64 & 4095) << 3) | (GetBits(id) >> 57 & 7);
foreach (long time in new[] { -1L, 0, 1, 7, 8, 32767, 32768, 1L << 31, wrap - 1, wrap, wrap + 1 })
{
    var id = Tnid.NewV0("time", DateTimeOffset.FromUnixTimeMilliseconds(time));
    Check(TimeBits(id) == (unchecked((ulong)time) & (ulong)(wrap - 1)) && id.Variant == 0, "V0 timestamp/wrap");
}
var last = Tnid.NewV0("time", DateTimeOffset.FromUnixTimeMilliseconds(wrap - 1));
var first = Tnid.NewV0("time", DateTimeOffset.FromUnixTimeMilliseconds(wrap));
Check(last > first, "Wrap resets order");
// Adversarial random tails: latest possible previous millisecond precedes earliest next.
for (long time = 0; time < 10000; time++)
{
    var before = Tnid.NewV0("time", DateTimeOffset.FromUnixTimeMilliseconds(time));
    var after = Tnid.NewV0("time", DateTimeOffset.FromUnixTimeMilliseconds(time + 1));
    UInt128 tail = ((UInt128)1 << 57) - 1;
    var maximumBefore = Tnid.FromGuid(Uuid(GetBits(before) | tail));
    var minimumAfter = Tnid.FromGuid(Uuid(GetBits(after) & ~tail));
    Check(maximumBefore < minimumAfter && string.CompareOrdinal(maximumBefore.ToString(), minimumAfter.ToString()) < 0,
        "V0 order across milliseconds");
}
var sameMillisecond = Enumerable.Range(0, 64).Select(_ => Tnid.NewV0("time", DateTimeOffset.UnixEpoch)).ToArray();
Check(sameMillisecond.Select(TimeBits).Distinct().Count() == 1, "Same timestamp");
Check(sameMillisecond.Distinct().Count() == 64, "Random v0 tails");
Check(sameMillisecond.All(id => GetBits(id) >> 57 == GetBits(sameMillisecond[0]) >> 57), "V0 only tail random");
var generated = new List<Tnid>();
foreach (string name in new[] { "0", "00", "04az", "a", "aa", "ab", "az", "b", "lead", "part", "z", "zzzz" })
for (int i = 0; i < 128; i++)
{
    foreach (var id in new[] { Tnid.NewV0(name), Tnid.NewV1(name) })
    {
        generated.Add(id);
        Check(id.Name == name && Tnid.Parse(id.ToString()) == id && Tnid.FromGuid((Guid)id) == id, "Generated round trip");
        Check(ReferenceText(id.ToGuid().ToByteArray(bigEndian: true)) == id.ToString(), "Reference encoding");
        Check(id.GetHashCode() == Tnid.Parse(id.ToString()).GetHashCode(), "Hash contract");
    }
}
Check(generated.Distinct().Count() == generated.Count, "Generation uniqueness smoke check");
var byValue = generated.Order().Select(x => x.ToString()).ToArray();
Check(byValue.SequenceEqual(generated.Select(x => x.ToString()).Order(StringComparer.Ordinal)), "Ordinal order");
foreach (int variant in new[] { 2, 3 })
for (int bit = 0; bit < 128; bit++)
{
    UInt128 bits = baseBits | ((UInt128)(uint)variant << 60);
    if (bit is >= 60 and <= 63 or >= 76 and <= 79 or >= 108) continue;
    var id = Tnid.FromGuid(Uuid(bits | (UInt128)1 << bit));
    Check(Tnid.Parse(id.ToString()).ToGuid() == id.ToGuid() && id.Variant == variant, "Reserved round trip");
}
// Optional externally obtained vectors: tab-separated rows; no fixture redistribution.
if (args.Length > 0)
{
    if (Path.GetExtension(args[0]).Equals(".json", StringComparison.OrdinalIgnoreCase))
    {
        OfficialVectors.Run(args[0], Check);
    }
    else
    {
    int external = 0;
    foreach (string line in File.ReadLines(args[0]))
    {
        if (line.StartsWith('#') || line.Length == 0) continue;
        string[] fields = line.Split('\t');
        switch (fields[0])
        {
            case "valid" when fields.Length == 3:
                Check(Tnid.Parse(fields[1]).ToGuid() == Guid.Parse(fields[2]), "External valid text");
                Check(Tnid.FromGuid(Guid.Parse(fields[2])).ToString() == fields[1], "External valid UUID");
                break;
            case "invalid-text" when fields.Length == 2:
                Check(!Tnid.TryParse(fields[1], out _), "External invalid text"); break;
            case "invalid-uuid" when fields.Length == 2:
                Check(!Tnid.TryFromGuid(Guid.Parse(fields[1]), out _), "External invalid UUID"); break;
            default: throw new Exception("Unrecognized external vector row.");
        }
        external++;
    }
    Check(external > 0, "External vector file is empty");
    Console.WriteLine($"External vectors: {external}.");
    }
}
Console.WriteLine($"PASS: {checks:N0} checks ({System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}).");
readonly struct Lead : ITnidName { public static string Name => "lead"; }
readonly struct Partner : ITnidName { public static string Name => "part"; }
readonly struct BadName : ITnidName { public static string Name => "BAD"; }
