public class FavoriteItemSpriteSwitcher : SpriteSwitcher
{
	protected override bool hasAny()
	{
		foreach (Item item in World.world.items)
		{
			if (item.isFavorite())
			{
				return true;
			}
		}
		return false;
	}
}
