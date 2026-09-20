using System;
using UnityEngine;

public class SelectedMetaWithUnit<TMeta, TMetaData> : SelectedMeta<TMeta, TMetaData>, ISelectedMetaWithUnit where TMeta : MetaObject<TMetaData>, IFavoriteable where TMetaData : MetaObjectData
{
	[SerializeField]
	private SelectedMetaUnitElement _unit_element;

	[SerializeField]
	private GameObject _unit_element_separator;

	public SelectedMetaUnitElement unit_element => _unit_element;

	public GameObject unit_element_separator => _unit_element_separator;

	private ISelectedMetaWithUnit as_meta_with_unit => this;

	public int last_dirty_stats_unit { get; set; }

	public Actor last_unit { get; set; }

	public virtual string unit_title_locale_key { get; }

	public virtual bool hasUnit()
	{
		throw new NotImplementedException();
	}

	public virtual Actor getUnit()
	{
		throw new NotImplementedException();
	}

	protected override void updateElementsAlways(TMeta pNano)
	{
		base.updateElementsAlways(pNano);
		as_meta_with_unit.checkUnitElement();
		if (hasUnit())
		{
			_unit_element.updateBarAndTask(getUnit());
		}
	}

	protected override void showStatsGeneral(TMeta pMeta)
	{
		base.showStatsGeneral(pMeta);
		if (hasUnit())
		{
			Actor unit = getUnit();
			_unit_element.showStats(unit);
		}
	}

	public void avatarTouchScream()
	{
		as_meta_with_unit.avatarTouch();
	}

	protected override void clearLastObject()
	{
		base.clearLastObject();
		as_meta_with_unit.clearLastUnit();
	}
}
