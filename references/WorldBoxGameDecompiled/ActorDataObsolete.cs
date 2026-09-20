using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine.Scripting;

[Preserve]
public class ActorDataObsolete
{
	public List<long> saved_items;

	[DefaultValue(null)]
	public ActorBag inventory;

	public ActorData status;

	[DefaultValue(-1L)]
	public long cityID = -1L;

	public int x;

	public int y;
}
