using Steamworks;
using Steamworks.Ugc;
using UnityEngine;
using UnityEngine.UI;

public class SaveWindowIcons : MonoBehaviour
{
	[SerializeField]
	private bool _use_current_world_info;

	[SerializeField]
	private bool _allow_edit;

	[SerializeField]
	private bool _save_meta_data_on_close;

	[SerializeField]
	private bool _save_names_to_current_world;

	[SerializeField]
	private bool _clear_name_on_load;

	[SerializeField]
	private GameObject _icon_orc;

	[SerializeField]
	private GameObject _icon_human;

	[SerializeField]
	private GameObject _icon_elf;

	[SerializeField]
	private GameObject _icon_dwarf;

	[SerializeField]
	private Text _text_map_size;

	[SerializeField]
	private StatsIcon _map_age;

	[SerializeField]
	private StatsIcon _population;

	[SerializeField]
	private StatsIcon _mobs;

	[SerializeField]
	private StatsIcon _vegetation;

	[SerializeField]
	private StatsIcon _deaths;

	[SerializeField]
	private StatsIcon _kingdoms;

	[SerializeField]
	private StatsIcon _cities;

	[SerializeField]
	private StatsIcon _buildings;

	[SerializeField]
	private StatsIcon _equipment;

	[SerializeField]
	private StatsIcon _books;

	[SerializeField]
	private StatsIcon _wars;

	[SerializeField]
	private StatsIcon _alliances;

	[SerializeField]
	private StatsIcon _families;

	[SerializeField]
	private StatsIcon _clans;

	[SerializeField]
	private StatsIcon _cultures;

	[SerializeField]
	private StatsIcon _religions;

	[SerializeField]
	private StatsIcon _languages;

	[SerializeField]
	private StatsIcon _subspecies;

	[SerializeField]
	private StatsIcon _favorites;

	[SerializeField]
	private StatsIcon _favorite_items;

	[SerializeField]
	private GameObject _map_background_normal;

	[SerializeField]
	private GameObject _map_background_cursed;

	[SerializeField]
	private GameObject _map_overlay_cursed;

	[SerializeField]
	private GameObject _modded_icon;

	[SerializeField]
	private GameObject _cursed_icon;

	[SerializeField]
	private Text _map_name;

	[SerializeField]
	private Text _text_description;

	[SerializeField]
	private NameInput _name_input;

	[SerializeField]
	private NameInput _description_input;

	private string _save_path;

	private MapMetaData metaData;

	public void Awake()
	{
		_name_input.addListener(applyInputName);
		_description_input.addListener(applyInputDescription);
	}

