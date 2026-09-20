using System.Collections.Generic;
using ai.behaviours;

public class TesterBehSelectRandomMetaObjects : BehaviourActionTester
{
	private bool _pick_selected_objects;

	private string[] _trait_editors;

	public TesterBehSelectRandomMetaObjects(bool pPickSelectedObjects = false)
	{
		_pick_selected_objects = pPickSelectedObjects;
	}

	public override BehResult execute(AutoTesterBot pObject)
	{
		if (_trait_editors == null)
		{
			_trait_editors = new string[3]
			{
				PowerLibrary.traits_delta_rain_edit.id,
				PowerLibrary.traits_gamma_rain_edit.id,
				PowerLibrary.traits_omega_rain_edit.id
			};
		}
		Config.selected_trait_editor = _trait_editors.GetRandom();
		SelectedMetas.selected_alliance = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.alliances.list);
		SelectedMetas.selected_army = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.armies.list);
		SelectedMetas.selected_city = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.cities.list);
		SelectedMetas.selected_clan = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.clans.list);
		SelectedMetas.selected_culture = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.cultures.list);
		SelectedMetas.selected_family = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.families.list);
		SelectedMetas.selected_item = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.items.list);
		SelectedMetas.selected_kingdom = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.kingdoms.list);
		SelectedMetas.selected_language = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.languages.list);
		SelectedMetas.selected_plot = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.plots.list);
		SelectedMetas.selected_religion = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.religions.list);
		SelectedMetas.selected_subspecies = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.subspecies.list);
		SelectedMetas.selected_war = Randy.getRandom(BehaviourActionBase<AutoTesterBot>.world.wars.list);
		int num = 10;
		while (num-- > 0)
		{
			Actor random = BehaviourActionBase<AutoTesterBot>.world.units.GetRandom();
			if (!random.isRekt() && random.asset.can_be_inspected)
			{
				SelectedUnit.clear();
				SelectedUnit.select(random);
				SelectedObjects.setNanoObject(random);
				PowerTabController.showTabSelectedUnit();
				break;
			}
		}
		if (SelectedMetas.selected_item != null)
		{
			SelectedMetas.selected_item.data.favorite = Randy.randomBool();
		}
		if (SelectedUnit.isSet())
		{
			SelectedUnit.unit.data.favorite = Randy.randomBool();
		}
		Config.selected_objects_graph.Clear();
		if (_pick_selected_objects)
		{
			List<NanoObject> list = new List<NanoObject>();
			if (SelectedMetas.selected_alliance != null)
			{
				list.Add(SelectedMetas.selected_alliance);
			}
			if (SelectedMetas.selected_city != null)
			{
				list.Add(SelectedMetas.selected_city);
			}
			if (SelectedMetas.selected_clan != null)
			{
				list.Add(SelectedMetas.selected_clan);
			}
			if (SelectedMetas.selected_culture != null)
			{
				list.Add(SelectedMetas.selected_culture);
			}
			if (SelectedMetas.selected_family != null)
			{
				list.Add(SelectedMetas.selected_family);
			}
			if (SelectedMetas.selected_kingdom != null)
			{
				list.Add(SelectedMetas.selected_kingdom);
			}
			if (SelectedMetas.selected_language != null)
			{
				list.Add(SelectedMetas.selected_language);
			}
			if (SelectedMetas.selected_religion != null)
			{
				list.Add(SelectedMetas.selected_religion);
			}
			if (SelectedMetas.selected_subspecies != null)
			{
				list.Add(SelectedMetas.selected_subspecies);
			}
			list.Shuffle();
			for (int i = 0; i < 3 && i < list.Count; i++)
			{
				if (list[i] != null)
				{
					Config.selected_objects_graph.Add(list[i]);
				}
			}
		}
		return BehResult.Continue;
	}
}
