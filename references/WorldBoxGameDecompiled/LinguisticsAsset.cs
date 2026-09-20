using System;
using System.Collections.Generic;

[Serializable]
public class LinguisticsAsset : Asset
{
	public string simple_text;

	public WordType word_type = WordType.None;

	public bool symbols_around;

	public string symbols_around_left;

	public string symbols_around_right;

	public bool add_space;

	public bool next_uppercase;

	public string[] array;

	public bool word_group;

	private List<string[]> _pot_patterns = new List<string[]>();

	public void addPattern(int pRate, params string[] pPattern)
	{
		_pot_patterns.AddTimes(pRate, pPattern);
	}

	public string getRandom()
	{
		return array.GetRandom();
	}

	public string[] getRandomPattern()
	{
		if (_pot_patterns.Count == 0)
		{
			return null;
		}
		return _pot_patterns.GetRandom();
	}

	public string getLocaleID()
	{
		throw new NotImplementedException();
	}

	public string getDescriptionID()
	{
		throw new NotImplementedException();
	}

	public string getDescriptionID2()
	{
		throw new NotImplementedException();
	}
}
