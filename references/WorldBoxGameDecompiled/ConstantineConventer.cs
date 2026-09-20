using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConstantineConventer
{
	private static bool enabled;

	public static void init()
	{
		if (enabled)
		{
			string[] array = Resources.Load<TextAsset>("texts/fmod_sheet").text.Split('\n');
			Debug.Log((object)array[0]);
			List<string> list = new List<string>();
			string text = "";
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text2 = array2[i].Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
				string text3 = text2.Split('/')[^1];
				string text4 = "\tpublic const string ";
				text4 += text3;
				text4 += " = ";
				text4 += "\"";
				text4 += text2;
				text4 += "\"";
				text4 += ";";
				list.Add(text4);
				text = text + text4 + "\n";
			}
			File.WriteAllText(Application.dataPath + "/Resources/texts/fmod_sheet_converted.txt", text);
		}
	}

	public static void init2()
	{
		string[] array = Resources.Load<TextAsset>("texts/fmod_sheet").text.Split('\n');
		Debug.Log((object)array[0]);
		List<string> list = new List<string>();
		string text = "";
		string text2 = "";
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text3 = array2[i].Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
			if (text3.Contains("$"))
			{
				text3 = text3.Replace("$ ", "");
				text3 = text3.Replace("$", "");
				text2 = text3;
				continue;
			}
			if (!text3.Contains("WB_SFX_"))
			{
				text += "\n";
				continue;
			}
			string text4 = "\tpublic const string ";
			text4 += text3;
			text4 += " = ";
			text4 = text4 + text2 + " + ";
			text4 += "\"";
			text4 += text3;
			text4 += "\"";
			text4 += ";";
			list.Add(text4);
			text = text + text4 + "\n";
		}
		File.WriteAllText(Application.dataPath + "/Resources/texts/fmod_sheet_converted.txt", text);
	}
}
