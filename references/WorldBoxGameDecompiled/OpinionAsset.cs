using System;
using System.Collections.Generic;

[Serializable]
public class OpinionAsset : Asset, IMultiLocalesAsset
{
	public string translation_key;

	public string translation_key_negative;

	public OpinionDelegateCalc calc;

	public string getTranslationKey(int pValue)
	{
		if (pValue > 0)
		{
			return translation_key;
		}
		if (!string.IsNullOrEmpty(translation_key_negative))
		{
			return translation_key_negative;
		}
		return translation_key;
	}

	public IEnumerable<string> getLocaleIDs()
	{
		yield return translation_key;
		if (!string.IsNullOrEmpty(translation_key_negative))
		{
			yield return translation_key_negative;
		}
	}
}
