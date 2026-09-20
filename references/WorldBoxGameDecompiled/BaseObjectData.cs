using System;
using System.ComponentModel;

[Serializable]
public abstract class BaseObjectData : BaseSystemData
{
	[DefaultValue(100)]
	public int health { get; set; } = 100;
}
