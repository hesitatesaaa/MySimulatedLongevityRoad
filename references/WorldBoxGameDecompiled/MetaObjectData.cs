using System.ComponentModel;
using Newtonsoft.Json;

public class MetaObjectData : BaseSystemData
{
	[JsonProperty(/*Could not decode attribute arguments.*/)]
	public int color_id { get; set; }

	[DefaultValue(-1)]
	public int original_color_id { get; set; } = -1;

	public long total_deaths { get; set; }

	public long total_births { get; set; }

	public long total_kills { get; set; }

	public long deaths_natural { get; set; }

	public long deaths_hunger { get; set; }

	public long deaths_eaten { get; set; }

	public long deaths_plague { get; set; }

	public long deaths_poison { get; set; }

	public long deaths_infection { get; set; }

	public long deaths_tumor { get; set; }

	public long deaths_acid { get; set; }

	public long deaths_fire { get; set; }

	public long deaths_divine { get; set; }

	public long deaths_weapon { get; set; }

	public long deaths_gravity { get; set; }

	public long deaths_drowning { get; set; }

	public long deaths_water { get; set; }

	public long deaths_explosion { get; set; }

	public long deaths_other { get; set; }

	public long metamorphosis { get; set; }

	public long evolutions { get; set; }

	public int renown { get; set; }

	public void setColorID(int pColorID)
	{
		color_id = pColorID;
		if (original_color_id == -1)
		{
			original_color_id = color_id;
		}
	}
}
