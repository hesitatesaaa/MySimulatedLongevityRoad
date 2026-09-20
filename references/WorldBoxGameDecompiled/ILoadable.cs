public interface ILoadable<TData>
{
	void setData(TData pData);

	void loadData(TData pData);
}
