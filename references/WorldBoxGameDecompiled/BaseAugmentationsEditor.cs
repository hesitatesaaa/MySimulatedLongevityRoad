using System;
using System.Collections.Generic;
using LayoutGroupExt;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BaseAugmentationsEditor : MonoBehaviour
{
	public Transform augmentation_groups_parent;

	public Text text_counter_augmentations;

	public LocalizedText window_title_text;

	public Image power_icon;

	public Transform powers_icons;

	public GridLayoutGroupExtended selected_editor_augmentations_grid;

	public RainSwitcherButton rain_state_switcher;

	protected List<string> augmentations_list_link;

	protected readonly HashSet<string> augmentations_hashset = new HashSet<string>();

	public bool rain_editor;

	public RainState rain_editor_state;

	private bool _groups_initialized;

	private bool _created;

	private StatsWindow _stats_window;

	protected ToggleRainStateAction rain_state_toggle_action;

	private void Awake()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Expected O, but got Unknown
		create();
		_stats_window = ((Component)this).GetComponentInParent<StatsWindow>();
		RainSwitcherButton rainSwitcherButton = rain_state_switcher;
		if (rainSwitcherButton != null)
		{
			((UnityEvent)rainSwitcherButton.getButton().onClick).AddListener((UnityAction)delegate
			{
				rain_state_toggle_action();
			});
		}
	}

	protected virtual void OnEnable()
	{
		reloadButtons();
		checkEnabledGroups();
		if (!rain_editor)
		{
			_stats_window.updateStats();
		}
	}

	protected virtual void create()
	{
		if (!_created)
		{
			_created = true;
		}
	}

	protected virtual void onEnableRain()
	{
		throw new NotImplementedException();
	}

	public virtual void reloadButtons()
	{
		if (((Component)this).gameObject.activeInHierarchy)
		{
			loadAugmentationGroups();
		}
	}

	protected virtual void showActiveButtons()
	{
		throw new NotImplementedException();
	}

	private void loadAugmentationGroups()
	{
		if (!_groups_initialized)
		{
			_groups_initialized = true;
			groupsBuilder();
		}
	}

	protected virtual void checkEnabledGroups()
	{
		throw new NotImplementedException();
	}

	protected virtual void groupsBuilder()
	{
		throw new NotImplementedException();
	}

	protected virtual void startSignal()
	{
	}

	protected virtual void onNanoWasModified()
	{
		throw new NotImplementedException();
	}

	protected virtual void toggleRainState(ref RainState pState)
	{
		if (pState == RainState.Add)
		{
			pState = RainState.Remove;
			rain_state_switcher.toggleState(pState: true);
		}
		else
		{
			pState = RainState.Add;
			rain_state_switcher.toggleState(pState: false);
		}
		rain_editor_state = pState;
		reloadButtons();
	}
}
