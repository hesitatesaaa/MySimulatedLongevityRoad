namespace ai.behaviours;

public class BehCheckCure : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		if (!pActor.current_tile.Type.ground)
		{
			return BehResult.Stop;
		}
		Actor actor = null;
		foreach (Actor item in Finder.getUnitsFromChunk(pActor.current_tile, 1))
		{
			if (ActorTool.canBeCuredFromTraitsOrStatus(item))
			{
				actor = item;
				break;
			}
		}
		if (actor == null)
		{
			return BehResult.Stop;
		}
		AssetManager.spells.get("cast_cure").action?.Invoke(pActor, actor, actor.current_tile);
		pActor.doCastAnimation();
		return BehResult.Continue;
	}
}
