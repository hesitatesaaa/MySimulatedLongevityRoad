public class BenchmarkFieldAccess
{
	public static void start()
	{
		if (Config.game_loaded)
		{
			int num = 100000;
			Bench.bench("field_acess_test", "field_acess_total");
			Bench.bench("field_access", "field_acess_test");
			int num2 = 0;
			for (int i = 0; i < num; i++)
			{
				num2 += World.world.tiles_list.Length;
			}
			Bench.benchEnd("field_access", "field_acess_test", pSaveCounter: true, num);
			Bench.bench("temp_var", "field_acess_test");
			num2 = 0;
			MapBox world = World.world;
			for (int j = 0; j < num; j++)
			{
				num2 += world.tiles_list.Length;
			}
			Bench.benchEnd("temp_var", "field_acess_test", pSaveCounter: true, num);
			Bench.bench("temp_var_2", "field_acess_test");
			num2 = 0;
			WorldTile[] tiles_list = World.world.tiles_list;
			for (int k = 0; k < num; k++)
			{
				int num3 = tiles_list.Length;
				num2 += num3;
			}
			Bench.benchEnd("temp_var_2", "field_acess_test", pSaveCounter: true, num);
			Bench.bench("result_len", "field_acess_test");
			num2 = 0;
			int num4 = World.world.tiles_list.Length;
			for (int l = 0; l < num; l++)
			{
				num2 += num4;
			}
			Bench.benchEnd("result_len", "field_acess_test", pSaveCounter: true, num);
			Bench.benchEnd("field_acess_test", "field_acess_total", pSaveCounter: false, 0L);
		}
	}
}
