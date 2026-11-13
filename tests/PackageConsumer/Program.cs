using System.Runtime.InteropServices;
using TnidNet;

foreach (var id in new[] { Tnid<Lead>.NewV0(), Tnid<Lead>.NewV1() })
{
    string text = id.ToString();
    Guid uuid = (Guid)id;
    if (Tnid<Lead>.Parse(text) != id || (Tnid<Lead>)uuid != id)
        throw new Exception("Packed library round trip failed.");
    if (Tnid<Partner>.TryFromGuid(uuid, out _)) throw new Exception("Wrong name accepted.");
}
foreach (string text in new[] { "a.------1----------", "a.------2----------" })
{
    var id = Tnid.Parse(text);
    if (Tnid.FromGuid(id.ToGuid()).ToString() != text || id.Variant < 2)
        throw new Exception("Reserved variant lost.");
}
if (default(Tnid).IsInitialized || default(Tnid).ToString() != "") throw new Exception("Default contract changed.");
Console.WriteLine($"Packed TNID.NET 0.1.0 consumer passed on {RuntimeInformation.FrameworkDescription}.");
readonly struct Lead : ITnidName { public static string Name => "lead"; }
readonly struct Partner : ITnidName { public static string Name => "part"; }
