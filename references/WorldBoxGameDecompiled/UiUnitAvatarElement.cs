using UnityEngine;
using UnityEngine.UI;

public class UiUnitAvatarElement : MonoBehaviour, IBanner, IBaseMono, IRefreshElement
{
	public Image unit_type_bg;

	[SerializeField]
	private Sprite _type_king;

	[SerializeField]
	private Sprite _type_leader;

	[SerializeField]
	private Sprite _type_captain;

	public UnitAvatarLoader avatarLoader;

	public KingdomBanner kingdomBanner;

	public ClanBanner clanBanner;

	public bool show_banner_kingdom = true;

	public bool show_banner_clan = true;

	[SerializeField]
	private Image _tile_graphics_1;

	[SerializeField]
	private Image _tile_graphics_2;

	[SerializeField]
	private Sprite _tile_inside_boat;

	private Actor _actor;

	public MetaCustomizationAsset meta_asset => AssetManager.meta_customization_library.getAsset(MetaType.Unit);

	public MetaTypeAsset meta_type_asset => AssetManager.meta_type_library.getAsset(MetaType.Unit);

	private void Start()
	{
		setupTooltip();
	}

	public void setupTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (!((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			return;
		}
		tipButton.hoverAction = delegate
		{
			if (InputHelpers.mouseSupported)
			{
				tooltipActionActor();
			}
		};
	}

	public void showTooltip()
	{
		tooltipActionActor();
	}

	public void tooltipActionActor()
	{
		if (!_actor.isRekt())
		{
			_actor.showTooltip(this);
		}
	}

	public void load(NanoObject pActor)
	{
		show((Actor)pActor);
	}

	public void show(Actor pActor)
	{
		if (pActor == null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		((Component)this).gameObject.SetActive(true);
		_actor = pActor;
		checkSpecialBg(pActor);
		avatarLoader.load(pActor);
		if (show_banner_kingdom)
		{
			if (pActor.isAlive() && pActor.isKingdomCiv())
			{
				((Component)kingdomBanner).gameObject.SetActive(true);
				kingdomBanner.load(pActor.kingdom);
			}
			else
			{
				((Component)kingdomBanner).gameObject.SetActive(false);
			}
		}
		if (show_banner_clan)
		{
			if (pActor.isAlive() && pActor.hasClan())
			{
				((Component)clanBanner).gameObject.SetActive(true);
				clanBanner.load(pActor.clan);
			}
			else
			{
				((Component)clanBanner).gameObject.SetActive(false);
			}
		}
		updateTileSprite();
		((Object)((Component)this).gameObject).name = "UnitAvatar_" + pActor.data.id;
	}

	public void updateTileSprite()
	{
		if (_actor.isRekt() || _actor.current_tile == null)
		{
			((Component)_tile_graphics_1).gameObject.SetActive(false);
			((Component)_tile_graphics_2).gameObject.SetActive(false);
			return;
		}
		((Component)_tile_graphics_1).gameObject.SetActive(true);
		((Component)_tile_graphics_2).gameObject.SetActive(true);
		if (_actor.is_inside_boat)
		{
			_tile_graphics_1.sprite = _tile_inside_boat;
			_tile_graphics_2.sprite = _tile_inside_boat;
		}
		else
		{
			_tile_graphics_1.sprite = _actor.current_tile.Type.sprites.main.sprite;
			_tile_graphics_2.sprite = _actor.current_tile.Type.sprites.main.sprite;
		}
	}

	private void checkSpecialBg(Actor pActor)
	{
		((Component)unit_type_bg).gameObject.SetActive(false);
		if (pActor.isAlive())
		{
			if (pActor.hasKingdom() && pActor.isKing())
			{
				unit_type_bg.sprite = _type_king;
				((Component)unit_type_bg).gameObject.SetActive(true);
			}
			else if (pActor.hasCity() && pActor.isCityLeader())
			{
				unit_type_bg.sprite = _type_leader;
				((Component)unit_type_bg).gameObject.SetActive(true);
			}
			else if (pActor.is_army_captain)
			{
				unit_type_bg.sprite = _type_captain;
				((Component)unit_type_bg).gameObject.SetActive(true);
			}
		}
	}

	public void showThisActor()
	{
		if (!_actor.isRekt())
		{
			if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(this))
			{
				tooltipActionActor();
			}
			else
			{
				ActionLibrary.openUnitWindow(_actor);
			}
		}
	}

	public Actor getActor()
	{
		return _actor;
	}

	private void OnDisable()
	{
		_actor = null;
	}

	public bool isSameActor(Actor pActor)
	{
		return _actor == pActor;
	}

	public MetaCustomizationAsset GetMetaAsset()
	{
		return AssetManager.meta_customization_library.get("unit");
	}

	public string getName()
	{
		return _actor.getName();
	}

	public NanoObject GetNanoObject()
	{
		return _actor;
	}

	T IBaseMono.GetComponent<T>()
	{
		return ((Component)this).GetComponent<T>();
	}
}
