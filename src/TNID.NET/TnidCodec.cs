using System.Buffers.Binary;
using System.Security.Cryptography;

namespace TnidNet;

// Bit positions are measured from the least significant end of a big-endian UUID.
internal static class TnidCodec
{
    private const string Names = "\0" + "01234abcdefghijklmnopqrstuvwxyz";
    private const string Data = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
    private static readonly UInt128 Low60 = ((UInt128)1 << 60) - 1;
    private static readonly UInt128 Low62 = ((UInt128)1 << 62) - 1;
    private const ulong TimeMask = (1UL << 43) - 1;

    internal static bool TryEncodeName(ReadOnlySpan<char> name, out uint encoded)
    {
        encoded = 0;
        if (name.Length is < 1 or > 4) return false;
        foreach (char c in name)
        {
            int index = Names.IndexOf(c);
            if (index <= 0) return false;
            encoded = (encoded << 5) | (uint)index;
        }
        encoded <<= (4 - name.Length) * 5;
        return true;
    }

    internal static bool IsValid(UInt128 bits)
    {
        if (((bits >> 76) & 15) != 8 || ((bits >> 62) & 3) != 2) return false;
        uint name = (uint)(bits >> 108);
        if (name == 0) return false;
        bool padding = false;
        for (int shift = 15; shift >= 0; shift -= 5)
        {
            uint c = (name >> shift) & 31;
            if (c == 0) padding = true;
            else if (padding) return false;
        }
        return true;
    }

    internal static string Name(UInt128 bits)
    {
        Span<char> chars = stackalloc char[4];
        int count = 0;
        for (int shift = 123; shift >= 108; shift -= 5)
        {
            int c = (int)((bits >> shift) & 31);
            if (c == 0) break;
            chars[count++] = Names[c];
        }
        return new string(chars[..count]);
    }

    internal static bool TryParse(ReadOnlySpan<char> text, out UInt128 bits)
    {
        bits = 0;
        int nameLength = text.Length - 18;
        if (nameLength is < 1 or > 4 || text[nameLength] != '.' ||
            !TryEncodeName(text[..nameLength], out uint name)) return false;
        UInt128 data = 0;
        foreach (char c in text[(nameLength + 1)..])
        {
            int index = Data.IndexOf(c);
            if (index < 0) return false;
            data = (data << 6) | (uint)index;
        }
        bits = Expand(name, data);
        return true;
    }

    private static UInt128 Expand(uint name, UInt128 data) =>
        ((UInt128)name << 108) | ((data >> 74) << 80) |
        ((UInt128)8 << 76) | (((data >> 62) & 4095) << 64) |
        ((UInt128)2 << 62) | (data & Low62);

    internal static string Format(UInt128 bits)
    {
        string name = Name(bits);
        Span<char> result = stackalloc char[name.Length + 18];
        name.AsSpan().CopyTo(result);
        result[name.Length] = '.';
        UInt128 data = (((bits >> 80) & 0xFFFFFFF) << 74) |
            (((bits >> 64) & 4095) << 62) | (bits & Low62);
        for (int i = result.Length - 1; i > name.Length; i--)
        {
            result[i] = Data[(int)(data & 63)];
            data >>= 6;
        }
        return new string(result);
    }

    internal static UInt128 Generate(string name, long? unixMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (!TryEncodeName(name, out uint encoded))
            throw new ArgumentException("A TNID name must contain 1–4 characters from 0–4 and a–z.", nameof(name));
        Span<byte> random = stackalloc byte[16];
        RandomNumberGenerator.Fill(random);
        UInt128 entropy = BinaryPrimitives.ReadUInt128BigEndian(random);
        UInt128 data;
        if (unixMilliseconds is long time)
        {
            ulong timestamp = unchecked((ulong)time) & TimeMask;
            // First 40 timestamp bits precede the TNID variant; last 3 follow it.
            data = ((UInt128)(timestamp >> 3) << 62) |
                ((UInt128)(timestamp & 7) << 57) | (entropy & (((UInt128)1 << 57) - 1));
        }
        else
        {
            data = ((entropy >> 60 & (((UInt128)1 << 40) - 1)) << 62) |
                ((UInt128)1 << 60) | (entropy & Low60);
        }
        return Expand(encoded, data);
    }
}
