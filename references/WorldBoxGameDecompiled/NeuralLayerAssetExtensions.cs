public static class NeuralLayerAssetExtensions
{
	public static NeuralLayerAsset GetAsset(this NeuroLayer pLayerID)
	{
		return AssetManager.neural_layers.getWithID(pLayerID);
	}

	public static string getDebugString(this NeuroLayer pLayerID)
	{
		return pLayerID.GetAsset().debug_string;
	}
}
