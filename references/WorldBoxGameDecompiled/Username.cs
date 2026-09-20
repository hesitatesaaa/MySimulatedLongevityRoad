using System.Threading.Tasks;

public class Username
{
	public static bool isValid(string strToCheck)
	{
		return false;
	}

	public static async Task<bool> isTaken(string pUsername)
	{
		if (!isValid(pUsername))
		{
			return false;
		}
		await Task.Yield();
		return false;
	}
}
