using System;

namespace EpPathFinding.cs;

public class GridPos : IEquatable<GridPos>
{
	public int x;

	public int y;

	public GridPos()
	{
		x = 0;
		y = 0;
	}

	public GridPos(int iX, int iY)
	{
		x = iX;
		y = iY;
	}

	public GridPos(GridPos b)
	{
		x = b.x;
		y = b.y;
	}

	public override int GetHashCode()
	{
		return x ^ y;
	}

	public override bool Equals(object obj)
	{
		GridPos gridPos = (GridPos)obj;
		if ((object)gridPos == null)
		{
			return false;
		}
		if (x == gridPos.x)
		{
			return y == gridPos.y;
		}
		return false;
	}

	public bool Equals(GridPos p)
	{
		if ((object)p == null)
		{
			return false;
		}
		if (x == p.x)
		{
			return y == p.y;
		}
		return false;
	}

	public static bool operator ==(GridPos a, GridPos b)
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
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(GridPos a, GridPos b)
	{
		return !(a == b);
	}

	public GridPos Set(int iX, int iY)
	{
		x = iX;
		y = iY;
		return this;
	}

	public override string ToString()
	{
		return $"({x},{y})";
	}
}
