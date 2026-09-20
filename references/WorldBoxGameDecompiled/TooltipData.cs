using System;
using System.ComponentModel;

public class TooltipData : IDisposable
{
	[NonSerialized]
	private string _tip_name;

	[NonSerialized]
	private string _tip_description;

	[NonSerialized]
	private string _tip_description_2;

	public War war;

	public Army army;

	public Subspecies subspecies;

	public Family family;

	public Language language;

	public Culture culture;

	public Religion religion;

	public City city;

	public Clan clan;

	public Kingdom kingdom;

	public Alliance alliance;

	public Plot plot;

	public PlotAsset plot_asset;

	public Book book;

	public ResourceAsset resource;

	public Item item;

	public EquipmentAsset item_asset;

	public bool is_editor_augmentation_button;

	public Actor actor;

	public ActorTrait trait;

	public ActorAsset actor_asset;

	public Status status;

	public KingdomAsset kingdom_asset;

	public KingdomTrait kingdom_trait;

	public CultureTrait culture_trait;

	public LanguageTrait language_trait;

	public SubspeciesTrait subspecies_trait;

	public ClanTrait clan_trait;

	public ReligionTrait religion_trait;

	public OnomasticsAsset onomastics_asset;

	public OnomasticsData onomastics_data;

	public NanoObject nano_object;

	public Chromosome chromosome;

	public GeneAsset gene;

	public LocusElement locus;

	public NeuronElement neuron;

	public MapMetaData map_meta;

	public GodPower power;

	public Achievement achievement;

	public WorldLawAsset world_law;

	public ListPool<NameEntry> past_names;

	public ListPool<LeaderEntry> past_rulers;

	public GameLanguageAsset game_language_asset;

	public MetaType meta_type;

	public CustomDataContainer<long> custom_data_long;

	public CustomDataContainer<int> custom_data_int;

	public CustomDataContainer<float> custom_data_float;

	public CustomDataContainer<bool> custom_data_bool;

	public CustomDataContainer<string> custom_data_string;

	[DefaultValue(1f)]
	public float tooltip_scale = 1f;

	public bool sound_allowed = true;

	public bool is_sim_tooltip;

	public string tip_name
	{
		get
		{
			return _tip_name;
		}
		set
		{
			_tip_name = value;
		}
	}

	public string tip_description
	{
		get
		{
			return _tip_description;
		}
		set
		{
			_tip_description = value;
		}
	}

	public string tip_description_2
	{
		get
		{
			return _tip_description_2;
		}
		set
		{
			_tip_description_2 = value;
		}
	}

	public void Dispose()
	{
		custom_data_long?.Dispose();
		custom_data_int?.Dispose();
		custom_data_float?.Dispose();
		custom_data_bool?.Dispose();
		custom_data_string?.Dispose();
		war = null;
		army = null;
		subspecies = null;
		family = null;
		language = null;
		culture = null;
		religion = null;
		city = null;
		clan = null;
		kingdom = null;
		alliance = null;
		plot = null;
		plot_asset = null;
		book = null;
		resource = null;
		item = null;
		item_asset = null;
		actor = null;
		trait = null;
		actor_asset = null;
		status = null;
		kingdom_asset = null;
		kingdom_trait = null;
		culture_trait = null;
		language_trait = null;
		subspecies_trait = null;
		clan_trait = null;
		religion_trait = null;
		onomastics_asset = null;
		onomastics_data = null;
		nano_object = null;
		chromosome = null;
		gene = null;
		locus = null;
		neuron = null;
		map_meta = null;
		power = null;
		achievement = null;
		world_law = null;
		past_names?.Dispose();
		past_names = null;
		past_rulers?.Dispose();
		past_rulers = null;
		game_language_asset = null;
		is_editor_augmentation_button = false;
		meta_type = MetaType.None;
		sound_allowed = true;
	}
}
