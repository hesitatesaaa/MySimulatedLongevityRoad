using System;
using Beebyte.Obfuscator;
using SQLite;
using UnityEngine;

[Serializable]
[Preserve]
[Skip]
public class WorldLogMessage
{
	[NotNull]
	public string asset_id { get; set; }

	[NotNull]
	public int timestamp { get; set; }

	public string special1 { get; set; }

	public string special2 { get; set; }

	public string special3 { get; set; }

	public string color_special_1 { get; set; }

	public string color_special_2 { get; set; }

	public string color_special_3 { get; set; }

	public long unit_id { get; set; } = -1L;

	public long kingdom_id { get; set; } = -1L;

	public int? x { get; set; }

	public int? y { get; set; }

	[Ignore]
	public Vector2 location
	{
		get
		{
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			if (!x.HasValue || !y.HasValue)
			{
				return new Vector2(-1f, -1f);
			}
			return new Vector2((float)x.Value, (float)y.Value);
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			x = (int)value.x;
			y = (int)value.y;
		}
	}

	[Ignore]
	public Actor unit
	{
		get
		{
			return World.world.units.get(unit_id);
		}
		set
		{
			unit_id = value?.getID() ?? (-1);
		}
	}

	[Ignore]
	public Kingdom kingdom
	{
		get
		{
			return World.world.kingdoms.get(kingdom_id);
		}
		set
		{
			kingdom_id = value?.getID() ?? (-1);
		}
	}

	[Ignore]
	public Color color_special1
	{
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			color_special_1 = Toolbox.colorToHex(Color32.op_Implicit(value), pAlpha: false);
		}
	}

	[Ignore]
	public Color color_special2
	{
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			color_special_2 = Toolbox.colorToHex(Color32.op_Implicit(value), pAlpha: false);
		}
	}

	[Ignore]
	public Color color_special3
	{
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			color_special_3 = Toolbox.colorToHex(Color32.op_Implicit(value), pAlpha: false);
		}
	}

	public WorldLogMessage()
	{
	}

	public WorldLogMessage(WorldLogAsset pAsset, string pSpecial1 = null, string pSpecial2 = null, string pSpecial3 = null)
	{
		asset_id = pAsset.id;
		special1 = pSpecial1;
		special2 = pSpecial2;
		special3 = pSpecial3;
		x = null;
		y = null;
		unit_id = -1L;
		color_special_1 = null;
		color_special_2 = null;
		color_special_3 = null;
		timestamp = (int)World.world.getCurWorldTime();
		unit = null;
	}
}
