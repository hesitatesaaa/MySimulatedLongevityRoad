public static class WarWinnerExtensions
{
	public static string getLocaleID(this WarWinner pWinner)
	{
		return pWinner switch
		{
			WarWinner.Attackers => "attackers", 
			WarWinner.Defenders => "defenders", 
			WarWinner.Peace => "peace", 
			WarWinner.Merged => "war_winner_merged", 
			_ => "war_winner_nobody", 
		};
	}
}
