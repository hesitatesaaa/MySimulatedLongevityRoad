using ai.behaviours;

public class TesterMutateActorTraits : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		using ListPool<ActorTrait> listPool = new ListPool<ActorTrait>();
		foreach (Actor unit in BehaviourActionBase<AutoTesterBot>.world.units)
		{
			if (Randy.randomChance(0.9f))
			{
				continue;
			}
			listPool.Clear();
			listPool.AddRange(unit.getTraits());
			if (listPool.Count > 0)
			{
				ActorTrait random = listPool.GetRandom();
				if (random.can_be_removed)
				{
					unit.removeTrait(random);
				}
			}
			int num = 10;
			while (num-- > 0)
			{
				ActorTrait random2 = AssetManager.traits.list.GetRandom();
				if (random2.can_be_given && !random2.id.Contains("zombie") && !random2.id.Contains("plague") && unit.addTrait(random2))
				{
					break;
				}
			}
		}
		return base.execute(pObject);
	}
}
