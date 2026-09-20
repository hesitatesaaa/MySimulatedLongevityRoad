using System.Collections;
using UnityEngine;

public class UnitHealthBarElement : UnitElement
{
	[SerializeField]
	private StatBar _health;

	protected override IEnumerator showContent()
	{
		_health.setBar(actor.getHealth(), actor.getMaxHealth(), "/" + actor.getMaxHealth().ToText(4));
		yield break;
	}
}
