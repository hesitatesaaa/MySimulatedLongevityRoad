using System;
using Beebyte.Obfuscator;

[ObfuscateLiterals]
internal static class VersionCallbacks
{
	internal static Action<string> versionCallbacks;

	internal static float timer = 0f;

	internal static string versionCheck = string.Empty;

	public static void init()
	{
		versionCheck = VersionCheck._vsCheck;
		if (!string.IsNullOrEmpty(versionCheck) && versionCheck.Split('.').Length != 3 && (versionCallbacks == null || versionCallbacks.GetInvocationList().Length == 0))
		{
			TestingCB.init();
		}
	}

	internal static void updateVC(float pElapsed)
	{
		timer -= pElapsed;
		if (timer > 0f)
		{
			return;
		}
		timer = 0f;
		try
		{
			init();
			if (!string.IsNullOrEmpty(versionCheck))
			{
				versionCallbacks?.Invoke(versionCheck);
			}
			if (versionCheck.Split('.').Length != 3)
			{
				timer = Randy.randomFloat(300f, 600f);
			}
		}
		catch (Exception)
		{
		}
	}
}
