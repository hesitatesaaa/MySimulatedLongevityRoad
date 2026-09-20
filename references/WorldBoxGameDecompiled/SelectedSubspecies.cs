using UnityEngine;

public class SelectedSubspecies : SelectedMeta<Subspecies, SubspeciesData>
{
	[SerializeField]
	private CitiesKingdomsContainersController _banners_cities_kingdoms;

	private SubspeciesSelectedContainerBirthTraits _container_traits_birth;

	protected override MetaType meta_type => MetaType.Subspecies;

	protected override string getPowerTabAssetID()
	{
		return "selected_subspecies";
	}

	protected override void Awake()
	{
		base.Awake();
		_container_traits_birth = ((Component)this).GetComponentInChildren<SubspeciesSelectedContainerBirthTraits>();
	}

	protected override void updateTraits()
	{
		base.updateTraits();
		if (!((Object)(object)_container_traits_birth == (Object)null))
		{
			_container_traits_birth.update(nano_object);
		}
	}

	protected override void showStatsGeneral(Subspecies pSubspecies)
	{
		base.showStatsGeneral(pSubspecies);
		setIconValue("i_kingdoms", pSubspecies.countMainKingdoms());
		setIconValue("i_villages", pSubspecies.countMainCities());
	}

	public void openBirthTraitsTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("BirthTraitsEditor");
	}

	public void openGeneticsTab()
	{
		ScrollWindow.showWindow(base.window_id);
		ScrollWindow.getCurrentWindow().tabs.showTab("Genetics");
	}

	protected override void updateElementsOnChange(Subspecies pNano)
	{
		base.updateElementsOnChange(pNano);
		_banners_cities_kingdoms.update(pNano);
	}

	protected override void checkAchievements(Subspecies pNano)
	{
		AchievementLibrary.checkSubspeciesAchievements(pNano);
	}
}
