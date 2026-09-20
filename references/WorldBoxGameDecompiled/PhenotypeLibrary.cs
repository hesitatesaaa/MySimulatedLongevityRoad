using System.Collections.Generic;
using UnityEngine;

public class PhenotypeLibrary : AssetLibrary<PhenotypeAsset>
{
	private Dictionary<int, PhenotypeAsset> _phenotypes_assets_by_index = new Dictionary<int, PhenotypeAsset>();

	public const int PHENOTYPE_SHADES = 4;

	public const int PHENOTYPE_NONE = 0;

	public static PhenotypeAsset default_asset;

	public static PhenotypeAsset default_green;

	public override void init()
	{
		base.init();
		default_asset = add(new PhenotypeAsset
		{
			id = "skin_pale",
			shades_from = "#FFF9F4",
			shades_to = "#ECD9D5"
		});
		add(new PhenotypeAsset
		{
			id = "skin_light",
			shades_from = "#F5D6BA",
			shades_to = "#F3C6BE"
		});
		add(new PhenotypeAsset
		{
			id = "skin_medium",
			shades_from = "#C5966D",
			shades_to = "#C06859"
		});
		add(new PhenotypeAsset
		{
			id = "skin_dark",
			shades_from = "#7C5A3C",
			shades_to = "#593731"
		});
		add(new PhenotypeAsset
		{
			id = "skin_black",
			shades_from = "#46372A",
			shades_to = "#34211E"
		});
		add(new PhenotypeAsset
		{
			id = "skin_mixed",
			shades_from = "#E9BA90",
			shades_to = "#34211E"
		});
		add(new PhenotypeAsset
		{
			id = "skin_red",
			shades_from = "#DB4D48",
			shades_to = "#7F2E39"
		});
		add(new PhenotypeAsset
		{
			id = "skin_yellow",
			shades_from = "#FFE18E",
			shades_to = "#AB8B51"
		});
		add(new PhenotypeAsset
		{
			id = "skin_green",
			shades_from = "#9DB361",
			shades_to = "#425338"
		});
		add(new PhenotypeAsset
		{
			id = "skin_blue",
			shades_from = "#7FC1C7",
			shades_to = "#37617A"
		});
		add(new PhenotypeAsset
		{
			id = "skin_purple",
			shades_from = "#C29FD5",
			shades_to = "#5C54A2"
		});
		add(new PhenotypeAsset
		{
			id = "skin_pink",
			shades_from = "#FFAFC0",
			shades_to = "#D1539F"
		});
		add(new PhenotypeAsset
		{
			id = "white_gray",
			shades_from = "#FAFAFA",
			shades_to = "#B0B0B0"
		});
		add(new PhenotypeAsset
		{
			id = "mid_gray",
			shades_from = "#BBBBBB",
			shades_to = "#606060"
		});
		add(new PhenotypeAsset
		{
			id = "gray_black",
			shades_from = "#7A7A7A",
			shades_to = "#3E3E3E"
		});
		add(new PhenotypeAsset
		{
			id = "bright_red",
			shades_from = "#FF9A9A",
			shades_to = "#FF2222"
		});
		add(new PhenotypeAsset
		{
			id = "bright_orange",
			shades_from = "#FFC191",
			shades_to = "#FF7316"
		});
		add(new PhenotypeAsset
		{
			id = "bright_yellow",
			shades_from = "#FFEBA3",
			shades_to = "#FFCE1F"
		});
		add(new PhenotypeAsset
		{
			id = "bright_green",
			shades_from = "#C5FFA0",
			shades_to = "#1CDA2C"
		});
		add(new PhenotypeAsset
		{
			id = "bright_teal",
			shades_from = "#A3FFE0",
			shades_to = "#0DDCA4"
		});
		add(new PhenotypeAsset
		{
			id = "bright_blue",
			shades_from = "#A8E3FF",
			shades_to = "#1CA4FF"
		});
		add(new PhenotypeAsset
		{
			id = "bright_violet",
			shades_from = "#D6B7FF",
			shades_to = "#7142F3"
		});
		add(new PhenotypeAsset
		{
			id = "bright_purple",
			shades_from = "#F3B9FF",
			shades_to = "#BE4FDD"
		});
		add(new PhenotypeAsset
		{
			id = "bright_pink",
			shades_from = "#FFB6E3",
			shades_to = "#FF5FC2"
		});
		add(new PhenotypeAsset
		{
			id = "bright_salmon",
			shades_from = "#FFAFBC",
			shades_to = "#FF537E"
		});
		add(new PhenotypeAsset
		{
			id = "dark_red",
			shades_from = "#EC2D2D",
			shades_to = "#5F1414"
		});
		add(new PhenotypeAsset
		{
			id = "dark_orange",
			shades_from = "#FF7B16",
			shades_to = "#69340B"
		});
		add(new PhenotypeAsset
		{
			id = "dark_yellow",
			shades_from = "#FFC61A",
			shades_to = "#986411"
		});
		default_green = add(new PhenotypeAsset
		{
			id = "dark_green",
			shades_from = "#72C727",
			shades_to = "#2A6016"
		});
		add(new PhenotypeAsset
		{
			id = "dark_teal",
			shades_from = "#1FDEA5",
			shades_to = "#0F6557"
		});
		add(new PhenotypeAsset
		{
			id = "dark_blue",
			shades_from = "#2182EB",
			shades_to = "#163487"
		});
		add(new PhenotypeAsset
		{
			id = "dark_violet",
			shades_from = "#6F47DF",
			shades_to = "#321A75"
		});
		add(new PhenotypeAsset
		{
			id = "dark_purple",
			shades_from = "#A341E1",
			shades_to = "#491C5F"
		});
		add(new PhenotypeAsset
		{
			id = "dark_pink",
			shades_from = "#F84EE7",
			shades_to = "#652265"
		});
		add(new PhenotypeAsset
		{
			id = "dark_salmon",
			shades_from = "#F35688",
			shades_to = "#6F2949"
		});
		add(new PhenotypeAsset
		{
			id = "toxic_green",
			shades_from = "#CBFF2E",
			shades_to = "#10F023"
		});
		add(new PhenotypeAsset
		{
			id = "aqua",
			shades_from = "#77D8D0",
			shades_to = "#3779A5"
		});
		add(new PhenotypeAsset
		{
			id = "swamp",
			shades_from = "#784D85",
			shades_to = "#367F58"
		});
		add(new PhenotypeAsset
		{
			id = "jungle",
			shades_from = "#03803D",
			shades_to = "#2EB829"
		});
		add(new PhenotypeAsset
		{
			id = "polar",
			shades_from = "#E8EEF5",
			shades_to = "#95C2E7"
		});
		add(new PhenotypeAsset
		{
			id = "savanna",
			shades_from = "#FFB52B",
			shades_to = "#D06643"
		});
		add(new PhenotypeAsset
		{
			id = "corrupted",
			shades_from = "#7E7987",
			shades_to = "#483162"
		});
		add(new PhenotypeAsset
		{
			id = "infernal",
			shades_from = "#FF5100",
			shades_to = "#FF002B"
		});
		add(new PhenotypeAsset
		{
			id = "lemon",
			shades_from = "#FFDB27",
			shades_to = "#BDF741"
		});
		add(new PhenotypeAsset
		{
			id = "desert",
			shades_from = "#D3C0AB",
			shades_to = "#B68637"
		});
		add(new PhenotypeAsset
		{
			id = "crystal",
			shades_from = "#1CFFCE",
			shades_to = "#15A1FF"
		});
		add(new PhenotypeAsset
		{
			id = "candy",
			shades_from = "#FF658C",
			shades_to = "#D166FF"
		});
		add(new PhenotypeAsset
		{
			id = "soil",
			shades_from = "#533028",
			shades_to = "#160C15"
		});
		add(new PhenotypeAsset
		{
			id = "wood",
			shades_from = "#9D582A",
			shades_to = "#4C2828"
		});
		add(new PhenotypeAsset
		{
			id = "pink_yellow_mushroom",
			shades_from = "#F12C78",
			shades_to = "#FFAC26"
		});
		add(new PhenotypeAsset
		{
			id = "black_blue",
			shades_from = "#34345B",
			shades_to = "#222229"
		});
		add(new PhenotypeAsset
		{
			id = "magical",
			shades_from = "#24C695",
			shades_to = "#9D2EA7"
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		int num = 1;
		foreach (PhenotypeAsset item in list)
		{
			createShades(item);
			item.phenotype_index = num++;
			_phenotypes_assets_by_index.Add(item.phenotype_index, item);
		}
	}

	public PhenotypeAsset getAssetByPhenotypeIndex(int pIndex)
	{
		_phenotypes_assets_by_index.TryGetValue(pIndex, out var value);
		if (value == null)
		{
			return default_asset;
		}
		return value;
	}

	public void createShades(PhenotypeAsset pAsset)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		Color pFrom = Toolbox.makeColor(pAsset.shades_from);
		Color pTo = Toolbox.makeColor(pAsset.shades_to);
		float num = 1f / 3f;
		for (int i = 0; i < 4; i++)
		{
			float num2 = 1f - (float)i * num;
			if (num2 > 1f)
			{
				num2 = 1f;
			}
			Color val = Toolbox.blendColor(pFrom, pTo, num2);
			pAsset.colors[i] = Color32.op_Implicit(val);
		}
	}
}
