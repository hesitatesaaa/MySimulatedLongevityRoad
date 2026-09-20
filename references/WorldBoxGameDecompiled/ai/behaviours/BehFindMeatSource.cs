namespace ai.behaviours;

public class BehFindMeatSource : BehaviourActionActor
{
	private MeatTargetType _meat_target_type;

	private bool _check_for_factions;

	public BehFindMeatSource(MeatTargetType pMeatTargetType = MeatTargetType.Meat, bool pCheckForFactions = true)
	{
		_check_for_factions = pCheckForFactions;
		_meat_target_type = pMeatTargetType;
	}

	public override BehResult execute(Actor pActor)
	{
		if (pActor.beh_actor_target != null && isTargetOk(pActor, pActor.beh_actor_target.a))
		{
			return BehResult.Continue;
		}
		pActor.beh_actor_target = getClosestMeatActor(pActor);
		if (pActor.beh_actor_target != null)
		{
			return BehResult.Continue;
		}
		return BehResult.Stop;
	}

	private Actor getClosestMeatActor(Actor pActor)
	{
		bool flag = Randy.randomBool();
		WorldTile current_tile = pActor.current_tile;
		float num = 2.1474836E+09f;
		Actor result = null;
		int pChunkRadius = Randy.randomInt(1, 3);
		foreach (Actor item in Finder.getUnitsFromChunk(current_tile, pChunkRadius, 0f, flag))
		{
			float num2 = Toolbox.SquaredDistTile(item.current_tile, current_tile);
			if (num2 >= num || !isTargetOk(pActor, item))
			{
				continue;
			}
			bool flag2 = item.isSameSpecies(pActor.asset.id);
			switch (_meat_target_type)
			{
			case MeatTargetType.Meat:
				if (!item.asset.source_meat || flag2)
				{
					continue;
				}
				break;
			case MeatTargetType.MeatSameSpecies:
				if (!flag2)
				{
					continue;
				}
				break;
			case MeatTargetType.Insect:
				if (!item.asset.source_meat_insect || flag2)
				{
					continue;
				}
				break;
			}
			num = num2;
			result = item;
			if (flag && Randy.randomBool())
			{
				break;
			}
		}
		return result;
	}

	private bool isTargetOk(Actor pActor, Actor pTarget)
	{
		if (pTarget == pActor)
		{
			return false;
		}
		if (!pActor.canAttackTarget(pTarget, _check_for_factions))
		{
			return false;
		}
		if (pTarget.asset.actor_size > pActor.asset.actor_size)
		{
			return false;
		}
		if (!pTarget.current_tile.isSameIsland(pActor.current_tile))
		{
			return false;
		}
		return true;
	}
}
