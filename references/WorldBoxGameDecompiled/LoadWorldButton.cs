using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadWorldButton : MonoBehaviour
{
	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Button val = default(Button);
		if (((Component)this).TryGetComponent<Button>(ref val))
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(loadWorld));
		}
	}

	private void loadWorld()
	{
		if (SaveManager.getCurrentMeta().saveVersion == 15)
		{
			ErrorWindow.errorMessage = "No, abandon it.";
			ScrollWindow.get("error_with_reason").clickShow();
		}
		else
		{
			ScrollWindow.showWindow("save_load_confirm");
		}
	}
}
