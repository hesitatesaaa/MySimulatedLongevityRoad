using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleObjects : MonoBehaviour
{
	[SerializeField]
	private List<GameObject> _elements;

	private void Awake()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		((UnityEvent)((Component)this).GetComponent<Button>().onClick).AddListener(new UnityAction(toggle));
	}

	private void toggle()
	{
		if (_elements == null)
		{
			return;
		}
		foreach (GameObject element in _elements)
		{
			element.SetActive(!element.activeSelf);
		}
	}
}
