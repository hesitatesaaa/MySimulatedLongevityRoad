using ai.behaviours;

public class TesterMutateSubspeciesTraits : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		using ListPool<SubspeciesTrait> listPool = new ListPool<SubspeciesTrait>();
		foreach (Subspecies subspecy in BehaviourActionBase<AutoTesterBot>.world.subspecies)
		{
			if (Randy.randomChance(0.9f))
			{
				continue;
			}
			listPool.Clear();
			listPool.AddRange(subspecy.getTraits());
			if (listPool.Count > 0)
			{
				subspecy.removeTrait(listPool.GetRandom());
			}
			int num = 10;
			for (int i = 0; i < num; i++)
			{
				SubspeciesTrait random = AssetManager.subspecies_traits.list.GetRandom();
				if (random.can_be_given && subspecy.addTrait(random))
				{
					break;
				}
			}
		}
		return base.execute(pObject);
	}
}
