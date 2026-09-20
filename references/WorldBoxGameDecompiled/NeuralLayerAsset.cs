using System;
using Newtonsoft.Json;

[Serializable]
public class NeuralLayerAsset : Asset, IDescriptionAsset, ILocalizedAsset
{
	public NeuroLayer layer;

	public string color_hex;

	public bool critical;

	public float chance_to_go_to_next_layer;

	[JsonIgnore]
	public string debug_string => $"{getLocaleID().Localize()} - {getDescriptionID().Localize()} - ({layer})";

	public string getLocaleID()
	{
		return id;
	}

	public string getDescriptionID()
	{
		return id + "_priority";
	}
}