	private unsafe void OnEnable()
	{
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		if (!Config.game_loaded)
		{
			return;
		}
		((Component)_name_input).gameObject.SetActive(_allow_edit);
		((Component)_description_input).gameObject.SetActive(_allow_edit);
		if (_use_current_world_info)
		{
			SavedMap savedMap = SaveManager.currentWorldToSavedMap();
			metaData = savedMap.getMeta();
		}
		else if (SaveManager.currentWorkshopMapData != null)
		{
			metaData = SaveManager.currentWorkshopMapData.meta_data_map;
		}
		else
		{
			metaData = SaveManager.getCurrentMeta();
			_save_path = SaveManager.currentSavePath;
		}
		if (metaData != null)
		{
			checkRaceIcons(metaData);
			MapStats mapStats = metaData.mapStats;
			if (_allow_edit)
			{
				if (!_clear_name_on_load)
				{
					_name_input.setText(mapStats.name);
				}
				else
				{
					_name_input.setText("");
				}
				_description_input.setText(mapStats.description);
			}
			MapSizeAsset presetAsset = MapSizeLibrary.getPresetAsset(metaData.width);
			if (presetAsset != null)
			{
				((Component)_text_map_size).GetComponent<LocalizedText>().setKeyAndUpdate(presetAsset.getLocaleID());
			}
			else
			{
				_text_map_size.text = metaData.width + "x" + metaData.height;
			}
			_modded_icon.SetActive(metaData.modded);
			bool cursed = metaData.cursed;
			_cursed_icon.SetActive(cursed);
			_map_background_cursed.SetActive(cursed);
			_map_overlay_cursed.SetActive(cursed);
			_map_background_normal.SetActive(!cursed);
			_map_age.setValue(Date.getYear(mapStats.world_time));
			_population.setValue(metaData.population);
			_mobs.setValue(metaData.mobs);
			_vegetation.setValue(metaData.vegetation);
			_deaths.setValue(metaData.deaths);
			_kingdoms.setValue(metaData.kingdoms);
			_cities.setValue(metaData.cities);
			_buildings.setValue(metaData.buildings);
			_equipment.setValue(metaData.equipment);
			_books.setValue(metaData.books);
			_wars.setValue(metaData.wars);
			_alliances.setValue(metaData.alliances);
			_families.setValue(metaData.families);
			_clans.setValue(metaData.clans);
			_cultures.setValue(metaData.cultures);
			_religions.setValue(metaData.religions);
			_languages.setValue(metaData.languages);
			_subspecies.setValue(metaData.subspecies);
			_favorites.setValue(metaData.favorites);
			_favorite_items.setValue(metaData.favorite_items);
			_map_name.text = mapStats.name;
			((Graphic)_map_name).color = mapStats.getArchitectMood().getColorText();
			((Graphic)_name_input.textField).color = mapStats.getArchitectMood().getColorText();
			_text_description.text = mapStats.description;
			if (SaveManager.currentWorkshopMapData != null)
			{
				Item workshop_item = SaveManager.currentWorkshopMapData.workshop_item;
				Friend owner = ((Item)(ref workshop_item)).Owner;
				if (((object)(*(SteamId*)(&owner.Id))/*cast due to constrained. prefix*/).ToString() == Config.steam_id)
				{
					((Graphic)_map_name).color = Toolbox.makeColor("#3DDEFF");
				}
				else
				{
					((Graphic)_map_name).color = Toolbox.makeColor("#FF9B1C");
				}
			}
		}
		else
		{
			((Component)_map_name).GetComponent<LocalizedText>().updateText();
		}
	}

	private void applyInputName(string pInput)
	{
		if (!string.IsNullOrEmpty(pInput))
		{
			if (_save_names_to_current_world)
			{
				World.world.map_stats.name = pInput;
			}
			else
			{
				metaData.mapStats.name = pInput;
			}
			if (_save_meta_data_on_close && metaData != null)
			{
				SaveManager.saveMetaData(metaData, _save_path);
			}
		}
	}

	private void applyInputDescription(string pInput)
	{
		if (pInput == null)
		{
			return;
		}
		if (_save_names_to_current_world)
		{
			if ((Object)(object)World.world == (Object)null || World.world.map_stats == null)
			{
				return;
			}
			World.world.map_stats.description = pInput;
		}
		else
		{
			if (metaData == null || metaData.mapStats == null)
			{
				return;
			}
			metaData.mapStats.description = pInput;
		}
		if (_save_meta_data_on_close && metaData != null)
		{
			SaveManager.saveMetaData(metaData, _save_path);
		}
	}

	private void OnDisable()
	{
		if (_save_meta_data_on_close && metaData != null)
		{
			SaveManager.saveMetaData(metaData, _save_path);
		}
	}

	private void checkNameInput(bool pDeactivate = false)
	{
		if (_save_meta_data_on_close && metaData != null)
		{
			metaData.mapStats.name = _name_input.textField.text;
			metaData.mapStats.description = _description_input.textField.text;
			SaveManager.saveMetaData(metaData, _save_path);
		}
	}

	private void checkRaceIcons(MapMetaData pData)
	{
		_icon_orc.gameObject.SetActive(false);
		_icon_human.gameObject.SetActive(false);
		_icon_elf.gameObject.SetActive(false);
		_icon_dwarf.gameObject.SetActive(false);
	}
}
