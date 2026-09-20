using System;

namespace EpPathFinding.cs;

public class Node : IComparable<Node>, IDisposable
{
	public WorldTile tile;

	public readonly int x;

	public readonly int y;

	public float heuristicStartToEndLen;

	public float startToCurNodeLen;

	public float? heuristicCurNodeToEndLen;

	public bool isOpened;

	public bool isClosed;

	public Node parent;

	public Node(int iX, int iY, bool? iWalkable = null)
	{
		x = iX;
		y = iY;
		heuristicStartToEndLen = 0f;
		startToCurNodeLen = 0f;
		heuristicCurNodeToEndLen = null;
		isOpened = false;
		isClosed = false;
		parent = null;
	}

	public Node(Node b)
	{
		x = b.x;
		y = b.y;
		heuristicStartToEndLen = b.heuristicStartToEndLen;
		startToCurNodeLen = b.startToCurNodeLen;
		heuristicCurNodeToEndLen = b.heuristicCurNodeToEndLen;
		isOpened = b.isOpened;
		isClosed = b.isClosed;
		parent = b.parent;
	}

	public void Reset(bool? iWalkable = null)
	{
		heuristicStartToEndLen = 0f;
		startToCurNodeLen = 0f;
		heuristicCurNodeToEndLen = null;
		isOpened = false;
		isClosed = false;
		parent = null;
	}

	public int CompareTo(Node iObj)
	{
		float num = heuristicStartToEndLen - iObj.heuristicStartToEndLen;
		if (num > 0f)
		{
			return 1;
		}
		if (num == 0f)
		{
			return 0;
		}
		return -1;
	}

	public override int GetHashCode()
	{
		return x ^ y;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (!(obj is Node node))
		{
			return false;
		}
		if (x == node.x)
		{
			return y == node.y;
		}
		return false;
	}

	public bool Equals(Node p)
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

	public static bool operator ==(Node a, Node b)
	{
		if ((object)a == b)
		{
			return true;
		}
		if ((object)a == null || (object)b == null)
		{
			return false;
		}
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public static bool operator !=(Node a, Node b)
	{
		return !(a == b);
	}

	public void Dispose()
	{
		tile = null;
		parent = null;
	}
}
