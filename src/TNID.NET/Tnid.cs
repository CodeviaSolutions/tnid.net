using System.Buffers.Binary;

namespace TnidNet;

/// <summary>An immutable TNID 1.0 value. The default value is an uninitialized sentinel.</summary>
public readonly struct Tnid : IEquatable<Tnid>, IComparable<Tnid>
{
    private readonly UInt128 _bits;
    private Tnid(UInt128 bits) => _bits = bits;

    public bool IsInitialized => _bits != 0;
    public string Name => TnidCodec.Name(_bits);
    public int Variant { get { EnsureInitialized(); return (int)((_bits >> 60) & 3); } }

    public static Tnid NewV0(string name) => NewV0(name, DateTimeOffset.UtcNow);
    /// <summary>Creates v0 using the supplied time, modulo 2^43 Unix milliseconds.</summary>
    public static Tnid NewV0(string name, DateTimeOffset timestamp) =>
        new(TnidCodec.Generate(name, timestamp.ToUnixTimeMilliseconds()));
    public static Tnid NewV1(string name) => new(TnidCodec.Generate(name, null));

    public static Tnid Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!TryParse(text, out var value)) throw new FormatException("Invalid canonical TNID text.");
        return value;
    }

    public static bool TryParse(string? text, out Tnid value)
    {
        value = default;
        if (!TnidCodec.TryParse(text.AsSpan(), out var bits)) return false;
        value = new(bits);
        return true;
    }

    public static Tnid FromGuid(Guid guid)
    {
        if (!TryFromGuid(guid, out var value)) throw new ArgumentException("The UUID is not a TNID.", nameof(guid));
        return value;
    }

    public static bool TryFromGuid(Guid guid, out Tnid value)
    {
        value = default;
        Span<byte> bytes = stackalloc byte[16];
        guid.TryWriteBytes(bytes, bigEndian: true, out _);
        UInt128 bits = BinaryPrimitives.ReadUInt128BigEndian(bytes);
        if (!TnidCodec.IsValid(bits)) return false;
        value = new(bits);
        return true;
    }

    public Guid ToGuid()
    {
        EnsureInitialized();
        Span<byte> bytes = stackalloc byte[16];
        BinaryPrimitives.WriteUInt128BigEndian(bytes, _bits);
        return new Guid(bytes, bigEndian: true);
    }

    private void EnsureInitialized()
    {
        if (!IsInitialized) throw new InvalidOperationException("The default TNID is uninitialized.");
    }

    public override string ToString() => IsInitialized ? TnidCodec.Format(_bits) : string.Empty;
    // The encoding alphabets and trailing zero name padding preserve ordinal text order.
    public int CompareTo(Tnid other) => _bits.CompareTo(other._bits);
    public bool Equals(Tnid other) => _bits == other._bits;
    public override bool Equals(object? obj) => obj is Tnid other && Equals(other);
    public override int GetHashCode() => _bits.GetHashCode();
    public static explicit operator Guid(Tnid value) => value.ToGuid();
    public static explicit operator Tnid(Guid value) => FromGuid(value);
    public static bool operator ==(Tnid left, Tnid right) => left.Equals(right);
    public static bool operator !=(Tnid left, Tnid right) => !left.Equals(right);
    public static bool operator <(Tnid left, Tnid right) => left.CompareTo(right) < 0;
    public static bool operator >(Tnid left, Tnid right) => left.CompareTo(right) > 0;
    public static bool operator <=(Tnid left, Tnid right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Tnid left, Tnid right) => left.CompareTo(right) >= 0;
}
