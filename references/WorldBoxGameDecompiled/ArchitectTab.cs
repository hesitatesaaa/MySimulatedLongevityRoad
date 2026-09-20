using System.Collections.Generic;
using UnityEngine;

public class ArchitectTab : MonoBehaviour
{
	private Dictionary<ArchitectMood, ArchitectMoodButton> _buttons = new Dictionary<ArchitectMood, ArchitectMoodButton>();

	[SerializeField]
	private ArchitectMoodButton _mood_prefab;

	[SerializeField]
	private Transform _grid_placement;

	private void Awake()
	{
		initButtons();
	}

	private void initButtons()
	{
		for (int i = 0; i < AssetManager.architect_mood_library.list.Count; i++)
		{
			ArchitectMood architectMood = AssetManager.architect_mood_library.list[i];
			ArchitectMoodButton value = initButton(architectMood);
			_buttons.Add(architectMood, value);
		}
	}

	private ArchitectMoodButton initButton(ArchitectMood pAsset)
	{
		ArchitectMoodButton architectMoodButton = Object.Instantiate<ArchitectMoodButton>(_mood_prefab, _grid_placement);
		architectMoodButton.setAsset(pAsset);
		architectMoodButton.addClickCallback(buttonAction);
		return architectMoodButton;
	}

	private void buttonAction(ArchitectMoodButton pElement)
	{
		ArchitectMood asset = pElement.getAsset();
		World.world.map_stats.player_mood = asset.id;
		World.world.clearArchitectMood();
		updateElements();
	}

	private void updateElements()
	{
		ArchitectMood architectMood = World.world.getArchitectMood();
		foreach (ArchitectMoodButton value in _buttons.Values)
		{
			bool flag = value.getAsset() == architectMood;
			value.toggleSelectedButton(flag);
			value.setIconActiveColor(flag);
		}
	}

	private void OnEnable()
	{
		updateElements();
	}
}
