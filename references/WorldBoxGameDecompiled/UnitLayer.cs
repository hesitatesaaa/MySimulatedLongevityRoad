using System.Collections.Generic;
using UnityEngine;

public class UnitLayer : MapLayer
{
	private List<WorldTile> prevTiles;

	private float interval;

	private Color32 dead;

	private Color32 _color_clear;

	internal override void create()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		dead = Color32.op_Implicit(Toolbox.makeColor("#393939"));
		prevTiles = new List<WorldTile>();
		base.create();
	}

	internal override void clear()
	{
		prevTiles.Clear();
		base.clear();
	}

	protected override void UpdateDirty(float pElapsed)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		if (MapBox.isRenderGameplay())
		{
			timer = 0f;
			return;
		}
		if (timer > 0f)
		{
			timer -= pElapsed;
			return;
		}
		timer = interval;
		for (int i = 0; i < prevTiles.Count; i++)
		{
			WorldTile worldTile = prevTiles[i];
			pixels[worldTile.data.tile_id] = _color_clear;
		}
		prevTiles.Clear();
		bool flag = PlayerConfig.optionBoolEnabled("marks_boats");
		bool flag2 = Zones.showCultureZones();
		if (World.world.isAnyPowerSelected() && !Zones.isPowerForceMapMode())
		{
			flag2 = false;
		}
		bool flag3 = Zones.showClanZones();
		bool flag4 = Zones.showAllianceZones();
		List<Actor> simpleList = World.world.units.getSimpleList();
		for (int j = 0; j < simpleList.Count; j++)
		{
			Actor actor = simpleList[j];
			if (actor.asset.visible_on_minimap || !actor.asset.color.HasValue || actor.is_inside_building)
			{
				continue;
			}
			prevTiles.Add(actor.current_tile);
			if (!actor.isAlive())
			{
				pixels[actor.current_tile.data.tile_id] = dead;
				continue;
			}
			if (flag2)
			{
				if (actor.hasCulture())
				{
					pixels[actor.current_tile.data.tile_id] = actor.culture.getColor().getColorUnit32();
				}
				continue;
			}
			if (flag3)
			{
				if (actor.hasClan())
				{
					pixels[actor.current_tile.data.tile_id] = actor.clan.getColor().getColorUnit32();
				}
				continue;
			}
			if (flag4)
			{
				Alliance alliance = World.world.alliances.get(actor.kingdom.data.allianceID);
				if (alliance != null)
				{
					pixels[actor.current_tile.data.tile_id] = alliance.getColor().getColorUnit32();
					continue;
				}
			}
			if ((actor.asset.is_boat || actor.isSapient()) && actor.hasKingdom() && actor.isKingdomCiv())
			{
				if (!flag || !actor.asset.draw_boat_mark)
				{
					pixels[actor.current_tile.data.tile_id] = actor.kingdom.getColor().getColorUnit32();
				}
			}
			else
			{
				pixels[actor.current_tile.data.tile_id] = actor.asset.color.Value;
			}
		}
		updatePixels();
	}

	public UnitLayer()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		interval = 0.1f;
		dead = Color32.op_Implicit(new Color(0f, 0f, 0f, 0.5f));
		_color_clear = Color32.op_Implicit(Color.clear);
		base._002Ector();
	}
}
