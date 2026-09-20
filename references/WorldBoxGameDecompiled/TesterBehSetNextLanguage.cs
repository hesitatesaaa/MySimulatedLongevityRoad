using System;
using System.Collections.Generic;
using ai.behaviours;

public class TesterBehSetNextLanguage : BehaviourActionTester
{
	private List<string> languages = new List<string>();

	private int currentLanguage;

	public TesterBehSetNextLanguage()
	{
		languages = LocalizedTextManager.getAllLanguagesWithChanges();
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		if (currentLanguage >= languages.Count)
		{
			currentLanguage = 0;
		}
		string text = languages[currentLanguage++];
		Console.WriteLine("[" + Date.TimeNow() + "] Changed language to : " + text + " " + currentLanguage + "/" + languages.Count);
		LocalizedTextManager.instance.setLanguage(text);
		return BehResult.Continue;
	}
}
