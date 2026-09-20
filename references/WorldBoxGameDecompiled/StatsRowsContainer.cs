using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsRowsContainer : MonoBehaviour
{
	protected StatsWindow stats_window;

	private ObjectPoolGenericMono<KeyValueField> _stats_pool;

	protected List<KeyValueField> stats_rows = new List<KeyValueField>();

	private void Awake()
	{
		init();
	}

	private void OnEnable()
	{
		showStats();
		((MonoBehaviour)this).StartCoroutine(showRows());
	}

	protected virtual void init()
	{
		stats_window = ((Component)this).GetComponentInParent<StatsWindow>();
		KeyValueField pPrefab = Resources.Load<KeyValueField>("ui/KeyValueFieldStats");
		_stats_pool = new ObjectPoolGenericMono<KeyValueField>(pPrefab, ((Component)this).transform);
	}

	protected virtual void showStats()
	{
		stats_window.showStatsRows();
	}

	private IEnumerator showRows()
	{
		foreach (KeyValueField stats_row in stats_rows)
		{
			((Component)stats_row).gameObject.SetActive(true);
			yield return CoroutineHelper.wait_for_next_frame;
		}
	}

	private void OnDisable()
	{
		_stats_pool.clear();
		stats_rows.Clear();
	}

	internal KeyValueField getStatRow(string pKey)
	{
		KeyValueField next = _stats_pool.getNext();
		((Object)((Component)next).gameObject).name = "[KV] " + pKey;
		((Component)next).gameObject.SetActive(false);
		stats_rows.Add(next);
		return next;
	}

	internal KeyValueField showStatRow(string pId, object pValue, string pColor, MetaType pMetaType = MetaType.None, long pMetaId = -1L, bool pColorText = false, string pIconPath = null, string pTooltipId = null, TooltipDataGetter pTooltipData = null, bool pLocalize = true)
	{
		KeyValueField statRow = getStatRow(pId);
		bool flag = !string.IsNullOrEmpty(pIconPath);
		if (flag)
		{
			Sprite sprite = SpriteTextureLoader.getSprite("ui/Icons/" + pIconPath);
			statRow.icon.sprite = sprite;
		}
		((Component)statRow.icon).gameObject.SetActive(flag);
		string text = (pLocalize ? LocalizedTextManager.getText(pId) : pId);
		if (string.IsNullOrEmpty(pId))
		{
			statRow.name_text.text = "";
		}
		else if (pColorText)
		{
			statRow.name_text.text = Toolbox.coloredString(text, pColor);
		}
		else
		{
			statRow.name_text.text = text;
		}
		if (!string.IsNullOrEmpty(pColor))
		{
			statRow.value.text = Toolbox.coloredString(pValue.ToString(), pColor);
		}
		else
		{
			statRow.value.text = pValue.ToString();
		}
		statRow.setMetaForTooltip(pMetaType, pMetaId, pTooltipId, pTooltipData);
		return statRow;
	}
}
