using UnityEngine;

public class TileManager
{
	public int[] random_seeds;

	public int[] fire_animation_set;

	public bool[] fires;

	public Vector3[] positions_vector3;

	public int tiles_count;

	public void setup(int pWidth, int pHeight, WorldTile[,] pTilesMap)
	{
		tiles_count = pWidth * pHeight;
		setupAnimationSeeds();
		setupVector3Positions();
		setupFires();
	}

	private void setupFires()
	{
		if (fires == null || fires.Length != tiles_count)
		{
			fires = new bool[tiles_count];
		}
	}

	private void setupVector3Positions()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (positions_vector3 == null || positions_vector3.Length != tiles_count)
		{
			positions_vector3 = (Vector3[])(object)new Vector3[tiles_count];
		}
		WorldTile[] tiles_list = World.world.tiles_list;
		foreach (WorldTile worldTile in tiles_list)
		{
			positions_vector3[worldTile.data.tile_id] = worldTile.posV3;
		}
	}

	private void setupAnimationSeeds()
	{
		if (random_seeds != null && random_seeds.Length == tiles_count)
		{
			return;
		}
		random_seeds = new int[tiles_count];
		fire_animation_set = new int[tiles_count];
		for (int i = 0; i < random_seeds.Length; i++)
		{
			random_seeds[i] = Randy.randomInt(0, 10000);
			if (random_seeds[i] % 6 == 0)
			{
				fire_animation_set[i] = 2;
			}
			else if (random_seeds[i] % 3 == 0)
			{
				fire_animation_set[i] = 1;
			}
			else
			{
				fire_animation_set[i] = 0;
			}
		}
	}

	public void clear()
	{
		fires?.Clear();
	}
}
