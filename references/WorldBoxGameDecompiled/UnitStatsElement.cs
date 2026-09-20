using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UnitStatsElement : UnitElement, IStatsElement, IRefreshElement
{
	[SerializeField]
	private Image _icon_sex;

	[SerializeField]
	private Image _icon_species;

	[SerializeField]
	private Sprite _default_creature_icon;

	private StatsIconContainer _stats_icons;

	public void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		_stats_icons.setIconValue(pName, pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
	}

	protected override void Awake()
	{
		_stats_icons = ((Component)this).gameObject.AddOrGetComponent<StatsIconContainer>();
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (actor == null || !actor.isAlive())
		{
			yield break;
		}
		actor.updateStats();
		if (actor.asset.inspect_stats)
		{
			showAttribute("i_diplomacy", (int)actor.stats["diplomacy"]);
			showAttribute("i_stewardship", (int)actor.stats["stewardship"]);
			showAttribute("i_intelligence", (int)actor.stats["intelligence"]);
			showAttribute("i_warfare", (int)actor.stats["warfare"]);
			int num = (int)actor.stats["damage"];
			int num2 = (int)((float)num * actor.stats["damage_range"]);
			setIconValue("i_damage", num2, num, "", pFloat: false, "", '-');
			setIconValue("i_armor", actor.stats["armor"], null, "", pFloat: false, "%");
			setIconValue("i_speed", actor.stats["speed"]);
			setIconValue("i_critical_chance", actor.stats["critical_chance"] * 100f, null, "", pFloat: false, "%");
			setIconValue("i_attack_speed", actor.stats["attack_speed"], null, "", pFloat: true);
		}
		if (actor.asset.inspect_kills)
		{
			setIconValue("i_kills", actor.data.kills);
		}
		if (actor.asset.inspect_children)
		{
			setIconValue("i_births", actor.data.births);
			setIconValue("i_children", actor.current_children_count, actor.getMaxOffspring());
		}
		setIconValue("i_renown", actor.data.renown);
		if (actor.asset.inspect_experience)
		{
			bool num3 = actor.hasTrait("immortal");
			StatsIcon iconViaId = _stats_icons.getIconViaId("i_lifespan");
			if (num3)
			{
				_stats_icons.setText("i_lifespan", "???", "#F3961F");
			}
			else
			{
				iconViaId.enable_animation = true;
				setIconValue("i_lifespan", actor.stats["lifespan"]);
			}
			setIconValue("i_level", actor.data.level);
			setIconValue("i_experience", actor.data.experience, actor.getExpToLevelup());
		}
		Sprite val = actor.asset.getSpriteIcon();
		if ((Object)(object)val == (Object)null || actor.asset.is_boat)
		{
			val = _default_creature_icon;
		}
		_icon_species.sprite = val;
		if (actor.asset.inspect_sex)
		{
			if (actor.isSexMale())
			{
				_icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
			}
			else
			{
				_icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
			}
		}
		else
		{
			_icon_sex.sprite = val;
		}
	}

	private void showAttribute(string pName, int pValue)
	{
		_stats_icons.TryGetValue(pName, out var pIcon);
		if (!((Object)(object)pIcon == (Object)null))
		{
			((Component)pIcon).gameObject.SetActive(true);
			if (pValue < 4)
			{
				pIcon.setValue(pValue, null, "#FB2C21");
			}
			else if (pValue >= 20)
			{
				pIcon.setValue(pValue, null, "#43FF43");
			}
			else
			{
				pIcon.setValue(pValue);
			}
		}
	}
}
