using System;
using System.Collections.Generic;

public static class OnomasticsEvolver
{
	private static List<OnomasticsEvolutionAsset> _pool = new List<OnomasticsEvolutionAsset>();

	private static Dictionary<int, bool> _replaced = new Dictionary<int, bool>();

	public static void add(OnomasticsEvolutionAsset pEvolution)
	{
		_pool.Add(pEvolution);
	}

	public static void shuffle()
	{
		_pool.Shuffle();
	}

	public static bool scramble(OnomasticsData pData)
	{
		int num = 0;
		foreach (KeyValuePair<string, OnomasticsDataGroup> group in pData.groups)
		{
			OnomasticsDataGroup value = group.Value;
			if (value.isEmpty())
			{
				continue;
			}
			_replaced.Clear();
			string[] array = value.characters_string.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				_replaced.Add(i, value: false);
			}
			int num2 = 0;
			for (int j = 0; j < _pool.Count; j++)
			{
				if (num2 >= 7)
				{
					continue;
				}
				OnomasticsEvolutionAsset onomasticsEvolutionAsset = _pool[j];
				bool flag = false;
				if (array.Contains(onomasticsEvolutionAsset.to))
				{
					continue;
				}
				for (int k = 0; k < array.Length; k++)
				{
					if (!_replaced[k])
					{
						string pReplace = array[k];
						if (onomasticsEvolutionAsset.replacer(onomasticsEvolutionAsset, ref pReplace))
						{
							flag = true;
							_replaced[k] = true;
						}
						array[k] = pReplace;
					}
				}
				if (flag)
				{
					num2++;
				}
			}
			value.characters_string = string.Join(' ', array);
			value.characters = array;
			if (num2 > 0)
			{
				num++;
			}
		}
		return num > 0;
	}
}
