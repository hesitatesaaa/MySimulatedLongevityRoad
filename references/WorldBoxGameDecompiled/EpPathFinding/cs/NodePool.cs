using System.Collections.Generic;

namespace EpPathFinding.cs;

public class NodePool
{
	protected Dictionary<GridPos, Node> m_nodes;

	public Dictionary<GridPos, Node> Nodes => m_nodes;

	public NodePool()
	{
		m_nodes = new Dictionary<GridPos, Node>();
	}

	public Node GetNode(int iX, int iY)
	{
		GridPos iPos = new GridPos(iX, iY);
		return GetNode(iPos);
	}

	public Node GetNode(GridPos iPos)
	{
		Node value = null;
		m_nodes.TryGetValue(iPos, out value);
		return value;
	}

	public Node SetNode(int iX, int iY, bool? iWalkable = null)
	{
		GridPos iPos = new GridPos(iX, iY);
		return SetNode(iPos, iWalkable);
	}

	public Node SetNode(GridPos iPos, bool? iWalkable = null)
	{
		if (iWalkable.HasValue)
		{
			if (iWalkable.Value)
			{
				Node value = null;
				if (m_nodes.TryGetValue(iPos, out value))
				{
					return value;
				}
				Node node = new Node(iPos.x, iPos.y, iWalkable);
				m_nodes.Add(iPos, node);
				return node;
			}
			removeNode(iPos);
			return null;
		}
		Node node2 = new Node(iPos.x, iPos.y, true);
		m_nodes.Add(iPos, node2);
		return node2;
	}

	protected void removeNode(int iX, int iY)
	{
		GridPos iPos = new GridPos(iX, iY);
		removeNode(iPos);
	}

	protected void removeNode(GridPos iPos)
	{
		if (m_nodes.ContainsKey(iPos))
		{
			m_nodes.Remove(iPos);
		}
	}
}
