using UnityEngine;

public class BaseEmptyListMono : MonoBehaviour
{
	internal NanoObject meta_object;

	internal MonoBehaviour element;

	private bool has_element;

	private bool has_object;

	public RectTransform rect_transform;

	internal string debug_original_name;

	public void Awake()
	{
		rect_transform = ((Component)this).GetComponent<RectTransform>();
	}

	public void assignObject(NanoObject pObject)
	{
		meta_object = pObject;
		has_object = true;
	}

	public void assignElement(MonoBehaviour pElement)
	{
		element = pElement;
		has_element = true;
	}

	public bool hasElement()
	{
		return has_element;
	}

	public void clearElement()
	{
		element = null;
		has_element = false;
	}

	public void clearObject()
	{
		meta_object = null;
		has_object = false;
	}

	public bool hasObject()
	{
		return has_object;
	}

	public void debugUpdateName(bool tVisible)
	{
		if (string.IsNullOrEmpty(debug_original_name))
		{
			debug_original_name = ((Object)((Component)this).gameObject).name;
		}
		if (tVisible)
		{
			((Object)((Component)this).gameObject).name = "(v) (" + ((Component)this).gameObject.transform.childCount + ") " + debug_original_name;
		}
		else
		{
			((Object)((Component)this).gameObject).name = "(i) (" + ((Component)this).gameObject.transform.childCount + ") " + debug_original_name;
		}
	}
}
