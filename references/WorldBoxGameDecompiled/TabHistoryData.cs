public struct TabHistoryData(NanoObject pObject)
{
	public readonly MetaType meta_type = pObject.getMetaType();

	public readonly long id = pObject.id;

	public NanoObject getNanoObject()
	{
		return AssetManager.meta_type_library.getAsset(meta_type).get(id);
	}
}
