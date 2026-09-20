using System;
using System.Collections.Generic;

namespace EpPathFinding.cs;

public abstract class BaseGrid : IDisposable
{
	public readonly List<Node> closedList = new List<Node>();

	public int closed_list_count;

	public const int CLOSED_LIST_MINIMUM_ELEMENTS = 10;

	protected GridRect m_gridRect;

	public GridRect gridRect => m_gridRect;

	public abstract int width { get; protected set; }

	public abstract int height { get; protected set; }

	public BaseGrid()
	{
		m_gridRect = new GridRect();
	}

	public BaseGrid(BaseGrid b)
	{
		m_gridRect = new GridRect(b.m_gridRect);
		width = b.width;
		height = b.height;
	}

	public abstract Node GetNodeAt(int iX, int iY);

	public abstract bool IsWalkableAt(int iX, int iY);

	public abstract bool SetWalkableAt(int iX, int iY, bool iWalkable, int pCost = 1);

	public abstract Node GetNodeAt(GridPos iPos);

	public abstract bool IsWalkableAt(GridPos iPos);

	public abstract bool SetWalkableAt(GridPos iPos, bool iWalkable);

	public List<Node> GetNeighbors(Node iNode, DiagonalMovement diagonalMovement)
	{
		int x = iNode.x;
		int y = iNode.y;
		List<Node> list = new List<Node>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		bool flag6 = false;
		bool flag7 = false;
		bool flag8 = false;
		GridPos gridPos = new GridPos();
		if (IsWalkableAt(gridPos.Set(x, y - 1)))
		{
			list.Add(GetNodeAt(gridPos));
			flag = true;
		}
		if (IsWalkableAt(gridPos.Set(x + 1, y)))
		{
			list.Add(GetNodeAt(gridPos));
			flag3 = true;
		}
		if (IsWalkableAt(gridPos.Set(x, y + 1)))
		{
			list.Add(GetNodeAt(gridPos));
			flag5 = true;
		}
		if (IsWalkableAt(gridPos.Set(x - 1, y)))
		{
			list.Add(GetNodeAt(gridPos));
			flag7 = true;
		}
		switch (diagonalMovement)
		{
		case DiagonalMovement.Always:
			flag2 = true;
			flag4 = true;
			flag6 = true;
			flag8 = true;
			break;
		case DiagonalMovement.IfAtLeastOneWalkable:
			flag2 = flag7 | flag;
			flag4 = flag | flag3;
			flag6 = flag3 | flag5;
			flag8 = flag5 | flag7;
			break;
		case DiagonalMovement.OnlyWhenNoObstacles:
			flag2 = flag7 & flag;
			flag4 = flag & flag3;
			flag6 = flag3 & flag5;
			flag8 = flag5 & flag7;
			break;
		}
		if (flag2 && IsWalkableAt(gridPos.Set(x - 1, y - 1)))
		{
			list.Add(GetNodeAt(gridPos));
		}
		if (flag4 && IsWalkableAt(gridPos.Set(x + 1, y - 1)))
		{
			list.Add(GetNodeAt(gridPos));
		}
		if (flag6 && IsWalkableAt(gridPos.Set(x + 1, y + 1)))
		{
			list.Add(GetNodeAt(gridPos));
		}
		if (flag8 && IsWalkableAt(gridPos.Set(x - 1, y + 1)))
		{
			list.Add(GetNodeAt(gridPos));
		}
		return list;
	}

	public void addToClosed(Node pNode)
	{
		closedList.Add(pNode);
		closed_list_count++;
	}

	public abstract void Reset();

	public abstract BaseGrid Clone();

	public virtual void Dispose()
	{
		foreach (Node closed in closedList)
		{
			closed.Dispose();
		}
		closedList.Clear();
		closed_list_count = 0;
		m_gridRect = null;
	}
}
