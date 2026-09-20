public class BenchmarkSprites
{
	public static void start()
	{
		if (!Config.game_loaded || !SelectedUnit.isSet())
		{
			return;
		}
		Actor unit = SelectedUnit.unit;
		if (unit.is_visible)
		{
			int num = 100;
			Bench.bench("sprites_old", "sprites_test");
			for (int i = 0; i < num; i++)
			{
				DynamicSpriteCreator.createNewSpriteUnit(unit.frame_data, unit.calculateMainSprite(), unit.cached_sprite_head, unit.kingdom.getColor(), unit.asset, unit.data.phenotype_index, unit.data.phenotype_shade, UnitTextureAtlasID.Units);
			}
			Bench.benchEnd("sprites_old", "sprites_test", pSaveCounter: true, num);
			Bench.bench("sprites_new", "sprites_test");
			for (int j = 0; j < num; j++)
			{
				DynamicSpriteCreator.createNewSpriteUnit(unit.frame_data, unit.calculateMainSprite(), unit.cached_sprite_head, unit.kingdom.getColor(), unit.asset, unit.data.phenotype_index, unit.data.phenotype_shade, UnitTextureAtlasID.Units);
			}
			Bench.benchEnd("sprites_new", "sprites_test", pSaveCounter: true, num);
		}
	}
}
