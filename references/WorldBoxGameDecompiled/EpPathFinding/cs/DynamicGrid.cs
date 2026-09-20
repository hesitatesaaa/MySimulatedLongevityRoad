using System.Collections.Generic;

namespace EpPathFinding.cs;

public class DynamicGrid : BaseGrid
{
	protected Dictionary<GridPos, Node> m_nodes;

	private bool m_notSet;

	public override int width
	{
		get
		{
			if (m_notSet)
			{
				setBoundingBox();
			}
			return m_gridRect.maxX - m_gridRect.minX + 1;
		}
		protected set
		{
		}
	}

	public override int height
	{
		get
		{
			if (m_notSet)
			{
				setBoundingBox();
			}
			return m_gridRect.maxY - m_gridRect.minY + 1;
		}
		protected set
		{
		}
	}

	public DynamicGrid(List<GridPos> iWalkableGridList = null)
	{
		m_gridRect = new GridRect();
		m_gridRect.minX = 0;
		m_gridRect.minY = 0;
		m_gridRect.maxX = 0;
		m_gridRect.maxY = 0;
		m_notSet = true;
		buildNodes(iWalkableGridList);
	}

	public DynamicGrid(DynamicGrid b)
		: base(b)
	{
		m_notSet = b.m_notSet;
		m_nodes = new Dictionary<GridPos, Node>(b.m_nodes);
	}

	protected void buildNodes(List<GridPos> iWalkableGridList)
	{
		m_nodes = new Dictionary<GridPos, Node>();
		if (iWalkableGridList == null)
		{
			return;
		}
		foreach (GridPos iWalkableGrid in iWalkableGridList)
		{
			SetWalkableAt(iWalkableGrid.x, iWalkableGrid.y, iWalkable: true);
		}
	}

	public override Node GetNodeAt(int iX, int iY)
	{
		GridPos iPos = new GridPos(iX, iY);
		return GetNodeAt(iPos);
	}

	public override bool IsWalkableAt(int iX, int iY)
	{
		GridPos iPos = new GridPos(iX, iY);
		return IsWalkableAt(iPos);
	}

	private void setBoundingBox()
	{
		m_notSet = true;
		foreach (KeyValuePair<GridPos, Node> node in m_nodes)
		{
			if (node.Key.x < m_gridRect.minX || m_notSet)
			{
				m_gridRect.minX = node.Key.x;
			}
			if (node.Key.x > m_gridRect.maxX || m_notSet)
			{
				m_gridRect.maxX = node.Key.x;
			}
			if (node.Key.y < m_gridRect.minY || m_notSet)
			{
				m_gridRect.minY = node.Key.y;
			}
			if (node.Key.y > m_gridRect.maxY || m_notSet)
			{
				m_gridRect.maxY = node.Key.y;
			}
			m_notSet = false;
		}
		m_notSet = false;
	}

	public override bool SetWalkableAt(int iX, int iY, bool iWalkable, int pCost = 1)
	{
		GridPos gridPos = new GridPos(iX, iY);
		if (iWalkable)
		{
			if (m_nodes.ContainsKey(gridPos))
			{
				return true;
			}
			if (iX < m_gridRect.minX || m_notSet)
			{
				m_gridRect.minX = iX;
			}
			if (iX > m_gridRect.maxX || m_notSet)
			{
				m_gridRect.maxX = iX;
			}
			if (iY < m_gridRect.minY || m_notSet)
			{
				m_gridRect.minY = iY;
			}
			if (iY > m_gridRect.maxY || m_notSet)
			{
				m_gridRect.maxY = iY;
			}
			m_nodes.Add(new GridPos(gridPos.x, gridPos.y), new Node(gridPos.x, gridPos.y, iWalkable));
		}
		else if (m_nodes.ContainsKey(gridPos))
		{
			m_nodes.Remove(gridPos);
			if (iX == m_gridRect.minX || iX == m_gridRect.maxX || iY == m_gridRect.minY || iY == m_gridRect.maxY)
			{
				m_notSet = true;
			}
		}
		return true;
	}

	public override Node GetNodeAt(GridPos iPos)
	{
		if (m_nodes.ContainsKey(iPos))
		{
			return m_nodes[iPos];
		}
		return null;
	}

	public override bool IsWalkableAt(GridPos iPos)
	{
		return m_nodes.ContainsKey(iPos);
	}

	public override bool SetWalkableAt(GridPos iPos, bool iWalkable)
	{
		return SetWalkableAt(iPos.x, iPos.y, iWalkable);
	}

	public override void Reset()
	{
		Reset(null);
	}

	public void Reset(List<GridPos> iWalkableGridList)
	{
		foreach (KeyValuePair<GridPos, Node> node in m_nodes)
		{
			node.Value.Reset();
		}
		if (iWalkableGridList == null)
		{
			return;
		}
		foreach (KeyValuePair<GridPos, Node> node2 in m_nodes)
		{
			if (iWalkableGridList.Contains(node2.Key))
			{
				SetWalkableAt(node2.Key, iWalkable: true);
			}
			else
			{
				SetWalkableAt(node2.Key, iWalkable: false);
			}
		}
	}

	public override BaseGrid Clone()
	{
		DynamicGrid dynamicGrid = new DynamicGrid();
		foreach (KeyValuePair<GridPos, Node> node in m_nodes)
		{
			dynamicGrid.SetWalkableAt(node.Key.x, node.Key.y, iWalkable: true);
		}
		return dynamicGrid;
	}
}
