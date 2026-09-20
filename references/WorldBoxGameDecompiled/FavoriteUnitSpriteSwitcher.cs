public class FavoriteUnitSpriteSwitcher : SpriteSwitcher
{
	protected override bool hasAny()
	{
		foreach (Actor unit in World.world.units)
		{
			if (unit.isFavorite())
			{
				return true;
			}
		}
		return false;
	}
}
