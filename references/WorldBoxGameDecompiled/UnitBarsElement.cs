using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnitBarsElement : UnitElement
{
	[SerializeField]
	private StatBar _hunger;

	[SerializeField]
	private StatBar _happiness;

	[SerializeField]
	private StatBar _stamina;

	[SerializeField]
	private StatBar _mana;

	[SerializeField]
	private Image _favorite_food_sprite;

	[SerializeField]
	private Image _favorite_food_bg;

	protected override IEnumerator showContent()
	{
		showHappiness();
		showHunger();
		showStamina();
		showMana();
		yield break;
	}

	private void showMana()
	{
		if (!actor.asset.force_hide_mana)
		{
			((Component)_mana).gameObject.SetActive(true);
			int maxMana = actor.getMaxMana();
			int num = Mathf.Clamp(actor.getMana(), 0, maxMana);
			_mana.setBar(num, maxMana, "/" + maxMana.ToText(4));
		}
	}

	private void showStamina()
	{
		if (!actor.asset.force_hide_stamina)
		{
			((Component)_stamina).gameObject.SetActive(true);
			int maxStamina = actor.getMaxStamina();
			int num = Mathf.Clamp(actor.getStamina(), 0, maxStamina);
			_stamina.setBar(num, maxStamina, "/" + maxStamina.ToText(4));
		}
	}

	private void showHappiness()
	{
		if (actor.hasEmotions())
		{
			((Component)_happiness).GetComponentInChildren<HappinessBarIcon>().load(actor);
			((Component)_happiness).gameObject.SetActive(true);
			int happinessPercent = actor.getHappinessPercent();
			_happiness.setBar(happinessPercent, 100f, "%");
		}
	}

	private void showHunger()
	{
		if (actor.needsFood())
		{
			((Component)_hunger).gameObject.SetActive(true);
			int num = (int)((float)actor.getNutrition() / (float)actor.getMaxNutrition() * 100f);
			_hunger.setBar(num, 100f, "%");
			if (actor.hasFavoriteFood())
			{
				((Component)_favorite_food_bg).gameObject.SetActive(true);
				((Component)_favorite_food_sprite).gameObject.SetActive(true);
				_favorite_food_sprite.sprite = actor.favorite_food_asset.getSpriteIcon();
			}
		}
	}

	protected override void clear()
	{
		((Component)_mana).gameObject.SetActive(false);
		((Component)_stamina).gameObject.SetActive(false);
		((Component)_hunger).gameObject.SetActive(false);
		((Component)_happiness).gameObject.SetActive(false);
		((Component)_favorite_food_bg).gameObject.SetActive(false);
		((Component)_favorite_food_sprite).gameObject.SetActive(false);
		base.clear();
	}
}
