using Newtonsoft.Json;

public readonly struct NameEntry
{
	[JsonProperty]
	public readonly int color_id;

	[JsonProperty]
	public readonly string name;

	[JsonProperty]
	public readonly double timestamp;

	[JsonProperty]
	public readonly bool custom;

	public NameEntry(string pName, bool pCustom)
	{
		name = pName;
		color_id = -1;
		timestamp = (int)World.world.getCurWorldTime();
		custom = pCustom;
	}

	public NameEntry(string pName, bool pCustom, int pColorId)
	{
		name = pName;
		color_id = pColorId;
		timestamp = (int)World.world.getCurWorldTime();
		custom = pCustom;
	}

	public NameEntry(string pName, bool pCustom, double pTimestamp)
	{
		name = pName;
		color_id = -1;
		timestamp = (int)pTimestamp;
		custom = pCustom;
	}

	public NameEntry(string pName, bool pCustom, int pColorId, double pTimestamp)
	{
		name = pName;
		color_id = pColorId;
		timestamp = (int)pTimestamp;
		custom = pCustom;
	}
}
