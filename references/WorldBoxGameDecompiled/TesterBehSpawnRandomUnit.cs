using System.Collections.Generic;
using UnityEngine;
using ai.behaviours;

public class TesterBehSpawnRandomUnit : BehaviourActionTester
{
	private string[] assets;

	private int _amount;

	private string _location;

	public ActorAssetFilter filter_delegate;

	private static readonly List<string> printers = new List<string>
	{
		"hexagon", "skull", "squares", "yinyang", "island1", "star", "heart", "diamond", "aliendrawing", "crater",
		"labyrinth", "spiral", "starfort", "code"
	};

	public TesterBehSpawnRandomUnit(int pAmount = 1, string pLocation = "random")
	{
		_amount = pAmount;
		_location = pLocation;
		filter_delegate = delegate(ActorAsset pActorAsset)
		{
			if (pActorAsset.isTemplateAsset())
			{
				return false;
			}
			if (!pActorAsset.has_ai_system)
			{
				return false;
			}
			if (pActorAsset.is_boat)
			{
				return false;
			}
			if (pActorAsset.unit_other)
			{
				return false;
			}
			if (pActorAsset.special)
			{
				return false;
			}
			return !pActorAsset.id.Contains("zombie");
		};
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		if (assets == null)
		{
			using ListPool<string> listPool = new ListPool<string>();
			foreach (ActorAsset item in AssetManager.actor_library.list)
			{
				if (filter_delegate(item))
				{
					listPool.Add(item.id);
				}
			}
			listPool.Shuffle();
			assets = listPool.ToArray();
		}
		string random = assets.GetRandom();
		TileZone random2 = BehaviourActionBase<AutoTesterBot>.world.zone_calculator.zones.GetRandom();
		for (int i = 0; i < _amount; i++)
		{
			WorldTile worldTile = null;
			worldTile = ((!(_location == "tile_target") || pObject.beh_tile_target == null) ? random2.tiles.GetRandom() : pObject.beh_tile_target);
			Actor actor = BehaviourActionBase<AutoTesterBot>.world.units.spawnNewUnit(random, worldTile, pSpawnSound: false, pMiracleSpawn: true);
			if (actor == null)
			{
				Debug.LogError((object)("could not spawn " + random));
			}
			else if (random == "printer")
			{
				actor.data.set("template", printers.GetRandom());
			}
		}
		return base.execute(pObject);
	}
}
