using System;
using System.Collections.Generic;

[Serializable]
public class SentenceAsset : Asset
{
	private List<string[]> _templates = new List<string[]>();

	public void addTemplate(params string[] pTemplates)
	{
		_templates.Add(pTemplates);
	}

	public string[] getRandomTemplate()
	{
		if (_templates.Count == 0)
		{
			return null;
		}
		return _templates.GetRandom();
	}
}
