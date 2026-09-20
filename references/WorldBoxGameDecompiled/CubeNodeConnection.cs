using UnityEngine;
using UnityEngine.UI;

public class CubeNodeConnection : MonoBehaviour
{
	[SerializeField]
	private Sprite _connection_inner;

	[SerializeField]
	private Sprite _connection_outer;

	public Image image;

	internal CubeNode node_1;

	internal CubeNode node_2;

	internal bool inner_cube;

	internal float mod_light = 1f;

	public void update()
	{
		mod_light -= Time.deltaTime * 2f;
		mod_light = Mathf.Max(0f, mod_light);
	}

	public void setConnection(bool pInner)
	{
		inner_cube = pInner;
		if (pInner)
		{
			image.sprite = _connection_inner;
		}
		else
		{
			image.sprite = _connection_outer;
		}
	}

	public void clear()
	{
		node_1 = null;
		node_2 = null;
		inner_cube = false;
	}
}
