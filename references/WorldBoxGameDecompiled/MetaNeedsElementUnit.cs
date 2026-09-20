using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MetaNeedsElementUnit : UnitElement
{
	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private Text _text;

	protected override IEnumerator showContent()
	{
		Actor unit = SelectedUnit.unit;
		if (unit != null && unit.isAlive())
		{
			string text = MetaTextReportHelper.addSingleUnitText(unit, pAddGap: false, pAddNameQuote: false);
			_text.text = text;
			if (!string.IsNullOrEmpty(text))
			{
				_container.gameObject.SetActive(true);
			}
		}
		yield break;
	}

	protected override void clear()
	{
		base.clear();
		_container.gameObject.SetActive(false);
	}
}
