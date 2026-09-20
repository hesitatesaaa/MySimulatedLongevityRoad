using UnityEngine;

public class SelectedNano<TNanoObject> : SelectedNanoBase where TNanoObject : NanoObject, IFavoriteable
{
	[SerializeField]
	private GameObject _container_banners_parent;

	private ISelectedContainerTrait _container_traits;

	private ISelectedTabBanners<TNanoObject> _container_banners;

	private int _last_dirty_stats;

	private TNanoObject _last_nano;

	protected virtual TNanoObject nano_object => SelectedObjects.getSelectedNanoObject() as TNanoObject;

	protected override void Awake()
	{
		base.Awake();
		_container_traits = ((Component)this).GetComponentInChildren<ISelectedContainerTrait>();
		if ((Object)(object)_container_banners_parent != (Object)null)
		{
			_container_banners = _container_banners_parent.GetComponentInChildren<ISelectedTabBanners<TNanoObject>>();
		}
	}

	private void OnDisable()
	{
		clearLastObject();
	}

	public override void update()
	{
		if (_last_nano.isRekt())
		{
			_last_nano = null;
		}
		updateElements(nano_object);
	}

	protected virtual void updateElements(TNanoObject pNano)
	{
		if (pNano.isRekt())
		{
			clearLastObject();
			return;
		}
		updateFavoriteIcon(pNano.isFavorite());
		if (isNanoChanged(pNano))
		{
			updateElementsOnChange(pNano);
		}
		updateElementsAlways(pNano);
		_last_dirty_stats = pNano.getStatsDirtyVersion();
		_last_nano = pNano;
	}

	protected virtual void updateElementsOnChange(TNanoObject pNano)
	{
		showStatsGeneral(pNano);
		updateTraits();
		updateBanners(pNano);
		checkAchievements(pNano);
		World.world.selected_buttons.clearHighlightedButton();
	}

	protected virtual void checkAchievements(TNanoObject pNano)
	{
	}

	protected virtual void updateElementsAlways(TNanoObject pNano)
	{
		recalcTabSize();
	}

	protected virtual void showStatsGeneral(TNanoObject pNano)
	{
	}

	private void updateBanners(TNanoObject pNano)
	{
		if (_container_banners != null)
		{
			_container_banners.update(pNano);
			if (_container_banners.countVisibleBanners() > 0)
			{
				_container_banners_parent.gameObject.SetActive(true);
			}
			else
			{
				_container_banners_parent.gameObject.SetActive(false);
			}
		}
	}

	protected virtual void updateTraits()
	{
		if (_container_traits != null)
		{
			_container_traits.update(nano_object);
		}
	}

	protected virtual void clearLastObject()
	{
		_last_nano = null;
		_last_dirty_stats = -1;
	}

	protected void recalcTabSize()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		ScrollRectExtended scrollRect = PowerTabController.instance.scrollRect;
		if (scrollRect.isDragged() || Mathf.Abs(scrollRect.velocity.x) > 1f)
		{
			_powers_tab.sortButtons();
			return;
		}
		float horizontalNormalizedPosition = (scrollRect.isHorizontalScrollAvailable() ? scrollRect.horizontalNormalizedPosition : 0f);
		Vector2 velocity = scrollRect.velocity;
		if (_powers_tab.recalc())
		{
			_powers_tab.sortButtons();
		}
		scrollRect.horizontalNormalizedPosition = horizontalNormalizedPosition;
		scrollRect.velocity = velocity;
	}

	protected bool isNanoChanged(TNanoObject pNano)
	{
		if (pNano.getStatsDirtyVersion() == _last_dirty_stats)
		{
			return pNano != _last_nano;
		}
		return true;
	}
}
