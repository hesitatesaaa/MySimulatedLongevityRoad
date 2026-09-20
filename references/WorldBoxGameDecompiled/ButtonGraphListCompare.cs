using UnityEngine;

public class ButtonGraphListCompare : MonoBehaviour
{
	public void compareListItems()
	{
		ScrollWindow currentWindow = ScrollWindow.getCurrentWindow();
		IComponentList componentInChildren = ((Component)currentWindow).GetComponentInChildren<IComponentList>(true);
		if (componentInChildren == null)
		{
			Debug.LogError((object)("IComponentList missing in " + ((Object)((Component)currentWindow).gameObject).name), (Object)(object)((Component)currentWindow).gameObject);
			return;
		}
		using ListPool<NanoObject> listPool = componentInChildren.getElements();
		if (listPool.Count > 0)
		{
			Config.selected_objects_graph.Clear();
			for (int i = 0; i < listPool.Count && i < 3; i++)
			{
				NanoObject pObject = listPool[i];
				Config.selected_objects_graph.Add(pObject);
			}
		}
		ScrollWindow.showWindow("chart_comparer");
	}
}
