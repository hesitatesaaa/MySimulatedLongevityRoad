using System.ComponentModel;

public class PlotData : MetaObjectData
{
	public string plot_type_id;

	public string founder_name;

	[DefaultValue(-1L)]
	public long founder_id = -1L;

	[DefaultValue(-1L)]
	public long id_initiator_actor = -1L;

	[DefaultValue(-1L)]
	public long id_initiator_city = -1L;

	[DefaultValue(-1L)]
	public long id_initiator_kingdom = -1L;

	[DefaultValue(-1L)]
	public long id_target_actor = -1L;

	[DefaultValue(-1L)]
	public long id_target_city = -1L;

	[DefaultValue(-1L)]
	public long id_target_kingdom = -1L;

	[DefaultValue(-1L)]
	public long id_target_alliance = -1L;

	[DefaultValue(-1L)]
	public long id_target_war = -1L;

	public bool forced;

	public float progress_current;

	public override void Dispose()
	{
		base.Dispose();
	}
}
