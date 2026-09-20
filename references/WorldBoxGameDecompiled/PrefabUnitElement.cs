using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class PrefabUnitElement : WindowListElementBaseActor, IPointerMoveHandler, IEventSystemHandler
{
	[Serializable]
	[CompilerGenerated]
	private sealed class _003C_003Ec
	{
		public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

		public static UnityAction _003C_003E9__20_0;

		internal void _003CinitTooltip_003Eb__20_0()
		{
			Tooltip.hideTooltip();
		}
	}

	private Actor _actor;

	public Text unitName;

	public UiUnitAvatarElement avatarElement;

	public StatBar health_bar;

	public CountUpOnClick text_damage;

	public CountUpOnClick text_level;

	public CountUpOnClick text_kills;

	public CountUpOnClick text_age;

	public Image icon_sex;

	[SerializeField]
	private Image _icon_species;

	[SerializeField]
	private GameObject _icon_favorite;

	private void Awake()
	{
		initTooltip();
	}

	internal void show(Actor pActor)
	{
		_actor = pActor;
		unitName.text = pActor.coloredName;
		avatarElement.show(pActor);
		health_bar.setBar(pActor.getHealth(), pActor.getMaxHealth(), "");
		text_level.setValue(pActor.level);
		text_kills.setValue(pActor.data.kills);
		text_age.setValue(pActor.getAge());
		if (pActor.asset.inspect_sex)
		{
			((Component)icon_sex).gameObject.SetActive(true);
			if (pActor.isSexMale())
			{
				icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconMale");
			}
			else
			{
				icon_sex.sprite = SpriteTextureLoader.getSprite("ui/icons/IconFemale");
			}
		}
		else
		{
			((Component)icon_sex).gameObject.SetActive(false);
		}
		_icon_species.sprite = _actor.asset.getSpriteIcon();
		toggleFavorited(_actor.isFavorite());
	}

	public void clickLocate()
	{
		WorldLog.locationFollow(_actor);
	}

	public void clickInspect()
	{
		if (!InputHelpers.mouseSupported && !Tooltip.isShowingFor(this))
		{
			tooltipAction();
		}
		else
		{
			ActionLibrary.openUnitWindow(_actor);
		}
	}

	public Actor getActor()
	{
		return _actor;
	}

	public void toggleFavorited(bool pState)
	{
		_icon_favorite.SetActive(pState);
	}

	private void OnDisable()
	{
		_actor = null;
	}

	public void OnPointerMove(PointerEventData pData)
	{
		if (InputHelpers.mouseSupported && !Tooltip.anyActive())
		{
			tooltipAction();
		}
	}

	private void tooltipAction()
	{
		_actor.showTooltip(this);
	}

	private void initTooltip()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		Button component = ((Component)this).GetComponent<Button>();
		object obj = _003C_003Ec._003C_003E9__20_0;
		if (obj == null)
		{
			UnityAction val = delegate
			{
				Tooltip.hideTooltip();
			};
			_003C_003Ec._003C_003E9__20_0 = val;
			obj = (object)val;
		}
		component.OnHoverOut((UnityAction)obj);
	}
}
