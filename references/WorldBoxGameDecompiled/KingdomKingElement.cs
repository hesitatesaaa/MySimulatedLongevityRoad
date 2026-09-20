using System.Collections;
using UnityEngine;

public class KingdomKingElement : KingdomElement
{
	[SerializeField]
	private GameObject _title_element;

	[SerializeField]
	private PrefabUnitElement _king_element;

	protected override IEnumerator showContent()
	{
		if (base.kingdom.hasKing())
		{
			track_objects.Add(base.kingdom.king);
			_title_element.SetActive(true);
			((Component)_king_element).gameObject.SetActive(true);
			_king_element.show(base.kingdom.king);
		}
		yield break;
	}

	protected override void clear()
	{
		_title_element.SetActive(false);
		((Component)_king_element).gameObject.SetActive(false);
		base.clear();
	}

	public override bool checkRefreshWindow()
	{
		if (((Component)_king_element).gameObject.activeSelf && !base.kingdom.hasKing())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}
}
