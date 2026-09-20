using System.Collections.Generic;
using UnityEngine;

public class TileLibraryMain<T> : AssetLibrary<T> where T : TileTypeBase
{
	public override void linkAssets()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		base.linkAssets();
		foreach (T item in list)
		{
			HashSet<BiomeTag> biome_tags = item.biome_tags;
			item.has_biome_tags = biome_tags != null && biome_tags.Count > 0;
			if (item.color_hex != null)
			{
				item.color = Color32.op_Implicit(Toolbox.makeColor(item.color_hex));
			}
			if (item.edge_color_hex != null)
			{
				item.edge_color = Color32.op_Implicit(Toolbox.makeColor(item.edge_color_hex));
			}
		}
	}
}
