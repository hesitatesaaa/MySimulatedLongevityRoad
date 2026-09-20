using System.Collections;
using UnityEngine;

public class CityElementNeutralHandler : CityElement
{
	[SerializeField]
	private GameObject _layout_element_content_meta;

	[SerializeField]
	private GameObject _layout_element_wants;

	[SerializeField]
	private GameObject _layout_element_ruler;

	private void checkNeutralElements()
	{
		if (meta_object.isNeutral())
		{
			_layout_element_content_meta.SetActive(false);
			_layout_element_wants.SetActive(false);
			_layout_element_ruler.SetActive(false);
		}
	}

	protected override IEnumerator showContent()
	{
		checkNeutralElements();
		return base.showContent();
	}
}
