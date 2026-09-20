using System.Collections.Generic;

public class WildKingdomsManager : MetaSystemManager<Kingdom, KingdomData>
{
	public static Kingdom abandoned;

	public static Kingdom ruins;

	public static Kingdom neutral;

	public static Kingdom nature;

	protected readonly Dictionary<string, Kingdom> _dict = new Dictionary<string, Kingdom>();

	private bool _dirty_buildings = true;

	private long _latest;

	public WildKingdomsManager()
	{
		type_id = "kingdom_wild";
		foreach (KingdomAsset item in AssetManager.kingdoms.list)
		{
			newWildKingdom(item);
		}
		abandoned = get("abandoned");
		ruins = get("ruins");
		nature = get("nature");
		neutral = get("neutral");
		neutral.data.original_actor_asset = "druid";
	}

	public override void startCollectHistoryData()
	{
	}

	public override void clearLastYearStats()
	{
	}

	private Kingdom newWildKingdom(KingdomAsset pAsset)
	{
		long num = _latest--;
		if (num == -1)
		{
			num = _latest--;
		}
		Kingdom kingdom = newObject(num);
		kingdom.asset = pAsset;
		_dict.Add(kingdom.asset.id, kingdom);
		kingdom.createWildKingdom();
		if (pAsset.default_civ_color_index != -1)
		{
			kingdom.data.setColorID(pAsset.default_civ_color_index);
		}
		kingdom.data.name = pAsset.id;
		return kingdom;
	}

	public override void clear()
	{
		using IEnumerator<Kingdom> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Kingdom current = enumerator.Current;
			current.clearListUnits();
			current.clearBuildingList();
		}
	}

	protected override void updateDirtyUnits()
	{
		List<Actor> units_only_wild = World.world.units.units_only_wild;
		for (int i = 0; i < units_only_wild.Count; i++)
		{
			Actor actor = units_only_wild[i];
			Kingdom kingdom = actor.kingdom;
			if (kingdom != null && kingdom.isDirtyUnits())
			{
				kingdom.listUnit(actor);
			}
		}
	}

	public void beginChecksBuildings()
	{
		if (_dirty_buildings)
		{
			updateDirtyBuildings();
		}
		_dirty_buildings = false;
	}

	private void updateDirtyBuildings()
	{
		clearAllBuildingLists();
		foreach (Building building in World.world.buildings)
		{
			if (building.isAlive() && building.kingdom.wild)
			{
				building.kingdom.listBuilding(building);
			}
		}
	}

	public void setDirtyBuildings()
	{
		_dirty_buildings = true;
	}

	private void clearAllBuildingLists()
	{
		using IEnumerator<Kingdom> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			enumerator.Current.clearBuildingList();
		}
	}

	public override void checkDeadObjects()
	{
	}

	public Kingdom get(string pID)
	{
		if (string.IsNullOrEmpty(pID))
		{
			return null;
		}
		_dict.TryGetValue(pID, out var value);
		return value;
	}

	public override void removeObject(Kingdom pObject)
	{
		_dict.Remove(pObject.asset.id);
		base.removeObject(pObject);
	}
}
