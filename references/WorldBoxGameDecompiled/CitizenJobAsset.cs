using System;
using System.ComponentModel;

[Serializable]
public class CitizenJobAsset : Asset
{
	public string path_icon;

	public int priority;

	public int priority_no_food;

	[DefaultValue(true)]
	public bool common_job = true;

	[DefaultValue(true)]
	public bool ok_for_king = true;

	[DefaultValue(true)]
	public bool ok_for_leader = true;

	public bool only_leaders;

	public CitizenJobCondition should_be_assigned;

	public string unit_job_default;

	public DebugOption debug_option;
}
