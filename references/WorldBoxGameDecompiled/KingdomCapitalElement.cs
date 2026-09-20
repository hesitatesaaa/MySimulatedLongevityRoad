using System.Collections;
using UnityEngine;

public class KingdomCapitalElement : KingdomElement
{
	[SerializeField]
	private CityListElement _capital_element;

	protected override IEnumerator showContent()
	{
		if (base.kingdom.hasCapital())
		{
			track_objects.Add(base.kingdom.capital);
			((Component)_capital_element).gameObject.SetActive(true);
			_capital_element.show(base.kingdom.capital);
		}
		yield break;
	}

	protected override void clear()
	{
		((Component)_capital_element).gameObject.SetActive(false);
		base.clear();
	}

	public override bool checkRefreshWindow()
	{
		if (((Component)_capital_element).gameObject.activeSelf && !base.kingdom.hasCapital())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}
}
