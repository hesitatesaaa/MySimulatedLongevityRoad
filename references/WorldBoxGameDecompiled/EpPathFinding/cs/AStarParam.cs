namespace EpPathFinding.cs;

public class AStarParam : ParamBase
{
	internal float weight;

	internal int max_open_list = -1;

	internal bool roads;

	internal bool use_global_path_lock;

	internal bool boat;

	internal bool limit;

	internal bool swamp;

	internal bool ocean;

	internal bool lava;

	internal bool fire;

	internal bool block;

	internal bool ground;

	internal bool end_to_start_path;

	public void resetParam()
	{
		swamp = false;
		roads = false;
		ocean = false;
		lava = false;
		ground = false;
		use_global_path_lock = false;
		boat = false;
		limit = false;
		fire = false;
		end_to_start_path = false;
	}

	internal override void _reset(GridPos iStartPos, GridPos iEndPos, BaseGrid iSearchGrid = null)
	{
	}
}
