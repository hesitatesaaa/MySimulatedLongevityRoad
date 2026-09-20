using UnityEngine;

public interface ISelectedMetaWithUnit
{
	SelectedMetaUnitElement unit_element { get; }

	GameObject unit_element_separator { get; }

	string unit_title_locale_key { get; }

	int last_dirty_stats_unit { get; set; }

	Actor last_unit { get; set; }

	bool checkUnitElement()
	{
		if (!hasUnit())
		{
			setUnitElementVisible(pState: false);
			return true;
		}
		setUnitElementVisible(pState: true);
		Actor unit = getUnit();
		UiUnitAvatarElement avatar = unit_element.getAvatar();
		if (unitChanged(unit) || avatar.avatarLoader.actorStateChanged())
		{
			unit_element.show(unit, unit_title_locale_key);
			last_dirty_stats_unit = unit.getStatsDirtyVersion();
			last_unit = unit;
			return true;
		}
		avatar.updateTileSprite();
		return false;
	}

	void setUnitElementVisible(bool pState)
	{
		((Component)unit_element).gameObject.SetActive(pState);
		unit_element_separator.SetActive(pState);
	}

	void avatarTouch()
	{
		if (hasUnit())
		{
			Actor unit = getUnit();
			SelectedUnit.select(unit);
			SelectedObjects.setNanoObject(unit);
			PowerTabController.showTabSelectedUnit();
			((IShakable)ToolbarButtons.instance).shake();
		}
	}

	bool hasUnit();

	Actor getUnit();

	bool unitChanged(Actor pActor)
	{
		if (pActor.getStatsDirtyVersion() == last_dirty_stats_unit)
		{
			return pActor != last_unit;
		}
		return true;
	}

	void clearLastUnit()
	{
		last_unit = null;
		last_dirty_stats_unit = -1;
	}
}
