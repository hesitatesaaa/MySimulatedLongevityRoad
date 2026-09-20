using UnityEngine;

public class SubspeciesWindow : WindowMetaGeneric<Subspecies, SubspeciesData>, ITraitWindow<SubspeciesTrait, SubspeciesTraitButton>, IAugmentationsWindow<ITraitsEditor<SubspeciesTrait>>
{
	public StatBar experienceBar;

	public override MetaType meta_type => MetaType.Subspecies;

	protected override Subspecies meta_object => SelectedMetas.selected_subspecies;

	public override void startShowingWindow()
	{
		base.startShowingWindow();
		ActorAsset actorAsset = meta_object.getActorAsset();
		if (!actorAsset.isAvailable())
		{
			actorAsset.unlock();
		}
		AchievementLibrary.checkSubspeciesAchievements(meta_object);
	}

	protected override bool onNameChange(string pInput)
	{
		if (!base.onNameChange(pInput))
		{
			return false;
		}
		foreach (Religion religion in World.world.religions)
		{
			if (!religion.isRekt() && religion.data.creator_subspecies_id == meta_object.getID())
			{
				religion.data.creator_subspecies_name = meta_object.data.name;
			}
		}
		foreach (Culture culture in World.world.cultures)
		{
			if (!culture.isRekt() && culture.data.creator_subspecies_id == meta_object.getID())
			{
				culture.data.creator_subspecies_name = meta_object.data.name;
			}
		}
		foreach (Clan clan in World.world.clans)
		{
			if (!clan.isRekt() && clan.data.creator_subspecies_id == meta_object.getID())
			{
				clan.data.creator_subspecies_name = meta_object.data.name;
			}
		}
		foreach (Language language in World.world.languages)
		{
			if (!language.isRekt() && language.data.creator_subspecies_id == meta_object.getID())
			{
				language.data.creator_subspecies_name = meta_object.data.name;
			}
		}
		foreach (Family family in World.world.families)
		{
			if (!family.isRekt() && family.data.subspecies_id == meta_object.getID())
			{
				family.data.subspecies_name = meta_object.data.name;
			}
		}
		return true;
	}

	internal override void showStatsRows()
	{
		tryShowPastNames();
		showStatRow("created", meta_object.getFoundedDate(), MetaType.None, -1L, "iconAge");
		showStatRow("generation", meta_object.getGeneration(), MetaType.None, -1L, "worldrules/icon_grow_trees_fast");
		showStatRow("world_population_percentage", meta_object.countPopulationPercentage() + "%", MetaType.None, -1L, "iconPopulation");
		if (meta_object.hasParentSubspecies())
		{
			Subspecies subspecies = World.world.subspecies.get(meta_object.data.parent_subspecies);
			if (subspecies == null)
			{
				showStatRow("subspecies_ancestor", LocalizedTextManager.getText("subspecies_extinct"), ColorStyleLibrary.m.color_dead_text, MetaType.None, -1L, pColorText: true);
			}
			else
			{
				tryToShowMetaSubspecies("subspecies_ancestor", -1L, null, subspecies);
			}
		}
		Subspecies subspecies2 = World.world.subspecies.get(meta_object.data.evolved_into_subspecies);
		if (subspecies2 != null)
		{
			tryToShowMetaSubspecies("evolution", -1L, null, subspecies2);
		}
	}

	public void debugClearExpLevel()
	{
		meta_object.debugClear();
		OnEnable();
	}

	T IAugmentationsWindow<ITraitsEditor<SubspeciesTrait>>.GetComponentInChildren<T>(bool includeInactive)
	{
		return ((Component)this).GetComponentInChildren<T>(includeInactive);
	}
}
