using ai.behaviours;

public class TesterMutateSubspeciesGenes : BehaviourActionTester
{
	public override BehResult execute(AutoTesterBot pObject)
	{
		foreach (Subspecies subspecy in BehaviourActionBase<AutoTesterBot>.world.subspecies)
		{
			if (!Randy.randomChance(0.9f))
			{
				subspecy.nucleus.doRandomGeneMutations(2);
				subspecy.mutateTraits(1);
				subspecy.unstableGenomeEvent();
			}
		}
		return base.execute(pObject);
	}
}
