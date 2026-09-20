public interface IAugmentationsWindow<TEditor> where TEditor : IAugmentationsEditor
{
	void updateStats();

	T GetComponentInChildren<T>(bool includeInactive = false);

	void reloadBanner();

	TEditor getEditor()
	{
		return GetComponentInChildren<TEditor>(includeInactive: true);
	}
}
