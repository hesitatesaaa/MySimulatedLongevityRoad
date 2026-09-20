using UnityEngine;
using UnityEngine.UI;

public class ForgotPasswordButton : MonoBehaviour
{
	public GameObject emailBG;

	public InputField emailInput;

	public Text statusMessage;

	public Button continueButton;

	private Button forgotPasswordButton;

	private bool checking;

	private void OnEnable()
	{
		if (Config.game_loaded)
		{
			newStatus("");
			((Component)emailInput).gameObject.SetActive(true);
			emailBG.gameObject.SetActive(true);
			((Component)continueButton).gameObject.SetActive(false);
			((Component)this).gameObject.SetActive(true);
			forgotPasswordButton = ((Component)this).gameObject.GetComponent<Button>();
			checking = false;
		}
	}

	public void resetPassword()
	{
		checking = true;
		clearStatus();
	}

	private void Update()
	{
		((Selectable)forgotPasswordButton).interactable = !checking;
	}

	private void newStatus(string pMessage)
	{
		Debug.Log((object)("new status " + pMessage));
		if (LocalizedTextManager.stringExists(pMessage))
		{
			((Component)statusMessage).GetComponent<LocalizedText>().key = pMessage;
			((Component)statusMessage).GetComponent<LocalizedText>().updateText();
		}
		else
		{
			statusMessage.text = pMessage;
		}
	}

	private void clearStatus()
	{
		newStatus("");
	}
}
