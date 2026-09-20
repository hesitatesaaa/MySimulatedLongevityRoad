namespace ai.behaviours;

public class BehFingerGoToCircleTarget : BehFinger
{
	private int _min_range;

	private int _max_range;

	public BehFingerGoToCircleTarget(int pMinRange = 20, int pMaxRange = 25)
	{
		_min_range = pMinRange;
		_max_range = pMaxRange;
	}

	public override BehResult execute(Actor pActor)
	{
		WorldTile current_tile = pActor.current_tile;
		int num = Randy.randomInt(_min_range, _max_range);
		using ListPool<WorldTile> item = new ListPool<WorldTile>
		{
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x - num / 2, current_tile.y + num / 2),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x - num, current_tile.y),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x - num / 2, current_tile.y - num / 2)
		};
		using ListPool<WorldTile> item2 = new ListPool<WorldTile>
		{
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x + num / 2, current_tile.y + num / 2),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x + num, current_tile.y),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x + num / 2, current_tile.y - num / 2)
		};
		using ListPool<WorldTile> item3 = new ListPool<WorldTile>
		{
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x - num / 2, current_tile.y + num / 2),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x, current_tile.y + num),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x + num / 2, current_tile.y + num / 2)
		};
		using ListPool<WorldTile> item4 = new ListPool<WorldTile>
		{
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x - num / 2, current_tile.y - num / 2),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x, current_tile.y - num),
			BehaviourActionBase<Actor>.world.GetTile(current_tile.x + num / 2, current_tile.y - num / 2)
		};
		using ListPool<ListPool<WorldTile>> listPool = new ListPool<ListPool<WorldTile>> { item, item2, item3, item4 };
		listPool.RemoveAll((ListPool<WorldTile> tList) => tList.Contains(null));
		if (listPool.Count == 0)
		{
			return BehResult.Stop;
		}
		ListPool<WorldTile> random = listPool.GetRandom();
		if (ActorMove.goToCurved(pActor, pActor.current_tile, random[0], random[1], random[2], pActor.current_tile) == ExecuteEvent.False)
		{
			return BehResult.Stop;
		}
		return BehResult.Continue;
	}
}
