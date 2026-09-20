using System;
using UnityEngine;

[Serializable]
public class ColorStyleAsset : Asset
{
	public string taxonomy_kingdom;

	public string taxonomy_phylum;

	public string taxonomy_subphylum;

	public string taxonomy_class;

	public string taxonomy_order;

	public string taxonomy_family;

	public string taxonomy_genus;

	public string taxonomy_common_name;

	public string color_text_grey;

	public string color_text_grey_dark;

	public string color_text_selector;

	public string color_text_selector_remove;

	public string color_text_pumpkin;

	public string color_text_pumpkin_light;

	public Color favorite_selected;

	public Color favorite_not_selected;

	public Color health_bar_main_green;

	public Color health_bar_main_red;

	public Color health_bar_background;

	public string color_dead_text => color_text_grey_dark;

	public Color getSelectorColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Toolbox.makeColor(color_text_selector);
	}

	public Color getSelectorRemoveColor()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return Toolbox.makeColor(color_text_selector_remove);
	}

	public string getColorForTaxonomy(string pID)
	{
		return pID switch
		{
			"taxonomy_kingdom" => taxonomy_kingdom, 
			"taxonomy_phylum" => taxonomy_phylum, 
			"taxonomy_subphylum" => taxonomy_subphylum, 
			"taxonomy_class" => taxonomy_class, 
			"taxonomy_order" => taxonomy_order, 
			"taxonomy_family" => taxonomy_family, 
			"taxonomy_genus" => taxonomy_genus, 
			"taxonomy_common_name" => taxonomy_common_name, 
			_ => "0xFFFFFF", 
		};
	}

	public ColorStyleAsset()
	{
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		taxonomy_kingdom = "#76FFF8";
		taxonomy_phylum = "#74FFA3";
		taxonomy_subphylum = "#54FF8D";
		taxonomy_class = "#76FF4A";
		taxonomy_order = "#B9FF48";
		taxonomy_family = "#FEFD46";
		taxonomy_genus = "#F8AB4F";
		taxonomy_common_name = "#DC8D4E";
		color_text_grey = "#ADADAD";
		color_text_grey_dark = "#7D7D7D";
		color_text_selector = "#7FFF75AA";
		color_text_selector_remove = "#FF182AAA";
		color_text_pumpkin = "#FFA94C";
		color_text_pumpkin_light = "#FFBC66";
		favorite_selected = Color.white;
		favorite_not_selected = new Color(0.7f, 0.7f, 0.7f, 0.3f);
		health_bar_main_green = Toolbox.makeColor("#00C21F");
		health_bar_main_red = Toolbox.makeColor("#FF4300");
		health_bar_background = Toolbox.makeColor("#303030");
		base._002Ector();
	}
}
