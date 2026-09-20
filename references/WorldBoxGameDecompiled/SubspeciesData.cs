using System.Collections.Generic;
using System.ComponentModel;

public class SubspeciesData : MetaObjectData
{
	public List<ChromosomeData> saved_chromosome_data;

	public List<string> saved_traits;

	public List<string> saved_actor_birth_traits;

	public string biome_variant = "default_color";

	public int banner_background_id;

	public string species_id = "human";

	[DefaultValue(-1L)]
	public long parent_subspecies = -1L;

	[DefaultValue(-1L)]
	public long evolved_into_subspecies = -1L;

	[DefaultValue(0)]
	public int evolved_into_subspecies_next_spawn;

	public double last_evolution_timestamp;

	public int skin_id;

	public int mutation;

	[DefaultValue(1)]
	public int generation = 1;

	[DefaultValue(-1L)]
	public long skeleton_form_id = -1L;

	public override void Dispose()
	{
		base.Dispose();
		saved_chromosome_data?.Clear();
		saved_chromosome_data = null;
		saved_traits?.Clear();
		saved_traits = null;
		saved_actor_birth_traits?.Clear();
		saved_actor_birth_traits = null;
	}
}
