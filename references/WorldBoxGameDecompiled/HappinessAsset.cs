using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

[Serializable]
public class HappinessAsset : Asset, ILocalizedAsset, IMultiLocalesAsset
{
	public HappinessDelegateCalc calc;

	public int value;

	public string pot_task_id;

	public int pot_amount;

	public int index;

	public string path_icon;

	public bool ignored_by_psychopaths;

	[DefaultValue(true)]
	public bool show_change_happiness_effect = true;

	public int dialogs_amount = 4;

	private Sprite _cached_sprite;

	public virtual Sprite getSprite()
	{
		if (_cached_sprite == null)
		{
			_cached_sprite = SpriteTextureLoader.getSprite(path_icon);
		}
		return _cached_sprite;
	}

	public string getLocaleID()
	{
		return "happiness_" + id;
	}

	public IEnumerable<string> getLocaleIDs()
	{
		for (int i = 0; i < dialogs_amount; i++)
		{
			yield return getHappinnessDialogID() + i;
		}
	}

	public string getHappinnessDialogID()
	{
		return "happiness_dialog_" + id + "_";
	}

	public string getTextSingleReport()
	{
		int num = Random.Range(0, dialogs_amount);
		return getHappinnessDialogID() + num;
	}

	public string getRandomTextSingleReportLocalized()
	{
		return LocalizedTextManager.getText(getTextSingleReport());
	}
}
