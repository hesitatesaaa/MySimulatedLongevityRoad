using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CorruptedTreesManager : MonoBehaviour
{
	private string currentString = "";

	private List<CorruptedTreeObject> _objects;

	public GameObject win_icon;

	public void Start()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Expected O, but got Unknown
		_objects = new List<CorruptedTreeObject>();
		for (int i = 0; i < ((Component)this).transform.childCount; i++)
		{
			CorruptedTreeObject tObj = ((Component)((Component)this).transform.GetChild(i)).GetComponent<CorruptedTreeObject>();
			_objects.Add(tObj);
			((Component)((Component)tObj).transform.GetChild(0)).gameObject.SetActive(false);
			((UnityEvent)((Component)tObj).GetComponent<Button>().onClick).AddListener((UnityAction)delegate
			{
				click(tObj);
			});
		}
		win_icon.gameObject.SetActive(false);
	}

	public void click(CorruptedTreeObject pObject)
	{
		if (!pObject.used)
		{
			pObject.used = true;
			((Component)((Component)pObject).transform.GetChild(0)).gameObject.SetActive(true);
			((Behaviour)((Component)pObject).GetComponent<Image>()).enabled = false;
			string text = "162534" ?? "";
			currentString += ((Object)((Component)pObject).transform).name;
			text = text.Substring(0, currentString.Length);
			if ("162534".CompareTo(text) == 0)
			{
				win();
			}
			else if (!text.Contains(currentString))
			{
				lost();
			}
		}
	}

	private void win()
	{
		win_icon.gameObject.SetActive(true);
		AchievementLibrary.the_corrupted_trees.check();
	}

	private void lost()
	{
		foreach (CorruptedTreeObject @object in _objects)
		{
			((Component)@object).GetComponent<UiCreature>().click();
		}
	}
}
