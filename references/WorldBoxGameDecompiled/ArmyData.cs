using System.Collections.Generic;
using System.ComponentModel;

public class ArmyData : MetaObjectData
{
	[DefaultValue(-1L)]
	public long id_city = -1L;

	[DefaultValue(-1L)]
	public long id_captain = -1L;

	[DefaultValue(-1L)]
	public long id_kingdom = -1L;

	public List<LeaderEntry> past_captains;

	public override void Dispose()
	{
		base.Dispose();
		past_captains?.Clear();
		past_captains = null;
	}
}
