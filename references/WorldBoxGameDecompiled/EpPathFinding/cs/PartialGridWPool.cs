using System.Collections.Generic;

namespace EpPathFinding.cs;

public class PartialGridWPool : BaseGrid
{
	private NodePool m_nodePool;

	public override int width
	{
		get
		{
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
			return m_gridRect.maxY - m_gridRect.minY + 1;
		}
		protected set
		{
		}
	}

	public PartialGridWPool(NodePool iNodePool, GridRect iGridRect = null)
	{
		if (iGridRect == null)
		{
			m_gridRect = new GridRect();
		}
		else
		{
			m_gridRect = iGridRect;
		}
		m_nodePool = iNodePool;
	}

	public PartialGridWPool(PartialGridWPool b)
		: base(b)
	{
		m_nodePool = b.m_nodePool;
	}

	public void SetGridRect(GridRect iGridRect)
	{
		m_gridRect = iGridRect;
	}

	public bool IsInside(int iX, int iY)
	{
		if (iX < m_gridRect.minX || iX > m_gridRect.maxX || iY < m_gridRect.minY || iY > m_gridRect.maxY)
		{
			return false;
		}
		return true;
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

	public override bool SetWalkableAt(int iX, int iY, bool iWalkable, int pCost = 1)
	{
		if (!IsInside(iX, iY))
		{
			return false;
		}
		GridPos iPos = new GridPos(iX, iY);
		m_nodePool.SetNode(iPos, iWalkable);
		return true;
	}

	public bool IsInside(GridPos iPos)
	{
		return IsInside(iPos.x, iPos.y);
	}

	public override Node GetNodeAt(GridPos iPos)
	{
		if (!IsInside(iPos))
		{
			return null;
		}
		return m_nodePool.GetNode(iPos);
	}

	public override bool IsWalkableAt(GridPos iPos)
	{
		if (!IsInside(iPos))
		{
			return false;
		}
		return m_nodePool.Nodes.ContainsKey(iPos);
	}

	public override bool SetWalkableAt(GridPos iPos, bool iWalkable)
	{
		return SetWalkableAt(iPos.x, iPos.y, iWalkable);
	}

	public override void Reset()
	{
		int num = (m_gridRect.maxX - m_gridRect.minX) * (m_gridRect.maxY - m_gridRect.minY);
		if (m_nodePool.Nodes.Count > num)
		{
			GridPos gridPos = new GridPos(0, 0);
			for (int i = m_gridRect.minX; i <= m_gridRect.maxX; i++)
			{
				gridPos.x = i;
				for (int j = m_gridRect.minY; j <= m_gridRect.maxY; j++)
				{
					gridPos.y = j;
					Node node = m_nodePool.GetNode(gridPos);
					if (node != null)
					{
						node.Reset();
					}
				}
			}
			return;
		}
		foreach (KeyValuePair<GridPos, Node> node2 in m_nodePool.Nodes)
		{
			node2.Value.Reset();
		}
	}

	public override BaseGrid Clone()
	{
		return new PartialGridWPool(m_nodePool, m_gridRect);
	}
}
