using System;
using Newtonsoft.Json;

[Serializable]
[JsonConverter(typeof(BrushPixelDataConverter))]
public readonly struct BrushPixelData(int pX, int pY, int pDist) : IEquatable<BrushPixelData>
{
	public readonly int x = pX;

	public readonly int y = pY;

	public readonly int dist = pDist;

	public bool Equals(BrushPixelData pOther)
	{
		if (x == pOther.x)
		{
			return y == pOther.y;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return x * 100000 + y;
	}
}
