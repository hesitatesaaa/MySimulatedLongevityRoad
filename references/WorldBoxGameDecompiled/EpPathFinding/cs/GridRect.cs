namespace EpPathFinding.cs;

public class GridRect
{
	public int minX;

	public int minY;

	public int maxX;

	public int maxY;

	public GridRect()
	{
		minX = 0;
		minY = 0;
		maxX = 0;
		maxY = 0;
	}

	public GridRect(int iMinX, int iMinY, int iMaxX, int iMaxY)
	{
		minX = iMinX;
		minY = iMinY;
		maxX = iMaxX;
		maxY = iMaxY;
	}

	public GridRect(GridRect b)
	{
		minX = b.minX;
		minY = b.minY;
		maxX = b.maxX;
		maxY = b.maxY;
	}

	public override int GetHashCode()
	{
		return minX ^ minY ^ maxX ^ maxY;
	}

	public override bool Equals(object obj)
	{
		GridRect gridRect = (GridRect)obj;
		if ((object)gridRect == null)
		{
			return false;
		}
		if (minX == gridRect.minX && minY == gridRect.minY && maxX == gridRect.maxX)
		{
			return maxY == gridRect.maxY;
		}
		return false;
	}

	public bool Equals(GridRect p)
	{
		if ((object)p == null)
		{
			return false;
		}
		if (minX == p.minX && minY == p.minY && maxX == p.maxX)
		{
			return maxY == p.maxY;
		}
		return false;
	}

	public static bool operator ==(GridRect a, GridRect b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null)
		{
			return false;
		}
		if ((object)b == null)
		{
			return false;
		}
		if (a.minX == b.minX && a.minY == b.minY && a.maxX == b.maxX)
		{
			return a.maxY == b.maxY;
		}
		return false;
	}

	public static bool operator !=(GridRect a, GridRect b)
	{
		return !(a == b);
	}

	public GridRect Set(int iMinX, int iMinY, int iMaxX, int iMaxY)
	{
		minX = iMinX;
		minY = iMinY;
		maxX = iMaxX;
		maxY = iMaxY;
		return this;
	}
}
