using System;
using System.Collections.Generic;

[Serializable]
public class LocaleGroupAsset : Asset
{
	public string[] libraries;

	public List<string> contains = new List<string>();

	public List<string> starts_with_priority = new List<string>();

	public List<string> starts_with = new List<string>();

	public List<string> matches = new List<string>();

	public LocaleGroupChecker checker;

	public Dictionary<string, string> locales = new Dictionary<string, string>();
}
