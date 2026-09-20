using UnityEngine;

namespace ai.behaviours;

public class BehBoatFishing : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		spawnFishnet(pActor);
		return BehResult.Continue;
	}

	public void spawnFishnet(Actor pActor)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (MapBox.isRenderGameplay())
		{
			Vector2 val = Randy.randomPointOnCircle(3f, 4f);
			WorldTile worldTile = null;
			MapBox mapBox = BehaviourActionBase<Actor>.world;
			Vector2Int pos = pActor.current_tile.pos;
			int pX = ((Vector2Int)(ref pos)).x + (int)val.x;
			pos = pActor.current_tile.pos;
			worldTile = mapBox.GetTile(pX, ((Vector2Int)(ref pos)).y + (int)val.y);
			if (worldTile != null && worldTile.Type.ocean)
			{
				EffectsLibrary.spawnAtTile("fx_fishnet", worldTile, pActor.asset.base_stats["scale"]);
			}
		}
	}
}
