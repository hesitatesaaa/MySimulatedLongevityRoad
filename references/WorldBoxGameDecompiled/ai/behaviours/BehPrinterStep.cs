namespace ai.behaviours;

public class BehPrinterStep : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		pActor.data.get("template", out var pResult, null);
		pActor.data.get("step", out var pResult2, -1);
		pActor.data.get("origin_x", out var pResult3, 0);
		pActor.data.get("origin_y", out var pResult4, 0);
		PrintTemplate template = PrintLibrary.getTemplate(pResult);
		for (int i = 0; i < template.steps_per_tick; i++)
		{
			if (pResult2 >= template.steps.Length)
			{
				pActor.data.set("step", pResult2);
				return BehResult.Stop;
			}
			PrintStep printStep = template.steps[pResult2];
			WorldTile tile = BehaviourActionBase<Actor>.world.GetTile(pResult3 + printStep.x, pResult4 + printStep.y);
			if (tile != null)
			{
				pActor.spawnOn(tile);
				printTile(pActor);
			}
			pResult2++;
		}
		pActor.data.set("step", pResult2);
		return BehResult.RestartTask;
	}

	private static void printTile(Actor pActor)
	{
		MusicBox.playSound("event:/SFX/UNIQUE/PrinterStep", pActor.current_tile);
		if (pActor.current_tile.top_type != null)
		{
			MapAction.decreaseTile(pActor.current_tile, pDamage: false);
		}
		if (pActor.current_tile.Type.increase_to != null)
		{
			MapAction.terraformMain(pActor.current_tile, pActor.current_tile.Type.increase_to, AssetManager.terraform.get("destroy"));
			BehaviourActionBase<Actor>.world.setTileDirty(pActor.current_tile);
		}
		BehaviourActionBase<Actor>.world.conway_layer.remove(pActor.current_tile);
	}
}
