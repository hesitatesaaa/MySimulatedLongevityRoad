namespace MySimulatedLongevityRoad.Core;

internal static class MclslStableHash
{
    internal static int Positive43x53(string value)
    {
        unchecked { int result = 43; foreach (char c in value ?? string.Empty) result = result * 53 + c; return result & int.MaxValue; }
    }
    internal static int Positive79x43(string value)
    {
        unchecked { int result = 79; foreach (char c in value ?? string.Empty) result = result * 43 + c; return result & int.MaxValue; }
    }
    internal static int Positive23x37(string value)
    {
        unchecked { int result = 23; foreach (char c in value ?? string.Empty) result = result * 37 + c; return result & int.MaxValue; }
    }
    internal static int Positive83x47(string value)
    {
        unchecked { int result = 83; foreach (char c in value ?? string.Empty) result = result * 47 + c; return result & int.MaxValue; }
    }
    internal static int Positive31x37(string value)
    {
        unchecked { int result = 31; foreach (char c in value ?? string.Empty) result = result * 37 + c; return result & int.MaxValue; }
    }
    internal static int Stable17x31(string value)
    {
        unchecked { int hash = 17; foreach (char c in value ?? string.Empty) hash = hash * 31 + c; return hash; }
    }
}
