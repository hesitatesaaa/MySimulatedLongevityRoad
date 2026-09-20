public class OnomasticsDataGroup
{
	public string[] characters;

	public string characters_string;

	public bool isEmpty()
	{
		if (characters != null)
		{
			return characters.Length == 0;
		}
		return true;
	}
}
