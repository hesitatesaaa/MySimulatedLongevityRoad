public class Wheat : BaseBuildingComponent
{
	private int _current_level;

	private int _max_level;

	internal override void create(Building pBuilding)
	{
		base.create(pBuilding);
		_current_level = 0;
		_max_level = building.asset.building_sprites.animation_data.Count - 1;
		checkSprite();
	}

	public override void update(float pElapsed)
	{
		building.data.grow_time += pElapsed;
		if (!isMaxLevel())
		{
			float num = building.asset.growth_time * (float)(_current_level + 1);
			if (!(building.data.grow_time < num))
			{
				grow();
			}
		}
	}

	internal void grow()
	{
		if (_current_level < _max_level)
		{
			_current_level++;
			checkSprite();
		}
		MusicBox.playSound("event:/SFX/DROPS/DropSeedGrass", building.current_tile, pGameViewOnly: true, pVisibleOnly: true);
		if (!World.world_era.flag_crops_grow && Randy.randomBool())
		{
			building.startDestroyBuilding();
		}
	}

	public void growFull()
	{
		_current_level = _max_level;
		checkSprite();
		MusicBox.playSound("event:/SFX/DROPS/DropSeedGrass", building.current_tile, pGameViewOnly: true, pVisibleOnly: true);
	}

	private void checkSprite()
	{
		building.setAnimData(_current_level);
		if (building.asset.random_flip && !building.asset.shadow)
		{
			building.flip_x = Randy.randomBool();
		}
		building.setScaleTween();
		World.world.setTileDirty(building.current_tile);
	}

	public bool isMaxLevel()
	{
		return _current_level == _max_level;
	}

	public int getCurrentLevel()
	{
		return _current_level;
	}
}
