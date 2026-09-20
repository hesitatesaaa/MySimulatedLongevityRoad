using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class WarData : MetaObjectData
{
	public List<long> list_attackers = new List<long>();

	public List<long> list_defenders = new List<long>();

	public List<long> past_attackers = new List<long>();

	public List<long> past_defenders = new List<long>();

	public List<long> died_attackers = new List<long>();

	public List<long> died_defenders = new List<long>();

	[DefaultValue(WarWinner.Nobody)]
	public WarWinner winner;

	[DefaultValue(-1L)]
	public long main_attacker { get; set; } = -1L;

	[DefaultValue(-1L)]
	public long main_defender { get; set; } = -1L;

	public int dead_attackers { get; set; }

	public int dead_defenders { get; set; }

	public string started_by_actor_name { get; set; } = string.Empty;

	[DefaultValue(-1L)]
	public long started_by_actor_id { get; set; } = -1L;

	public string started_by_kingdom_name { get; set; } = string.Empty;

	[DefaultValue(-1L)]
	public long started_by_kingdom_id { get; set; } = -1L;

	[DefaultValue("normal")]
	public string war_type { get; set; } = "normal";

	public override void Dispose()
	{
		list_attackers.Clear();
		list_defenders.Clear();
		base.Dispose();
	}
}
