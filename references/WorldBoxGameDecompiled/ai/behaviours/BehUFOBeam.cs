using UnityEngine;

namespace ai.behaviours;

public class BehUFOBeam : BehaviourActionActor
{
	private bool enabled;

	public BehUFOBeam(bool pEnabled = false)
	{
		enabled = pEnabled;
	}

	public override BehResult execute(Actor pActor)
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		UFO actorComponent = pActor.getActorComponent<UFO>();
		if (!enabled)
		{
			actorComponent.hideBeam();
			return BehResult.Continue;
		}
		if (actorComponent.beamAnim.isOn)
		{
			if (actorComponent.beamAnim.currentFrameIndex == 4)
			{
				for (int i = 0; i < 8; i++)
				{
					for (int j = 0; j < 8; j++)
					{
						MapBox mapBox = BehaviourActionBase<Actor>.world;
						Vector2Int pos = pActor.current_tile.pos;
						int pX = ((Vector2Int)(ref pos)).x + j - 4;
						pos = pActor.current_tile.pos;
						WorldTile tile = mapBox.GetTile(pX, ((Vector2Int)(ref pos)).y + i - 4);
						if (tile != null)
						{
							pos = pActor.current_tile.pos;
							int x = ((Vector2Int)(ref pos)).x;
							pos = pActor.current_tile.pos;
							int y = ((Vector2Int)(ref pos)).y;
							pos = tile.pos;
							int x2 = ((Vector2Int)(ref pos)).x;
							pos = tile.pos;
							if (!(Toolbox.Dist(x, y, x2, ((Vector2Int)(ref pos)).y) > 4f))
							{
								MapAction.damageWorld(tile, 0, AssetManager.terraform.get("ufo_attack"), pActor);
							}
						}
					}
				}
			}
			if (actorComponent.beamAnim.currentFrameIndex == actorComponent.beamAnim.frames.Length - 1)
			{
				actorComponent.hideBeam();
				return BehResult.Continue;
			}
			return BehResult.RepeatStep;
		}
		actorComponent.startBeam();
		return BehResult.RepeatStep;
	}
}
