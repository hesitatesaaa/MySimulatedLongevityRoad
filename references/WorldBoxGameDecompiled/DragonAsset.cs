using System.Collections.Generic;
using UnityEngine;

public class DragonAsset : ScriptableObject
{
	private Dictionary<DragonState, DragonAssetContainer> _dict;

	public DragonAssetContainer[] list;

	public DragonAssetContainer getAsset(DragonState pState)
	{
		if (_dict == null)
		{
			_dict = new Dictionary<DragonState, DragonAssetContainer>();
			DragonAssetContainer[] array = list;
			foreach (DragonAssetContainer dragonAssetContainer in array)
			{
				_dict.Add(dragonAssetContainer.id, dragonAssetContainer);
			}
		}
		return _dict[pState];
	}
}
