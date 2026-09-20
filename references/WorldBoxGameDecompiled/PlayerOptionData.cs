using System;
using System.ComponentModel;

[Serializable]
public class PlayerOptionData
{
	public string name = "OPTION";

	[DefaultValue(true)]
	public bool boolVal = true;

	[DefaultValue("")]
	public string stringVal = string.Empty;

	[DefaultValue(0)]
	public int intVal;

	[NonSerialized]
	public PlayerOptionAction on_switch;

	public PlayerOptionData(string pName)
	{
		name = pName;
	}
}
