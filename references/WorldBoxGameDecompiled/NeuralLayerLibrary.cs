using System.Collections.Generic;

public class NeuralLayerLibrary : AssetLibrary<NeuralLayerAsset>
{
	private Dictionary<NeuroLayer, NeuralLayerAsset> _dict_enum_assets = new Dictionary<NeuroLayer, NeuralLayerAsset>();

	public NeuralLayerAsset[] layers_array;

	public override void init()
	{
		base.init();
		add(new NeuralLayerAsset
		{
			id = "neuro_layer_0",
			layer = NeuroLayer.Layer_0_Minimal,
			color_hex = "#B0E0E6"
		});
		add(new NeuralLayerAsset
		{
			id = "neuro_layer_1",
			layer = NeuroLayer.Layer_1_Low,
			color_hex = "#87CEEB",
			chance_to_go_to_next_layer = 0.3f
		});
		add(new NeuralLayerAsset
		{
			id = "neuro_layer_2",
			layer = NeuroLayer.Layer_2_Moderate,
			color_hex = "#FFD700",
			chance_to_go_to_next_layer = 0.2f
		});
		add(new NeuralLayerAsset
		{
			id = "neuro_layer_3",
			layer = NeuroLayer.Layer_3_High,
			color_hex = "#FF4500",
			chance_to_go_to_next_layer = 0.1f
		});
		add(new NeuralLayerAsset
		{
			id = "neuro_layer_4",
			layer = NeuroLayer.Layer_4_Critical,
			critical = true,
			color_hex = "#FF0000"
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		layers_array = new NeuralLayerAsset[list.Count];
		foreach (NeuralLayerAsset item in list)
		{
			_dict_enum_assets.Add(item.layer, item);
			layers_array[(int)item.layer] = item;
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (NeuralLayerAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}

	public NeuralLayerAsset getWithID(NeuroLayer pLayerID)
	{
		return _dict_enum_assets[pLayerID];
	}
}
