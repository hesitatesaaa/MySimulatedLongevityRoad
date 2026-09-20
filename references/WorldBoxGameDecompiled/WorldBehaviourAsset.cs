using System;
using System.ComponentModel;

[Serializable]
public class WorldBehaviourAsset : Asset
{
	[DefaultValue(true)]
	public bool enabled = true;

	[DefaultValue(true)]
	public bool enabled_on_minimap = true;

	[DefaultValue(1f)]
	public float interval = 1f;

	[DefaultValue(1f)]
	public float interval_random = 1f;

	public WorldBehaviourAction action;

	public WorldBehaviourAction action_world_clear;

	[DefaultValue(true)]
	public bool stop_when_world_on_pause = true;

	[NonSerialized]
	public WorldBehaviour manager;
}
