using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ai.behaviours;

public class SelectedMetaUnitElement : MonoBehaviour
{
	[SerializeField]
	private UiUnitAvatarElement _avatar;

	[SerializeField]
	private Text _title;

	[SerializeField]
	private UnitTextManager _unit_texts;

	[SerializeField]
	private Image _icon_sex;

	[SerializeField]
	private Image _icon_species;

	[SerializeField]
	private Image _task_icon_left;

	[SerializeField]
	private Image _task_icon_right;

	[SerializeField]
	private Text _task_title;

	[SerializeField]
	private Sprite _no_task_icon;

	[SerializeField]
	private StatBar _bar_health;

	private Dictionary<string, StatsIcon> _stats_icons = new Dictionary<string, StatsIcon>();

	private Actor _actor;

	private void Awake()
	{
		StatsIcon[] componentsInChildren = ((Component)this).GetComponentsInChildren<StatsIcon>(true);
		foreach (StatsIcon statsIcon in componentsInChildren)
		{
			if (!_stats_icons.TryAdd(((Object)statsIcon).name, statsIcon))
			{
				Debug.LogError((object)("Duplicate icon name! " + ((Object)statsIcon).name));
			}
		}
	}

	public void show(Actor pActor, string pLocaleKey)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		_actor = pActor;
		_avatar.show(_actor);
		if (string.IsNullOrEmpty(pLocaleKey))
		{
			_title.text = _actor.getName();
		}
		else
		{
			string text = LocalizedTextManager.getText(pLocaleKey);
			_title.text = text.Replace("$unit$", _actor.getName());
		}
		((Graphic)_title).color = _actor.kingdom.getColor().getColorText();
		if (_actor.isSexMale())
		{
			_icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
		}
		else
		{
			_icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
		}
		_icon_species.sprite = _actor.getActorAsset().getSprite();
	}

	public void updateBarAndTask(Actor pActor)
	{
		float pVal = pActor.getHealth();
		float num = pActor.getMaxHealth();
		_bar_health.setBar(pVal, num, "/" + ((int)num).ToText(4), pReset: false, pFloat: false, pUpdateText: true, 0.25f);
		BehaviourTaskActor task = pActor.ai.task;
		string taskText = pActor.getTaskText();
		Sprite sprite = ((task != null) ? task.getSprite() : _no_task_icon);
		_task_icon_left.sprite = sprite;
		_task_icon_right.sprite = sprite;
		_task_title.text = taskText;
	}

	public void showStats(Actor pActor)
	{
		int num = (int)pActor.stats["damage"];
		int num2 = (int)((float)num * pActor.stats["damage_range"]);
		setIconValue("i_age", pActor.data.getAge());
		setIconValue("i_damage", num2, num, "", pFloat: false, "", '-');
		setIconValue("i_armor", pActor.stats["armor"], null, "", pFloat: false, "%");
		setIconValue("i_kills", pActor.data.kills);
		setIconValue("i_renown", pActor.data.renown);
		setIconValue("i_level", pActor.data.level);
		setIconValue("i_experience", pActor.data.experience, pActor.getExpToLevelup());
		setIconValue("i_money", pActor.money);
		setIconValue("i_loot", pActor.loot);
	}

	public void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/')
	{
		StatsIcon iconViaId = getIconViaId(pName);
		if (!((Object)(object)iconViaId == (Object)null) && !iconViaId.areValuesTooClose(pMainVal))
		{
			iconViaId.setValue(pMainVal, pMax, pColor, pFloat, pEnding, pSeparator);
		}
	}

	public StatsIcon getIconViaId(string pName)
	{
		_stats_icons.TryGetValue(pName, out var value);
		if ((Object)(object)value == (Object)null)
		{
			return null;
		}
		((Component)value).gameObject.SetActive(true);
		return value;
	}

	public void spawnAvatarText()
	{
		_unit_texts.spawnAvatarText(_actor);
	}

	public UiUnitAvatarElement getAvatar()
	{
		return _avatar;
	}
}
