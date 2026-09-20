using UnityEngine;

public class EmptyLogElement : MonoBehaviour
{
	private WorldLogElement _log_element;

	public RectTransform rect_transform;

	private WorldLogElement _element;

	private WorldLogMessage _message;

	public void load(WorldLogMessage pMessage)
	{
		_message = pMessage;
	}

	public void setElement(WorldLogElement pElement)
	{
		_element = pElement;
		if (!((Object)(object)_element == (Object)null))
		{
			_element.showMessage(_message);
			((Component)pElement).transform.SetParent(((Component)this).transform);
		}
	}

	public WorldLogElement getElement()
	{
		return _element;
	}
}
