public class MetaObjectCounter<TObject, TData> where TObject : MetaObject<TData> where TData : MetaObjectData
{
	public TObject meta_object;

	public int amount;

	public MetaObjectCounter(TObject pMetaObject)
	{
		meta_object = pMetaObject;
	}
}
