using UnityEngine;

namespace ai.behaviours;

public class BehPrinterSetup : BehaviourActionActor
{
	public override BehResult execute(Actor pActor)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		pActor.data.get("step", out var pResult, -1);
		if (pResult < 0)
		{
			ActorData data = pActor.data;
			Vector2Int pos = pActor.current_tile.pos;
			data.set("origin_x", ((Vector2Int)(ref pos)).x);
			ActorData data2 = pActor.data;
			pos = pActor.current_tile.pos;
			data2.set("origin_y", ((Vector2Int)(ref pos)).y);
			pActor.data.get("template", out var pResult2, null);
			PrintTemplate template = PrintLibrary.getTemplate(pResult2);
			pActor.data.set("steps", template.steps.Length);
			pActor.data.set("step", 0);
		}
		pActor.data.get("steps", out var pResult3, -1);
		if (pResult >= pResult3)
		{
			pActor.dieSimpleNone();
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
