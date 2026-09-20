using System.Collections.Generic;
using UnityEngine;

public class DebugMessage : MonoBehaviour
{
	public GameObject prefab;

	public static bool log_enabled;

	public static DebugMessage instance;

	public List<DebugMessageFly> list;

	private List<DebugMessageFly> messagesToMove = new List<DebugMessageFly>();

	private void Start()
	{
		instance = this;
		list = new List<DebugMessageFly>();
	}

	public void moveAll(DebugMessageFly pMessage)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		messagesToMove.Clear();
		foreach (DebugMessageFly item in list)
		{
			if (!((Object)(object)item == (Object)(object)pMessage) && Toolbox.Dist(0f, ((Component)item).transform.localPosition.y, 0f, ((Component)pMessage).transform.localPosition.y) < 1f)
			{
				messagesToMove.Add(item);
			}
		}
		foreach (DebugMessageFly item2 in messagesToMove)
		{
			item2.moveUp();
		}
	}

	public DebugMessageFly getOldMessage(Transform pTransform)
	{
		foreach (DebugMessageFly item in list)
		{
			if ((Object)(object)item.originTransform == (Object)(object)pTransform)
			{
				return item;
			}
		}
		return null;
	}

	public static void log(Transform pTransofrm, string pMessage)
	{
		if (Debug.isDebugBuild && log_enabled)
		{
			DebugMessageFly oldMessage = instance.getOldMessage(pTransofrm);
			if ((Object)(object)oldMessage != (Object)null)
			{
				oldMessage.addString(pMessage);
				return;
			}
			TextMesh component = Object.Instantiate<GameObject>(instance.prefab).gameObject.GetComponent<TextMesh>();
			((Renderer)((Component)component).gameObject.GetComponent<MeshRenderer>()).sortingOrder = 100;
			((Component)component).transform.parent = ((Component)instance).transform;
			DebugMessageFly component2 = ((Component)component).GetComponent<DebugMessageFly>();
			component2.originTransform = pTransofrm;
			component2.addString(pMessage);
			instance.list.Add(component2);
		}
	}
}
