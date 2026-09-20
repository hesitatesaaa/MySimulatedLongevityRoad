using System;
using System.ComponentModel;

[Serializable]
public class ItemData : BaseSystemData
{
	public int durability = 100;

	public bool created_by_player;

	[DefaultValue("")]
	public string by = string.Empty;

	internal string byColor = string.Empty;

	[DefaultValue("")]
	public string from = string.Empty;

	internal string fromColor = string.Empty;

	[DefaultValue(0)]
	public int kills;

	public string asset_id = string.Empty;

	public string material = string.Empty;

	public readonly ListPool<string> modifiers = new ListPool<string>();

	[DefaultValue(-1L)]
	public long creator_id { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long creator_kingdom_id { get; set; } = -1L;

	public bool ShouldSerializemodifiers()
	{
		return modifiers.Count > 0;
	}

	public override void Dispose()
	{
		modifiers.Dispose();
		base.Dispose();
	}
}
