using UnityEngine;
using UnityEngine.UI;

public class NameplateText : MonoBehaviour
{
	private NameplateManager _manager;

	[SerializeField]
	private Image _icon_species;

	[SerializeField]
	private Image _icon_special;

	[SerializeField]
	private Image _icon_favorite;

	[SerializeField]
	private Image _background_image;

	[SerializeField]
	private KingdomBanner _banner_kingdoms;

	[SerializeField]
	private CultureBanner _banner_culture;

	[SerializeField]
	private LanguageBanner _banner_language;

	[SerializeField]
	private AllianceBanner _banner_alliance;

	[SerializeField]
	private ReligionBanner _banner_religion;

	[SerializeField]
	private SubspeciesBanner _banner_subspecies;

	[SerializeField]
	private FamilyBanner _banner_family;

	[SerializeField]
	private ClanBanner _banner_clan;

	[SerializeField]
	private ArmyBanner _banner_army;

	[SerializeField]
	private CityBanner _banner_city;

	[SerializeField]
	private RectTransform _container_capture;

	public HorizontalLayoutGroup layout_group;

	private NameplateAsset _asset;

	private bool _show_icon_species;

	private bool _show_icon_special;

	private bool _show_icon_favorite;

	private bool _show_banner_army;

	private bool _show_banner_kingdom;

	private bool _show_banner_culture;

	private bool _show_banner_alliance;

	private bool _show_banner_clan;

	private bool _show_banner_religion;

	private bool _show_banner_family;

	private bool _show_banner_subspecies;

	private bool _show_banner_language;

	private bool _show_banner_city;

	private bool _show_capture_counter;

	private CanvasGroup _canvas_group;

	[SerializeField]
	private RectTransform _rect_background;

	[SerializeField]
	private Text _text_name;

	[SerializeField]
	private Text _conquer_text;

	private RectTransform _rect;

	private RectTransform _text_rect;

	private bool _showing;

	internal bool priority_capital;

	internal int priority_population;

	internal bool favorited;

	internal Rect map_text_rect_click;

	internal Rect map_text_rect_overlap;

	public NanoObject nano_object;

	private float _last_scale;

	private string _old_text;

	private float _text_width;

	private bool _active_check_dirty;

	private NameplateRenderingType _last_mode;

	private Vector2 _last_position;

	private bool is_full => _last_mode == NameplateRenderingType.Full;

	public bool is_mini => _last_mode == NameplateRenderingType.BannerOnly;

