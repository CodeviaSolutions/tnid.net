using TnidNet;

AcceptLead(Tnid<Partner>.NewV1()); // Must fail with CS1503.
static void AcceptLead(Tnid<Lead> value) { }
readonly struct Lead : ITnidName { public static string Name => "lead"; }
readonly struct Partner : ITnidName { public static string Name => "part"; }
