using System;

public abstract class ItemAssetLibrary<T> : BaseLibraryWithUnlockables<T> where T : ItemAsset
{
	public override T add(T pAsset)
	{
		T val = base.add(pAsset);
		if (val.base_stats == null)
		{
			val.base_stats = new BaseStats();
		}
		return val;
	}

	public override void editorDiagnosticLocales()
	{
		foreach (T item in list)
		{
			if (!item.has_locales)
			{
				continue;
			}
			string localeID = item.getLocaleID();
			checkLocale(item, localeID);
			if (item.isMod())
			{
				continue;
			}
			string descriptionID = item.getDescriptionID();
			checkLocale(item, descriptionID);
			if (item.material != "basic")
			{
				checkLocale(item, item.getMaterialID());
			}
			foreach (Rarity value in Enum.GetValues(typeof(Rarity)))
			{
				string localeRarity = item.getLocaleRarity(value);
				checkLocale(item, localeRarity);
			}
		}
	}
}
