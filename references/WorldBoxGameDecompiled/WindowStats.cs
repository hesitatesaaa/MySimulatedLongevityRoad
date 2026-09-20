public struct WindowStats
{
	public int opens;

	public int closes;

	public int shows;

	public int hides;

	public string previous;

	public string current;

	public void setCurrent(string pCurrent)
	{
		if (!(current == pCurrent))
		{
			if (current != null && previous != current)
			{
				previous = current;
			}
			current = pCurrent;
		}
	}
}
