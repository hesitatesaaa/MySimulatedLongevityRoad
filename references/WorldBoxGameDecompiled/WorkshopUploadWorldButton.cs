using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WorkshopUploadWorldButton : MonoBehaviour
{
	public Text title;

	public Text description;

	public GameObject quickError;

	public Text errorMessage;

	private void Start()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected O, but got Unknown
		Button val = default(Button);
		if (((Component)this).TryGetComponent<Button>(ref val))
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(uploadWorldToWorkshop));
		}
	}

	private void OnEnable()
	{
		quickError.SetActive(false);
	}

	private void uploadWorldToWorkshop()
	{
		quickError.SetActive(false);
		if (string.IsNullOrWhiteSpace(title.text))
		{
			errorMessage.text = "Give your world a name!";
			quickError.SetActive(true);
		}
		else if (string.IsNullOrWhiteSpace(description.text))
		{
			errorMessage.text = "Give your world a description!";
			quickError.SetActive(true);
		}
		else
		{
			ScrollWindow.showWindow("steam_workshop_uploading");
		}
	}

	public void closeError()
	{
		quickError.SetActive(false);
	}
}
