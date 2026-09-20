using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SaveWorldButton : MonoBehaviour
{
	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Button val = default(Button);
		if (((Component)this).TryGetComponent<Button>(ref val))
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(saveWorld));
		}
	}

	private void saveWorld()
	{
		ScrollWindow.showWindow("save_world_confirm");
	}
}
