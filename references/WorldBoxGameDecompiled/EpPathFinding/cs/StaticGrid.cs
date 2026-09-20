using System.Runtime.CompilerServices;

namespace EpPathFinding.cs;

public class StaticGrid : BaseGrid
{
	public Node[][] m_nodes;

	public override int width { get; protected set; }

	public override int height { get; protected set; }

	public StaticGrid(int iWidth, int iHeight, bool[][] iMatrix = null)
	{
		width = iWidth;
		height = iHeight;
		m_gridRect.minX = 0;
		m_gridRect.minY = 0;
		m_gridRect.maxX = iWidth - 1;
		m_gridRect.maxY = iHeight - 1;
		m_nodes = buildNodes(iWidth, iHeight, iMatrix);
	}

	public StaticGrid(StaticGrid b)
		: base(b)
	{
		bool[][] array = new bool[b.width][];
		for (int i = 0; i < b.width; i++)
		{
			array[i] = new bool[b.height];
			for (int j = 0; j < b.height; j++)
			{
				if (b.IsWalkableAt(i, j))
				{
					array[i][j] = true;
				}
				else
				{
					array[i][j] = false;
				}
			}
		}
		m_nodes = buildNodes(b.width, b.height, array);
	}

	private Node[][] buildNodes(int iWidth, int iHeight, bool[][] iMatrix)
	{
		Node[][] array = new Node[iWidth][];
		for (int i = 0; i < iWidth; i++)
		{
			array[i] = new Node[iHeight];
			for (int j = 0; j < iHeight; j++)
			{
				array[i][j] = new Node(i, j);
			}
		}
		return array;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override Node GetNodeAt(int iX, int iY)
	{
		return m_nodes[iX][iY];
	}

	public override bool IsWalkableAt(int iX, int iY)
	{
		return isInside(iX, iY);
	}

	protected bool isInside(int iX, int iY)
	{
		if (iX >= 0 && iX < width)
		{
			if (iY >= 0)
			{
				return iY < height;
			}
			return false;
		}
		return false;
	}

	public override bool SetWalkableAt(int iX, int iY, bool iWalkable, int pCost = 1)
	{
		return true;
	}

	public void SetTileNode(int iX, int iY, WorldTile pTile)
	{
		m_nodes[iX][iY].tile = pTile;
	}

	protected bool isInside(GridPos iPos)
	{
		return isInside(iPos.x, iPos.y);
	}

	public override Node GetNodeAt(GridPos iPos)
	{
		return GetNodeAt(iPos.x, iPos.y);
	}

	public override bool IsWalkableAt(GridPos iPos)
	{
		return IsWalkableAt(iPos.x, iPos.y);
	}

	public override bool SetWalkableAt(GridPos iPos, bool iWalkable)
	{
		return SetWalkableAt(iPos.x, iPos.y, iWalkable);
	}

	public override void Reset()
	{
		Reset(null);
	}

	public void Reset(bool[][] iMatrix)
	{
		foreach (Node closed in closedList)
		{
			closed.Reset();
		}
		closedList.Clear();
		closed_list_count = 0;
	}

	public override BaseGrid Clone()
	{
		int num = width;
		int num2 = height;
		_ = m_nodes;
		StaticGrid staticGrid = new StaticGrid(num, num2);
		Node[][] array = new Node[num][];
		for (int i = 0; i < num; i++)
		{
			array[i] = new Node[num2];
			for (int j = 0; j < num2; j++)
			{
				array[i][j] = new Node(i, j, false);
			}
		}
		staticGrid.m_nodes = array;
		return staticGrid;
	}

	public override void Dispose()
	{
		Node[][] nodes = m_nodes;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				nodes[i][j].Dispose();
			}
		}
		m_nodes = null;
		base.Dispose();
	}
}
