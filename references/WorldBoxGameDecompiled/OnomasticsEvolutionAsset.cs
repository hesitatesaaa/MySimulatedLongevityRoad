using System;
using System.ComponentModel;

[Serializable]
public class OnomasticsEvolutionAsset : Asset
{
	public string from;

	public string to;

	public char[] not_surrounded_by;

	[DefaultValue(true)]
	public bool reverse = true;

	public OnomasticsReplacerDelegate replacer;
}
