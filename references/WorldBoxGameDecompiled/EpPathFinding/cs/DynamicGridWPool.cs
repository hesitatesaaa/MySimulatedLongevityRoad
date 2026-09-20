using System.Collections.Generic;

namespace EpPathFinding.cs;

public class DynamicGridWPool : BaseGrid
{
	private bool m_notSet;

	private NodePool m_nodePool;

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

	public DynamicGridWPool(NodePool iNodePool)
	{
		m_gridRect = new GridRect();
		m_gridRect.minX = 0;
		m_gridRect.minY = 0;
		m_gridRect.maxX = 0;
		m_gridRect.maxY = 0;
		m_notSet = true;
		m_nodePool = iNodePool;
	}

	public DynamicGridWPool(DynamicGridWPool b)
		: base(b)
	{
		m_notSet = b.m_notSet;
		m_nodePool = b.m_nodePool;
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
		foreach (KeyValuePair<GridPos, Node> node in m_nodePool.Nodes)
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
		GridPos iPos = new GridPos(iX, iY);
		m_nodePool.SetNode(iPos, iWalkable);
		if (iWalkable)
		{
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
		}
		else if (iX == m_gridRect.minX || iX == m_gridRect.maxX || iY == m_gridRect.minY || iY == m_gridRect.maxY)
		{
			m_notSet = true;
		}
		return true;
	}

	public override Node GetNodeAt(GridPos iPos)
	{
		return m_nodePool.GetNode(iPos);
	}

	public override bool IsWalkableAt(GridPos iPos)
	{
		return m_nodePool.Nodes.ContainsKey(iPos);
	}

	public override bool SetWalkableAt(GridPos iPos, bool iWalkable)
	{
		return SetWalkableAt(iPos.x, iPos.y, iWalkable);
	}

	public override void Reset()
	{
		foreach (KeyValuePair<GridPos, Node> node in m_nodePool.Nodes)
		{
			node.Value.Reset();
		}
	}

	public override BaseGrid Clone()
	{
		return new DynamicGridWPool(m_nodePool);
	}
}
