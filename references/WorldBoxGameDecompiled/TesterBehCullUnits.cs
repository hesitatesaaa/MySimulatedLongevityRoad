using ai.behaviours;

public class TesterBehCullUnits : BehaviourActionTester
{
	private string _actor_asset_id;

	public TesterBehCullUnits(string pActorAssetId)
	{
		_actor_asset_id = pActorAssetId;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		foreach (Actor unit in AssetManager.actor_library.get(_actor_asset_id).units)
		{
			if (!unit.isRekt() && !Randy.randomChance(0.1f))
			{
				unit.getHit(10000f, pFlash: false, AttackType.Divine);
			}
		}
		return base.execute(pObject);
	}
}
