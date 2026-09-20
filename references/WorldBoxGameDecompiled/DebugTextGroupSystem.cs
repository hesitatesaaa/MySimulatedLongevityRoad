using FMOD;
using FMOD.Studio;
using UnityEngine;

public class DebugTextGroupSystem : SpriteGroupSystem<GroupSpriteObject>
{
	private Vector2 _pos;

	public override void create()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		base.create();
		((Object)((Component)this).transform).name = "Debug Text";
		GameObject val = (GameObject)Resources.Load("Prefabs/PrefabDebugText");
		prefab = val.GetComponent<GroupSpriteObject>();
	}

	protected override GroupSpriteObject createNew()
	{
		GroupSpriteObject groupSpriteObject = base.createNew();
		((Component)groupSpriteObject).GetComponent<DebugWorldText>().create();
		return groupSpriteObject;
	}

	public override void update(float pElapsed)
	{
		prepare();
		checkSoundsAttached();
		checkSounds();
		checkSoundsPlaying();
		checkActors();
		checkBoats();
		checkBuildings();
		checkCitiesOverlay();
		checkCitiesTasksOverlay();
		checkKingdoms();
		checkArmies();
		checkZones();
		base.update(pElapsed);
	}

	private void checkSoundsPlaying()
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugConfig.isOn(DebugOption.OverlaySoundsActive) || MapBox.isRenderMiniMap())
		{
			return;
		}
		foreach (DebugMusicBoxData item in MusicBox.inst.debug_box.list)
		{
			if (item.isPlaying())
			{
				GroupSpriteObject next = getNext();
				_pos.x = item.x;
				_pos.y = item.y;
				((Component)next).GetComponent<DebugWorldText>().setTextFmodSound(item, Color.green);
				next.setPosOnly(ref _pos);
			}
		}
	}

	private void checkSounds()
	{
		if (!DebugConfig.isOn(DebugOption.OverlaySounds) || MapBox.isRenderMiniMap())
		{
			return;
		}
		foreach (DebugMusicBoxData item in MusicBox.inst.debug_box.list)
		{
			GroupSpriteObject next = getNext();
			_pos.x = item.x;
			_pos.y = item.y;
			((Component)next).GetComponent<DebugWorldText>().setTextFmodSound(item);
			next.setPosOnly(ref _pos);
		}
	}

	private void checkSoundsAttached()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugConfig.isOn(DebugOption.OverlaySoundsAttached) || MapBox.isRenderMiniMap())
		{
			return;
		}
		ATTRIBUTES_3D val = default(ATTRIBUTES_3D);
		foreach (EventInstance value in MusicBox.inst.idle.currentAttachedSounds.Values)
		{
			EventInstance current = value;
			GroupSpriteObject next = getNext();
			((EventInstance)(ref current)).get3DAttributes(ref val);
			_pos.x = val.position.x;
			_pos.y = val.position.y;
			((Component)next).GetComponent<DebugWorldText>().setTextFmodSound(current);
			next.setPosOnly(ref _pos);
		}
		ATTRIBUTES_3D val2 = default(ATTRIBUTES_3D);
		foreach (QuantumSpriteAsset item in AssetManager.quantum_sprites.list)
		{
			int num = item.group_system.countActive();
			QuantumSprite[] all = item.group_system.getAll();
			for (int i = 0; i < num; i++)
			{
				QuantumSprite quantumSprite = all[i];
				if (((EventInstance)(ref quantumSprite.fmod_instance)).isValid())
				{
					((EventInstance)(ref quantumSprite.fmod_instance)).get3DAttributes(ref val2);
					_pos.x = val2.position.x;
					_pos.y = val2.position.y;
					GroupSpriteObject next2 = getNext();
					((Component)next2).GetComponent<DebugWorldText>().setTextFmodSound(quantumSprite.fmod_instance);
					next2.setPosOnly(ref _pos);
				}
			}
		}
		Actor[] array = World.world.units.visible_units.array;
		int count = World.world.units.visible_units.count;
		ATTRIBUTES_3D val3 = default(ATTRIBUTES_3D);
		for (int j = 0; j < count; j++)
		{
			Actor actor = array[j];
			if (actor.idle_loop_sound != null && ((EventInstance)(ref actor.idle_loop_sound.fmod_instance)).isValid())
			{
				((EventInstance)(ref actor.idle_loop_sound.fmod_instance)).get3DAttributes(ref val3);
				_pos.x = val3.position.x;
				_pos.y = val3.position.y;
				GroupSpriteObject next3 = getNext();
				((Component)next3).GetComponent<DebugWorldText>().setTextFmodSound(actor.idle_loop_sound.fmod_instance);
				next3.setPosOnly(ref _pos);
			}
		}
	}

	private void checkBoats()
	{
		if (!DebugConfig.isOn(DebugOption.OverlayBoatTransport))
		{
			return;
		}
		foreach (Actor unit in World.world.units)
		{
			bool flag = false;
			if (unit.asset.is_boat)
			{
				flag = true;
			}
			if (flag)
			{
				GroupSpriteObject next = getNext();
				_pos.x = unit.current_position.x;
				_pos.y = unit.current_position.y;
				((Component)next).GetComponent<DebugWorldText>().setTextBoat(unit);
				next.setPosOnly(ref _pos);
			}
		}
	}

	private void checkActors()
	{
		if ((!DebugConfig.isOn(DebugOption.OverlayActorCivs) && !DebugConfig.isOn(DebugOption.OverlayCursorActor) && !DebugConfig.isOn(DebugOption.OverlayActorGroupLeaderOnly) && !DebugConfig.isOn(DebugOption.OverlayActorFavoritesOnly) && !DebugConfig.isOn(DebugOption.OverlayActorMobs)) || MapBox.isRenderMiniMap())
		{
			return;
		}
		Actor[] array = World.world.units.visible_units.array;
		int count = World.world.units.visible_units.count;
		for (int i = 0; i < count; i++)
		{
			Actor actor = array[i];
			bool flag = false;
			if (DebugConfig.isOn(DebugOption.OverlayCursorActor) && UnitSelectionEffect.last_actor == actor)
			{
				flag = true;
			}
			if (DebugConfig.isOn(DebugOption.OverlayActorFavoritesOnly) && actor.isFavorite())
			{
				flag = true;
			}
			if (DebugConfig.isOn(DebugOption.OverlayActorGroupLeaderOnly) && actor.is_army_captain)
			{
				flag = true;
			}
			if (actor.isSapient() && DebugConfig.isOn(DebugOption.OverlayActorCivs))
			{
				flag = true;
			}
			if (!actor.isSapient() && DebugConfig.isOn(DebugOption.OverlayActorMobs))
			{
				flag = true;
			}
			if (flag)
			{
				GroupSpriteObject next = getNext();
				_pos.x = actor.current_position.x;
				_pos.y = actor.current_position.y;
				((Component)next).GetComponent<DebugWorldText>().setTextActor(actor);
				next.setPosOnly(ref _pos);
			}
		}
	}

	private void checkBuildings()
	{
		if ((!DebugConfig.isOn(DebugOption.OverlayTrees) && !DebugConfig.isOn(DebugOption.OverlayPlants) && !DebugConfig.isOn(DebugOption.OverlayCivBuildings) && !DebugConfig.isOn(DebugOption.OverlayOtherBuildings)) || MapBox.isRenderMiniMap())
		{
			return;
		}
		int num = World.world.buildings.countVisibleBuildings();
		Building[] visibleBuildings = World.world.buildings.getVisibleBuildings();
		for (int i = 0; i < num; i++)
		{
			Building building = visibleBuildings[i];
			if (building.asset.city_building)
			{
				if (!DebugConfig.isOn(DebugOption.OverlayCivBuildings))
				{
					continue;
				}
			}
			else if (building.asset.building_type == BuildingType.Building_Tree)
			{
				if (!DebugConfig.isOn(DebugOption.OverlayTrees))
				{
					continue;
				}
			}
			else if (building.asset.building_type == BuildingType.Building_Plant)
			{
				if (!DebugConfig.isOn(DebugOption.OverlayPlants))
				{
					continue;
				}
			}
			else if (!DebugConfig.isOn(DebugOption.OverlayOtherBuildings))
			{
				continue;
			}
			GroupSpriteObject next = getNext();
			_pos.x = building.current_position.x;
			_pos.y = building.current_position.y;
			((Component)next).GetComponent<DebugWorldText>().setTextBuilding(building);
			next.setPosOnly(ref _pos);
		}
	}

	private void checkZones()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if (!DebugConfig.isOn(DebugOption.DebugZones))
		{
			return;
		}
		foreach (TileZone zone in World.world.zone_calculator.zones)
		{
			if (zone.debug_show)
			{
				GroupSpriteObject next = getNext();
				ref Vector2 pos = ref _pos;
				Vector2Int pos2 = zone.centerTile.pos;
				pos.x = ((Vector2Int)(ref pos2)).x;
				ref Vector2 pos3 = ref _pos;
				pos2 = zone.centerTile.pos;
				pos3.y = ((Vector2Int)(ref pos2)).y;
				((Component)next).GetComponent<DebugWorldText>().setTextZone(zone);
				next.setPosOnly(ref _pos);
			}
		}
	}

	private void checkArmies()
	{
		if (!DebugConfig.isOn(DebugOption.OverlayArmies) || MapBox.isRenderMiniMap())
		{
			return;
		}
		foreach (Army army in World.world.armies)
		{
			if (army.hasCaptain())
			{
				Actor captain = army.getCaptain();
				GroupSpriteObject next = getNext();
				_pos.x = captain.current_position.x;
				_pos.y = captain.current_position.y;
				((Component)next).GetComponent<DebugWorldText>().setTextArmy(army);
				next.setPosOnly(ref _pos);
			}
		}
	}

	private void checkCitiesOverlay()
	{
		if (!DebugConfig.isOn(DebugOption.OverlayCity))
		{
			return;
		}
		foreach (City city in World.world.cities)
		{
			GroupSpriteObject next = getNext();
			_pos.x = city.city_center.x;
			_pos.y = city.city_center.y;
			((Component)next).GetComponent<DebugWorldText>().setTextCity(city);
			next.setPosOnly(ref _pos);
		}
	}

	private void checkCitiesTasksOverlay()
	{
		if (!DebugConfig.isOn(DebugOption.OverlayCityTasks))
		{
			return;
		}
		foreach (City city in World.world.cities)
		{
			GroupSpriteObject next = getNext();
			_pos.x = city.city_center.x;
			_pos.y = city.city_center.y;
			((Component)next).GetComponent<DebugWorldText>().setTextCityTasks(city);
			next.setPosOnly(ref _pos);
		}
	}

	private void checkKingdoms()
	{
		if (!DebugConfig.isOn(DebugOption.OverlayKingdom))
		{
			return;
		}
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (kingdom.hasCapital())
			{
				GroupSpriteObject next = getNext();
				_pos.x = kingdom.capital.city_center.x;
				_pos.y = kingdom.capital.city_center.y;
				((Component)next).GetComponent<DebugWorldText>().setTextKingdom(kingdom);
				next.setPosOnly(ref _pos);
			}
		}
	}
}
