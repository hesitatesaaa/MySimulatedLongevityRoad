using System.Collections.Generic;

public class BaseModule : BaseMapObject
{
	internal HashSet<WorldTile> hashset;

	protected float timer;

	internal virtual void clear()
	{
	}
}
