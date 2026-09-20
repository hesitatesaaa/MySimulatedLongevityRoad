public interface IBanner : IBaseMono, IRefreshElement
{
	MetaCustomizationAsset meta_asset { get; }

	MetaTypeAsset meta_type_asset { get; }

	NanoObject GetNanoObject();

	void load(NanoObject pObject);

	string getName();

	void showTooltip();

	void jump(float pSpeed = 0.1f, bool pSilent = false)
	{
	}

	void IRefreshElement.refresh()
	{
		NanoObject nanoObject = GetNanoObject();
		if (nanoObject != null && nanoObject.isAlive())
		{
			load(nanoObject);
		}
	}
}
