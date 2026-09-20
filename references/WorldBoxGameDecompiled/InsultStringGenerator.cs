using System.Collections.Generic;

public static class InsultStringGenerator
{
	private static string[] _insult_characters_2 = new string[4] { "#", "%", "&", "@" };

	private static string[] _insult_characters = new string[5] { "!", "#", "%", "&", "@" };

	private static List<string> _cached_bad_strings = new List<string>();

	private static List<string> _cached_bad_connections_string = new List<string>();

	private const int MAX_HARMFUL_DNA_SEQUENCES = 30;

	private const int MAX_BAD_CONNECTION_STRINGS = 30;

	public static string getRandomText(int pMin = 4, int pMax = 9, bool pUseSameSizeSet = false)
	{
		using StringBuilderPool stringBuilderPool = new StringBuilderPool();
		int num = Randy.randomInt(pMin, pMax);
		for (int i = 0; i < num; i++)
		{
			string value = (pUseSameSizeSet ? _insult_characters_2.GetRandom() : _insult_characters.GetRandom());
			stringBuilderPool.Append(value);
		}
		return stringBuilderPool.ToString();
	}

	public static string getDNASequenceBad()
	{
		string text;
		if (_cached_bad_strings.Count < 30)
		{
			using StringBuilderPool stringBuilderPool = new StringBuilderPool();
			for (int i = 0; i < 6; i++)
			{
				if (i > 0)
				{
					stringBuilderPool.Append(" ");
				}
				stringBuilderPool.Append(getRandomText(3, 3, pUseSameSizeSet: true));
			}
			text = stringBuilderPool.ToString();
			text = Toolbox.coloredString(text, "#B159FF");
			_cached_bad_strings.Add(text);
		}
		else
		{
			text = _cached_bad_strings.GetRandom();
		}
		return text;
	}

	public static string getBadConnectionString()
	{
		string randomText;
		if (_cached_bad_connections_string.Count < 30)
		{
			randomText = getRandomText(7, 7, pUseSameSizeSet: true);
			randomText = Toolbox.coloredString(randomText, "#B159FF");
			_cached_bad_connections_string.Add(randomText);
		}
		else
		{
			randomText = _cached_bad_connections_string.GetRandom();
		}
		return randomText;
	}
}
