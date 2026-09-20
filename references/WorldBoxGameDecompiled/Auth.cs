using System;
using RSG;

public static class Auth
{
	public static UserLoginWindow userLoginWindow;

	public static bool isLoggedIn;

	public static string userId;

	public static string userName;

	public static string displayName;

	public static string emailAddress;

	private static bool initialized;

	public static bool authLoaded;

	public static Promise authLoadedPromise;

	public static void initializeAuth()
	{
		if (!initialized)
		{
			initialized = true;
		}
	}

	public static void AuthStateChanged(object sender, EventArgs eventArgs)
	{
	}

	public static void signOut()
	{
	}

	public static bool isValidUsername(string username)
	{
		return false;
	}

	public static bool isValidEmail(string email)
	{
		return false;
	}

	static Auth()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Expected O, but got Unknown
		isLoggedIn = false;
		initialized = false;
		authLoaded = false;
		authLoadedPromise = new Promise();
	}
}
