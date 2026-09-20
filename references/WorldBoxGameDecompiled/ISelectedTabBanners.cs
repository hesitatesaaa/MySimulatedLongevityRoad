public interface ISelectedTabBanners<T> where T : NanoObject
{
	void update(T pNano);

	int countVisibleBanners();
}
