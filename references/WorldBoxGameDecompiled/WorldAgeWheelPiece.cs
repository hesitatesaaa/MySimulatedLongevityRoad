using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class WorldAgeWheelPiece : BaseWorldAgeElement, IDropHandler, IEventSystemHandler
{
	public Image mask;

	[SerializeField]
	private Image _highlight;

	[SerializeField]
	private Image _icon_frame;

	private int _index;

	public void init(int pIndex)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		_index = pIndex;
		_tip_button.setHoverAction(_tip_button.showTooltipDefault, pAddAnimation: false);
		((UnityEvent)button.onClick).AddListener(new UnityAction(clickThisPiece));
	}

	public void toggleHighlight(bool pState)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		((Behaviour)_highlight).enabled = pState;
		((Graphic)_highlight).color = asset.pie_selection_color;
		if (((Component)(object)_highlight).HasComponent<FadeInOutAnimation>())
		{
			((Component)_highlight).GetComponent<FadeInOutAnimation>().resetToFadeIn();
		}
	}

	private void clickThisPiece()
	{
		World.world.era_manager.setCurrentSlotIndex(_index, 0f);
	}

	public void toggleIconFrame(bool pState)
	{
		if ((Object)(object)_icon_frame != (Object)null)
		{
			((Behaviour)_icon_frame).enabled = pState;
		}
	}

	public void OnDrop(PointerEventData pEventData)
	{
		if (!((Object)(object)pEventData.pointerDrag == (Object)null))
		{
			WorldAgeButton component = pEventData.pointerDrag.GetComponent<WorldAgeButton>();
			if (!((Object)(object)component == (Object)null))
			{
				WorldAgesWindow.setAgeAndSelectPiece(component.getAsset(), this);
			}
		}
	}

	public bool isCurrentAge()
	{
		return _index == World.world.era_manager.getCurrentSlotIndex();
	}

	public int getIndex()
	{
		return _index;
	}
}
