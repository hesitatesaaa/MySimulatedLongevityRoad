using System.Collections.Generic;
using UnityEngine;

public static class HotkeysLocalized
{
	private static Dictionary<KeyCode, string> _dictionary;

	private static void init()
	{
		_dictionary = new Dictionary<KeyCode, string>();
		_dictionary.Add((KeyCode)48, "0");
		_dictionary.Add((KeyCode)49, "1");
		_dictionary.Add((KeyCode)50, "2");
		_dictionary.Add((KeyCode)51, "3");
		_dictionary.Add((KeyCode)52, "4");
		_dictionary.Add((KeyCode)53, "5");
		_dictionary.Add((KeyCode)54, "6");
		_dictionary.Add((KeyCode)55, "7");
		_dictionary.Add((KeyCode)56, "8");
		_dictionary.Add((KeyCode)57, "9");
		_dictionary.Add((KeyCode)256, "0");
		_dictionary.Add((KeyCode)257, "1");
		_dictionary.Add((KeyCode)258, "2");
		_dictionary.Add((KeyCode)259, "3");
		_dictionary.Add((KeyCode)260, "4");
		_dictionary.Add((KeyCode)261, "5");
		_dictionary.Add((KeyCode)262, "6");
		_dictionary.Add((KeyCode)263, "7");
		_dictionary.Add((KeyCode)264, "8");
		_dictionary.Add((KeyCode)265, "9");
		_dictionary.Add((KeyCode)32, "SPACE");
		_dictionary.Add((KeyCode)304, "SHIFT");
		_dictionary.Add((KeyCode)303, "SHIFT");
		_dictionary.Add((KeyCode)308, "ALT");
		_dictionary.Add((KeyCode)307, "ALT");
		_dictionary.Add((KeyCode)306, "CONTROL");
		_dictionary.Add((KeyCode)305, "CONTROL");
		_dictionary.Add((KeyCode)310, "");
		_dictionary.Add((KeyCode)309, "");
	}

	public unsafe static string getLocalizedKey(KeyCode pCode)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (_dictionary == null)
		{
			init();
		}
		if ((int)pCode == 0)
		{
			return string.Empty;
		}
		string empty = string.Empty;
		empty = ((!_dictionary.ContainsKey(pCode)) ? ((object)(*(KeyCode*)(&pCode))/*cast due to constrained. prefix*/).ToString() : _dictionary[pCode]);
		if (string.IsNullOrEmpty(empty))
		{
			return string.Empty;
		}
		return Toolbox.coloredText(empty, "#95DD5D");
	}
}