	public Vector2 getLastScreenPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return _last_position;
	}

	private void Awake()
	{
		_rect = ((Component)this).GetComponent<RectTransform>();
		_text_rect = ((Component)_text_name).GetComponent<RectTransform>();
		_canvas_group = ((Component)this).GetComponent<CanvasGroup>();
	}

	public void prepare(NameplateAsset pAsset, NanoObject pMeta, float pGlobalScale, NameplateRenderingType pNameplateMode, bool pNanoObjectSet, NanoObject pSelectedNanoObject)
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		if (pNanoObjectSet)
		{
			pNameplateMode = ((pSelectedNanoObject == pMeta) ? NameplateRenderingType.Full : NameplateRenderingType.BannerOnly);
		}
		if (pNameplateMode != _last_mode)
		{
			clearCaches();
			_active_check_dirty = true;
			_last_mode = pNameplateMode;
			switch (_last_mode)
			{
			case NameplateRenderingType.Full:
				((Component)_background_image).transform.localScale = new Vector3(1f, 1f, 1f);
				((Behaviour)_background_image).enabled = true;
				break;
			case NameplateRenderingType.BannerOnly:
				((Component)_background_image).transform.localScale = new Vector3(pAsset.banner_only_mode_scale, pAsset.banner_only_mode_scale, 1f);
				((Behaviour)_background_image).enabled = false;
				break;
			}
		}
		updateScale(pMeta, pGlobalScale, pNanoObjectSet, pSelectedNanoObject);
		resetElements();
		setShowing(pVal: true);
		setAssetAndMeta(pAsset, pMeta);
		if (((IFavoriteable)pMeta).isFavorite())
		{
			showFavoriteIcon();
		}
		else
		{
			_show_icon_favorite = false;
		}
		checkSetActive((Component)(object)_icon_favorite, _show_icon_favorite);
	}

	private void updateScale(NanoObject pMeta, float pGlobalScale, bool pNanoObjectSet, NanoObject pSelectedNanoObject)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		float num = pGlobalScale;
		if (pNanoObjectSet)
		{
			num = ((pSelectedNanoObject != pMeta) ? (pGlobalScale * 0.8f) : (pGlobalScale * 1.2f));
		}
		if (_last_scale != num)
		{
			_last_scale = num;
			((Component)this).transform.localScale = new Vector3(num, num, 1f);
		}
	}

	public void forceScale(Vector3 pScale)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_last_scale = pScale.x;
		((Component)this).transform.localScale = pScale;
	}

	public void newNameplate(NameplateManager pManager, string pName)
	{
		clearFull();
		_manager = pManager;
		((Object)this).name = pName;
	}

	public bool isShowing()
	{
		return _showing;
	}

	public void setShowing(bool pVal)
	{
		_showing = pVal;
	}

	public void checkActive()
	{
		if (_showing)
		{
			if (!((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(true);
			}
		}
		else if (((Component)this).gameObject.activeSelf)
		{
			((Component)this).gameObject.SetActive(false);
		}
		if (_showing && _active_check_dirty)
		{
			_active_check_dirty = false;
			checkActiveElements();
		}
	}

	private void checkActiveElements()
	{
		checkSetActive((Component)(object)_container_capture, _show_capture_counter);
		checkSetActive((Component)(object)_icon_species, _show_icon_species);
		checkSetActive((Component)(object)_icon_special, _show_icon_special);
		checkSetActive((Component)(object)_banner_alliance, _show_banner_alliance);
		checkSetActive((Component)(object)_banner_clan, _show_banner_clan);
		checkSetActive((Component)(object)_banner_culture, _show_banner_culture);
		checkSetActive((Component)(object)_banner_kingdoms, _show_banner_kingdom);
		checkSetActive((Component)(object)_banner_religion, _show_banner_religion);
		checkSetActive((Component)(object)_banner_family, _show_banner_family);
		checkSetActive((Component)(object)_banner_language, _show_banner_language);
		checkSetActive((Component)(object)_banner_subspecies, _show_banner_subspecies);
		checkSetActive((Component)(object)_banner_army, _show_banner_army);
		checkSetActive((Component)(object)_banner_city, _show_banner_city);
	}

	private void checkSetActive(Component pComponent, bool pShouldBeActive)
	{
		checkSetActive(pComponent.gameObject, pShouldBeActive);
	}

	private void checkSetActive(GameObject pObject, bool pShouldBeActive)
	{
		if (pShouldBeActive)
		{
			if (!pObject.activeSelf)
			{
				pObject.SetActive(true);
			}
		}
		else if (pObject.activeSelf)
		{
			pObject.SetActive(false);
		}
	}

	public void clearFull()
	{
		nano_object = null;
		clearCaches();
		setShowing(pVal: false);
		resetElements();
	}

	public void clearCaches()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		_asset = null;
		_last_position = Globals.POINT_IN_VOID_2;
		_last_scale = -1f;
		_old_text = "!";
		_last_mode = NameplateRenderingType.Clear;
	}

	private void resetElements()
	{
		priority_capital = false;
		priority_population = 0;
		favorited = false;
		_show_capture_counter = false;
		_show_icon_favorite = false;
		_show_icon_species = false;
		_show_icon_special = false;
		_show_banner_alliance = false;
		_show_banner_clan = false;
		_show_banner_culture = false;
		_show_banner_army = false;
		_show_banner_kingdom = false;
		_show_banner_religion = false;
		_show_banner_subspecies = false;
		_show_banner_language = false;
		_show_banner_family = false;
		_show_banner_city = false;
	}

	private void setupMeta(MetaObjectData pData, ColorAsset pColorAsset)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		favorited = pData.favorite;
		Color colorText = pColorAsset.getColorText();
		((Graphic)_text_name).color = colorText;
		updateAlpha(pData);
	}

	private void updateAlpha(MetaObjectData pData)
	{
		float num = ((!checkShouldDrawObject(pData)) ? 0.5f : 1f);
		if (_canvas_group.alpha != num)
		{
			_canvas_group.alpha = num;
		}
	}

	internal void showTextKingdom(Kingdom pMetaObject, Vector2 pPosition)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int populationPeople = pMetaObject.getPopulationPeople();
		string text = getStringForNameplate(pMetaObject.name, populationPeople);
		if (is_full)
		{
			if (DebugConfig.isOn(DebugOption.ShowWarriorsCityText))
			{
				text = text + " | " + pMetaObject.countTotalWarriors() + "/" + pMetaObject.countWarriorsMax();
			}
			if (DebugConfig.isOn(DebugOption.ShowCityWeaponsText))
			{
				text = text + " | w" + pMetaObject.countWeapons();
			}
		}
		setText(text, Vector2.op_Implicit(pPosition));
		setPriority(populationPeople);
		showSpecies(pMetaObject.getSpriteIcon());
		_show_banner_kingdom = true;
		_banner_kingdoms.load(pMetaObject);
		if (is_full)
		{
			Clan kingClan = pMetaObject.getKingClan();
			if (kingClan != null)
			{
				_show_banner_clan = true;
				_banner_clan.load(kingClan);
			}
		}
	}

	internal void showTextReligion(Religion pMetaObject, Vector3 pPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplate = getStringForNameplate(pMetaObject.name, count);
		setText(stringForNameplate, pPosition);
		setPriority(count);
		_show_banner_religion = true;
		_banner_religion.load(pMetaObject);
		showSpecies(pMetaObject.getActorAsset().getSpriteIcon());
	}

	internal void showTextSubspecies(Subspecies pMetaObject, Vector3 pPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplate = getStringForNameplate(pMetaObject.name, count);
		setText(stringForNameplate, pPosition);
		setPriority(count);
		_show_banner_subspecies = true;
		_banner_subspecies.load(pMetaObject);
		showSpecies(pMetaObject.getSpriteIcon());
	}

	internal void showTextFamily(Family pMetaObject, Vector3 pPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplate = getStringForNameplate(pMetaObject.name, count);
		setText(stringForNameplate, pPosition);
		setPriority(count);
		_show_banner_family = true;
		_banner_family.load(pMetaObject);
	}

	internal void showTextArmy(Army pMetaObject, Vector3 pPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplateLine = getStringForNameplateLine(pMetaObject.name, count);
		setText(stringForNameplateLine, pPosition);
		setPriority(count);
		_show_banner_army = true;
		_banner_army.load(pMetaObject);
		if (pMetaObject.hasCaptain())
		{
			showSpecies(pMetaObject.getCaptain().getActorAsset().getSpriteIcon());
		}
		else
		{
			showSpecies(pMetaObject.getActorAsset().getSpriteIcon());
		}
	}

	internal void showTextCulture(Culture pMetaObject, Vector3 pPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplate = getStringForNameplate(pMetaObject.name, count);
		setText(stringForNameplate, pPosition);
		setPriority(count);
		_show_banner_culture = true;
		_banner_culture.load(pMetaObject);
		showSpecies(pMetaObject.getActorAsset().getSpriteIcon());
	}

	internal void showTextLanguage(Language pMetaObject, Vector3 pPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplate = getStringForNameplate(pMetaObject.name, count);
		setText(stringForNameplate, pPosition);
		setPriority(count);
		_show_banner_language = true;
		_banner_language.load(pMetaObject);
		showSpecies(pMetaObject.getActorAsset().getSpriteIcon());
	}

	internal void showTextAlliance(Alliance pMetaObject, City pCity)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int num = pMetaObject.countPopulation();
		_show_icon_species = false;
		string text = getStringForNameplate(pMetaObject.name, num);
		if (is_full && DebugConfig.isOn(DebugOption.ShowWarriorsCityText))
		{
			text = text + " | " + pMetaObject.countWarriors();
		}
		setText(text, Vector2.op_Implicit(pCity.city_center));
		setPriority(num);
		_show_banner_alliance = true;
		_banner_alliance.load(pMetaObject);
	}

	internal void showTextClanFluid(Clan pMetaObject, Vector3 pPosition)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplate = getStringForNameplate(pMetaObject.name, count);
		setText(stringForNameplate, pPosition);
		setPriority(count);
		_show_banner_clan = true;
		_banner_clan.load(pMetaObject);
		showSpecies(pMetaObject.getActorAsset().getSpriteIcon());
	}

	internal void showTextClanCity(Clan pMetaObject, City pCity)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.getColor());
		int count = pMetaObject.units.Count;
		string stringForNameplate = getStringForNameplate(pMetaObject.name, count);
		setText(stringForNameplate, Vector2.op_Implicit(pCity.city_center));
		setPriority(count);
		ActorAsset actorAsset = pMetaObject.getActorAsset();
		showSpecies(actorAsset.getSpriteIcon());
		_show_banner_clan = true;
		_banner_clan.load(pMetaObject);
	}

	internal void showTextCity(City pMetaObject, Vector2 pPosition)
	{
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		setupMeta(pMetaObject.data, pMetaObject.kingdom.getColor());
		if (pMetaObject.isCapitalCity())
		{
			setNameplateSprite("ui/nameplates/nameplate_city_capital");
		}
		else
		{
			setNameplateSprite("ui/nameplates/nameplate_city");
		}
		int populationPeople = pMetaObject.getPopulationPeople();
		string text = getStringForNameplate(pMetaObject.name, populationPeople);
		if (is_full)
		{
			if (DebugConfig.isOn(DebugOption.ShowWarriorsCityText))
			{
				text = text + " | " + pMetaObject.countWarriors() + "/" + pMetaObject.getMaxWarriors();
				if (Config.isEditor)
				{
					string text2 = "  :  " + (int)(pMetaObject.getArmyMaxMultiplier() * 100f) + "%";
					text += text2;
				}
			}
			if (DebugConfig.isOn(DebugOption.ShowCityWeaponsText))
			{
				text = text + " | w" + pMetaObject.countWeapons();
			}
			if (DebugConfig.isOn(DebugOption.ShowFoodCityText))
			{
				text = text + " | F" + pMetaObject.getTotalFood();
			}
		}
		setText(text, Vector2.op_Implicit(pPosition));
		if (pMetaObject.getMainSubspecies() != null)
		{
			showSpecies(pMetaObject.getMainSubspecies().getActorAsset().getSpriteIcon());
		}
		if (pMetaObject.last_visual_capture_ticks != 0)
		{
			_show_capture_counter = true;
			_active_check_dirty = true;
			if (pMetaObject.being_captured_by != null && pMetaObject.being_captured_by.isAlive())
			{
				((Graphic)_conquer_text).color = pMetaObject.being_captured_by.getColor().getColorText();
			}
			_conquer_text.text = pMetaObject.last_visual_capture_ticks + "%";
		}
		else
		{
			_show_capture_counter = false;
			_active_check_dirty = true;
		}
		if (_show_capture_counter)
		{
			Vector2 anchoredPosition = default(Vector2);
			if (is_full)
			{
				((Vector2)(ref anchoredPosition))._002Ector(0f, -1f);
			}
			else
			{
				((Vector2)(ref anchoredPosition))._002Ector(3f, -25f);
			}
			_container_capture.anchoredPosition = anchoredPosition;
		}
		_show_banner_city = true;
		_banner_city.load(pMetaObject);
		priority_capital = pMetaObject.isCapitalCity();
		setPriority(populationPeople);
	}

	private void showSpecies(string pPath)
	{
		showSpecies(SpriteTextureLoader.getSprite(pPath));
	}

	private void showSpecies(Sprite pSprite)
	{
		if (!is_mini)
		{
			_show_icon_species = true;
			_icon_species.sprite = pSprite;
		}
	}

	public void showFavoriteIcon()
	{
		_show_icon_favorite = true;
	}

	private void showSpecial(string pPath)
	{
		_show_icon_special = true;
		_icon_special.sprite = SpriteTextureLoader.getSprite(pPath);
	}

	private void setText(string pNewText, Vector3 pPos, int pAdditionalWidth = 10)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		if (is_mini)
		{
			pAdditionalWidth = 0;
		}
		updatePositionAndRect(pPos);
		if (!(_old_text == pNewText))
		{
			_old_text = pNewText;
			_text_name.text = pNewText;
			_text_width = _text_name.preferredWidth + (float)pAdditionalWidth;
			float y = _rect_background.sizeDelta.y;
			_text_rect.sizeDelta = new Vector2(_text_width, y);
			_last_position = Globals.POINT_IN_VOID_2;
		}
	}

	private void updatePositionAndRect(Vector3 pPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = transformPosition(pPos);
		if (val != _last_position)
		{
			_last_position = val;
			((Component)this).transform.position = Vector2.op_Implicit(val);
			recalcScaledOverlapRect(_rect_background, ref map_text_rect_click);
			float num = ((Rect)(ref map_text_rect_click)).width * 0f * 0.5f;
			float num2 = ((Rect)(ref map_text_rect_click)).height * 0f * 0.5f;
			((Rect)(ref map_text_rect_overlap)).x = ((Rect)(ref map_text_rect_click)).x + num;
			((Rect)(ref map_text_rect_overlap)).y = ((Rect)(ref map_text_rect_click)).y + num2;
			((Rect)(ref map_text_rect_overlap)).width = ((Rect)(ref map_text_rect_click)).width - num * 2f;
			((Rect)(ref map_text_rect_overlap)).height = ((Rect)(ref map_text_rect_click)).height - num2 * 2f;
		}
	}

	private string getStringForNameplate(string pName, int pCount)
	{
		if (is_mini)
		{
			return string.Empty;
		}
		return pName + " " + pCount;
	}

	private string getStringForNameplateLine(string pName, int pCount)
	{
		if (is_mini)
		{
			return string.Empty;
		}
		return pName + " - " + pCount;
	}

	public bool overlapsWithOtherPlate(NameplateText pText)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ((Rect)(ref map_text_rect_overlap)).Overlaps(pText.map_text_rect_overlap);
	}

	private void recalcScaledOverlapRect(RectTransform pRectBackground, ref Rect pMapTextRect)
	{
		recalcScaledOverlapRectSimple(pRectBackground, ref pMapTextRect);
	}

	private void recalcScaledOverlapRectCorners(RectTransform pRectBackground, ref Rect pMapTextRect)
	{
		Vector3[] array = (Vector3[])(object)new Vector3[4];
		pRectBackground.GetWorldCorners(array);
		float num = Mathf.Min(new float[4]
		{
			array[0].x,
			array[1].x,
			array[2].x,
			array[3].x
		});
		float num2 = Mathf.Max(new float[4]
		{
			array[0].x,
			array[1].x,
			array[2].x,
			array[3].x
		});
		float num3 = Mathf.Min(new float[4]
		{
			array[0].y,
			array[1].y,
			array[2].y,
			array[3].y
		});
		float num4 = Mathf.Max(new float[4]
		{
			array[0].y,
			array[1].y,
			array[2].y,
			array[3].y
		});
		((Rect)(ref pMapTextRect)).x = num;
		((Rect)(ref pMapTextRect)).y = num3;
		((Rect)(ref pMapTextRect)).width = num2 - num;
		((Rect)(ref pMapTextRect)).height = num4 - num3;
	}

	private void recalcScaledOverlapRectSimple(RectTransform pRectBackground, ref Rect pMapTextRect)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		float cached_canvas_scale = _manager.cached_canvas_scale;
		Vector2 sizeDelta = pRectBackground.sizeDelta;
		if (is_mini)
		{
			((Vector2)(ref sizeDelta)).Set(60f, 60f);
		}
		float num = sizeDelta.x * 0.55f * cached_canvas_scale;
		float num2 = sizeDelta.y * 0.55f * cached_canvas_scale;
		Vector3 position = ((Transform)pRectBackground).position;
		((Rect)(ref pMapTextRect)).x = position.x - num * 0.5f;
		((Rect)(ref pMapTextRect)).y = position.y - num2 * 0.5f;
		((Rect)(ref pMapTextRect)).width = num;
		((Rect)(ref pMapTextRect)).height = num2;
	}

	private Vector2 transformPosition(Vector3 pVec)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return Vector2.op_Implicit(World.world.camera.WorldToScreenPoint(pVec));
	}

	private bool checkShouldDrawObject(MetaObjectData pData)
	{
		if (_manager.cached_favorites_only && !pData.favorite)
		{
			return false;
		}
		return true;
	}

	public void setAssetAndMeta(NameplateAsset pAsset, NanoObject pNano)
	{
		nano_object = pNano;
		if (_asset != pAsset)
		{
			_active_check_dirty = true;
			_asset = pAsset;
			setNameplateSprite(pAsset.path_sprite);
			RectOffset padding = ((LayoutGroup)layout_group).padding;
			if (is_mini)
			{
				padding.left = 0;
				padding.right = 0;
			}
			else
			{
				padding.left = pAsset.padding_left;
				padding.right = pAsset.padding_right;
			}
			padding.top = pAsset.padding_top;
		}
	}

	public void setNameplateSprite(string pPath)
	{
		Sprite sprite = SpriteTextureLoader.getSprite(pPath);
		_background_image.sprite = sprite;
	}

	private void setPriority(int pValue)
	{
		priority_population = pValue;
	}
}
