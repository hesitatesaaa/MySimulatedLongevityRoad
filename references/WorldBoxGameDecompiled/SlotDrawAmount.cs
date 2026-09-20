internal struct SlotDrawAmount
{
	public string resource_id;

	public int amount;

	public ResourceAsset asset => AssetManager.resources.get(resource_id);
}
