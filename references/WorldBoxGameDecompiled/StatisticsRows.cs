using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatisticsRows : MonoBehaviour
{
	[SerializeField]
	private KeyValueField _stat_prefab;

	private List<(KeyValueField field, MetaType type, string id)> _all_stats = new List<(KeyValueField, MetaType, string)>();

	private void Awake()
	{
		init();
	}

	private void OnEnable()
	{
		refreshStats();
	}

	private void OnDisable()
	{
		foreach (var all_stat in _all_stats)
		{
			((Component)all_stat.field).gameObject.SetActive(false);
		}
	}

	protected virtual void init()
	{
		throw new NotImplementedException();
	}

	protected void addStatRow(StatisticsAsset pAsset)
	{
		string id = pAsset.id;
		MetaType world_stats_meta_type = pAsset.world_stats_meta_type;
		KeyValueField keyValueField = Object.Instantiate<KeyValueField>(_stat_prefab, ((Component)this).transform);
		UiGameStat component = ((Component)keyValueField).GetComponent<UiGameStat>();
		component.setAsset(pAsset);
		component.updateText();
		bool flag = !string.IsNullOrEmpty(pAsset.path_icon);
		if (flag)
		{
			Sprite icon = pAsset.getIcon();
			keyValueField.icon.sprite = icon;
		}
		((Component)keyValueField.icon).gameObject.SetActive(flag);
		setupField(keyValueField, world_stats_meta_type, id);
		((Component)keyValueField).gameObject.SetActive(false);
		_all_stats.Add((keyValueField, world_stats_meta_type, id));
	}

	private void refreshStats()
	{
		foreach (var (pField, pMetaType, pID) in _all_stats)
		{
			setupField(pField, pMetaType, pID);
		}
		((MonoBehaviour)this).StartCoroutine(refreshRoutine());
	}

	private void setupField(KeyValueField pField, MetaType pMetaType, string pID)
	{
		StatisticsAsset tAsset = AssetManager.statistics_library.get(pID);
		if (pMetaType.isNone())
		{
			TooltipDataGetter pData = () => new TooltipData
			{
				tip_name = tAsset.getLocaleID(),
				tip_description = tAsset.getDescriptionID()
			};
			pField.setMetaForTooltip(pMetaType, -1L, "normal", pData);
		}
		else
		{
			pField.setMetaForTooltip(pMetaType, tAsset.get_meta_id?.Invoke(tAsset) ?? (-1));
		}
	}

	private IEnumerator refreshRoutine()
	{
		foreach (var all_stat in _all_stats)
		{
			((Component)all_stat.field).gameObject.SetActive(true);
			yield return (object)new WaitForSecondsRealtime(0.005f);
		}
	}
}
