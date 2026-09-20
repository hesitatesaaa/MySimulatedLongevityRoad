using System;
using System.Threading.Tasks;

public class Login
{
	public static string createLoginQueueItemAsJSON(string username, string password)
	{
		string text = "";
		return "" + text;
	}

	public static async void GetEmailForUsername(string username, string password, Action<string, string> resultCallback)
	{
		await Task.Yield();
	}

	private static void UnsubscribeLoginQueueListener()
	{
	}
}
