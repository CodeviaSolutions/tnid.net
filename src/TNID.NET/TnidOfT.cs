namespace TnidNet;

/// <summary>A marker with a stable, canonical TNID name.</summary>
public interface ITnidName
{
    static abstract string Name { get; }
}

/// <summary>A TNID whose name is checked against a static marker.</summary>
public readonly struct Tnid<TName> : IEquatable<Tnid<TName>>, IComparable<Tnid<TName>>
    where TName : ITnidName
{
    private readonly Tnid _value;
    private Tnid(Tnid value) => _value = value;
    public bool IsInitialized => _value.IsInitialized;
    public string Name => _value.Name;
    public int Variant => _value.Variant;

    public static Tnid<TName> NewV0() => new(Tnid.NewV0(TName.Name));
    public static Tnid<TName> NewV0(DateTimeOffset timestamp) => new(Tnid.NewV0(TName.Name, timestamp));
    public static Tnid<TName> NewV1() => new(Tnid.NewV1(TName.Name));
    public static Tnid<TName> FromTnid(Tnid value)
    {
        if (!Matches(value)) throw new ArgumentException("The TNID does not match the marker name.", nameof(value));
        return new(value);
    }
    private static bool Matches(Tnid value) => value.IsInitialized &&
        TnidCodec.TryEncodeName(TName.Name, out _) && string.Equals(value.Name, TName.Name, StringComparison.Ordinal);
    public static Tnid<TName> Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (!TryParse(text, out var value)) throw new FormatException("Invalid TNID or wrong marker name.");
        return value;
    }
    public static bool TryParse(string? text, out Tnid<TName> value)
    {
        value = default;
        if (!Tnid.TryParse(text, out var raw) || !Matches(raw)) return false;
        value = new(raw);
        return true;
    }
    public static Tnid<TName> FromGuid(Guid guid) => FromTnid(Tnid.FromGuid(guid));
    public static bool TryFromGuid(Guid guid, out Tnid<TName> value)
    {
        value = default;
        if (!Tnid.TryFromGuid(guid, out var raw) || !Matches(raw)) return false;
        value = new(raw);
        return true;
    }
    public Tnid ToTnid() => _value;
    public Guid ToGuid() => _value.ToGuid();
    public override string ToString() => _value.ToString();
    public bool Equals(Tnid<TName> other) => _value.Equals(other._value);
    public override bool Equals(object? obj) => obj is Tnid<TName> other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public int CompareTo(Tnid<TName> other) => _value.CompareTo(other._value);
    public static explicit operator Tnid(Tnid<TName> value) => value.ToTnid();
    public static explicit operator Tnid<TName>(Tnid value) => FromTnid(value);
    public static explicit operator Guid(Tnid<TName> value) => value.ToGuid();
    public static explicit operator Tnid<TName>(Guid value) => FromGuid(value);
    public static bool operator ==(Tnid<TName> left, Tnid<TName> right) => left.Equals(right);
    public static bool operator !=(Tnid<TName> left, Tnid<TName> right) => !left.Equals(right);
    public static bool operator <(Tnid<TName> left, Tnid<TName> right) => left.CompareTo(right) < 0;
    public static bool operator >(Tnid<TName> left, Tnid<TName> right) => left.CompareTo(right) > 0;
    public static bool operator <=(Tnid<TName> left, Tnid<TName> right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Tnid<TName> left, Tnid<TName> right) => left.CompareTo(right) >= 0;
}
