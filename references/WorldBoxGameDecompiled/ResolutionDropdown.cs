using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[Obsolete]
public class ResolutionDropdown : MonoBehaviour
{
	private Button button;

	private Dropdown dropdown;

	public OptionBool fullscreenOption;

	private static List<string> options = new List<string>();

	private void Start()
	{
		dropdown = ((Component)this).GetComponent<Dropdown>();
		PopulateDropdown(dropdown);
		((UnityEvent<int>)(object)dropdown.onValueChanged).AddListener((UnityAction<int>)delegate
		{
			DropdownValueChanged(dropdown);
		});
	}

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			dropdown = ((Component)this).GetComponent<Dropdown>();
			PopulateDropdown(dropdown);
		}
	}

	private unsafe void DropdownValueChanged(Dropdown change)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		Resolution[] resolutions = Screen.resolutions;
		if (options[change.value] == LocalizedTextManager.getText("windowed_mode"))
		{
			PlayerConfig.setFullScreen(pFullScreen: false);
		}
		else
		{
			Resolution[] array = resolutions;
			for (int i = 0; i < array.Length; i++)
			{
				Resolution val = array[i];
				if (((object)(*(Resolution*)(&val))/*cast due to constrained. prefix*/).ToString() == options[change.value])
				{
					if (!Screen.fullScreen)
					{
						PlayerConfig.setFullScreen(pFullScreen: true, pSwitchScreen: false);
					}
					Screen.SetResolution(((Resolution)(ref val)).width, ((Resolution)(ref val)).height, true, ((Resolution)(ref val)).refreshRate);
					break;
				}
			}
		}
		fullscreenOption.checkGameOption();
	}

	private unsafe void PopulateDropdown(Dropdown dropdown)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		options.Clear();
		Resolution[] resolutions = Screen.resolutions;
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution val = resolutions[i];
			options.Add(((object)(*(Resolution*)(&val))/*cast due to constrained. prefix*/).ToString());
		}
		options.Add(LocalizedTextManager.getText("windowed_mode"));
		dropdown.ClearOptions();
		options.Reverse();
		int num = options.IndexOf(((object)Screen.currentResolution/*cast due to constrained. prefix*/).ToString());
		if (!Screen.fullScreen)
		{
			num = options.IndexOf(LocalizedTextManager.getText("windowed_mode"));
		}
		dropdown.AddOptions(options);
		if (num > -1)
		{
			dropdown.value = num;
		}
		else
		{
			options.Insert(0, ((object)Screen.currentResolution/*cast due to constrained. prefix*/).ToString());
			dropdown.AddOptions(options);
			dropdown.value = options.IndexOf(((object)Screen.currentResolution/*cast due to constrained. prefix*/).ToString());
		}
		dropdown.RefreshShownValue();
	}
}
