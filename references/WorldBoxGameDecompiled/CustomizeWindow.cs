using UnityEngine;

public class CustomizeWindow : MonoBehaviour
{
	public ColorElement color_element_prefab;

	public MetaType meta_type;

	private bool _created;

	private void OnEnable()
	{
		if (!_created)
		{
			_created = true;
			AssetManager.meta_customization_library.getAsset(meta_type).customize_component(((Component)this).gameObject);
		}
	}
}
