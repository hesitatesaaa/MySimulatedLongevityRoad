using System;

public static class EditorHelper
{
	public static bool HasArgument(string pName)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].Contains(pName))
			{
				return true;
			}
		}
		return false;
	}

	public static string GetArgument(string pName)
	{
		string[] commandLineArgs = Environment.GetCommandLineArgs();
		for (int i = 0; i < commandLineArgs.Length; i++)
		{
			if (commandLineArgs[i].Contains(pName))
			{
				return commandLineArgs[i + 1];
			}
		}
		return null;
	}
}
