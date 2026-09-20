using ai.behaviours;

public class TesterMutateActorStatus : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		using ListPool<Status> listPool = new ListPool<Status>();
		foreach (Actor unit in BehaviourActionBase<AutoTesterBot>.world.units)
		{
			if (!unit.hasSubspecies() || Randy.randomChance(0.95f))
			{
				continue;
			}
			if (unit.hasAnyStatusEffectRaw())
			{
				listPool.Clear();
				listPool.AddRange(unit.getStatuses());
				if (listPool.Count > 0)
				{
					unit.finishStatusEffect(listPool.GetRandom().asset.id);
				}
			}
			else
			{
				int num = 10;
				while (!unit.addStatusEffect(AssetManager.status.list.GetRandom()) && num-- > 0)
				{
				}
			}
		}
		return base.execute(pObject);
	}
}
