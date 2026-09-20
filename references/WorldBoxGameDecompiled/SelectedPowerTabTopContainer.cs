using UnityEngine;
using UnityEngine.UI;

public class SelectedPowerTabTopContainer : MonoBehaviour
{
	[SerializeField]
	private Image _icon_unit;

	public bool goUp;

	public bool goDown;

	private bool _going_down;

	private bool _going_up;

	private RectTransform _rect;

	private float _timer;

	private void Awake()
	{
		_rect = ((Component)this).GetComponent<RectTransform>();
	}

	private void Update()
	{
		if (SelectedUnit.isSet())
		{
			Actor unit = SelectedUnit.unit;
			if (!unit.isRekt())
			{
				Sprite spriteIcon = unit.asset.getSpriteIcon();
				_icon_unit.sprite = spriteIcon;
			}
		}
		else if (SelectedObjects.isNanoObjectSet())
		{
			NanoObject selectedNanoObject = SelectedObjects.getSelectedNanoObject();
			if (!selectedNanoObject.isRekt())
			{
				MetaTypeAsset metaTypeAsset = selectedNanoObject.getMetaTypeAsset();
				if (metaTypeAsset.set_icon_for_cancel_button)
				{
					Sprite iconSprite = metaTypeAsset.getIconSprite();
					_icon_unit.sprite = iconSprite;
				}
			}
		}
		if (goDown != _going_down)
		{
			_going_down = goDown;
			_timer = 0f;
			if (goDown)
			{
				_timer = 0.95f;
			}
		}
		if (goUp != _going_up)
		{
			_going_up = goUp;
			_timer = -1f;
		}
		if (_timer < 1f)
		{
			_timer += Time.deltaTime / 2f;
			_timer = Mathf.Clamp(_timer, 0f, 1f);
		}
	}

	public void cancelSelection()
	{
		SelectedUnit.clear();
		SelectedObjects.unselectNanoObject();
		PowersTab.unselect();
	}
}
