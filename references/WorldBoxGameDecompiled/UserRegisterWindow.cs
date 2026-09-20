using UnityEngine;
using UnityEngine.UI;

public class UserRegisterWindow : MonoBehaviour
{
	public GameObject page1;

	public GameObject page2;

	public GameObject successPage;

	public GameObject creationPage;

	public Button usernameCheckButton;

	public Button emailCheckButton;

	public InputField inputTextUsername;

	public InputField inputTextEmail;

	public InputField inputTextPassword;

	public Text textMessage;

	private static string _email = "";

	private static string _password = "";

	private static string _username = "";

	public void Start()
	{
		checkState();
	}

	private void OnEnable()
	{
		checkState();
	}

	public void RegisterNewAccount(string username, string password, string email)
	{
		_username = username;
		_password = password;
		_email = email;
	}

	public void registerAccountCallback(string errorReason)
	{
		Config.lockGameControls = false;
	}

	public void checkState()
	{
		Debug.Log((object)"Check Register Window State");
		if (Auth.isLoggedIn)
		{
			setSuccess();
			return;
		}
		setPage1();
		blockRegister1Button();
		blockRegister2Button();
	}

	public void setSuccess()
	{
		Config.lockGameControls = false;
		page2.SetActive(false);
		page1.SetActive(false);
		creationPage.SetActive(false);
		successPage.SetActive(true);
	}

	public void setPage2()
	{
		Config.lockGameControls = false;
		page1.SetActive(false);
		successPage.SetActive(false);
		creationPage.SetActive(false);
		page2.SetActive(true);
	}

	public void setPage1()
	{
		Config.lockGameControls = false;
		page2.SetActive(false);
		successPage.SetActive(false);
		creationPage.SetActive(false);
		page1.SetActive(true);
		InputField obj = inputTextUsername;
		if (!string.IsNullOrEmpty((obj != null) ? obj.text : null))
		{
			RegisterUsername.runUsernameCheck(inputTextUsername);
		}
	}

	public void setCreation()
	{
		Config.lockGameControls = true;
		page1.SetActive(false);
		page2.SetActive(false);
		successPage.SetActive(false);
		creationPage.SetActive(true);
	}

	public void blockRegister1Button()
	{
		((Component)usernameCheckButton).GetComponent<CanvasGroup>().alpha = 0.2f;
		((Selectable)usernameCheckButton).interactable = false;
	}

	public void unblockRegister1Button()
	{
		((Component)usernameCheckButton).GetComponent<CanvasGroup>().alpha = 1f;
		((Selectable)usernameCheckButton).interactable = true;
	}

	public void blockRegister2Button()
	{
		((Component)emailCheckButton).GetComponent<CanvasGroup>().alpha = 0.2f;
		((Selectable)emailCheckButton).interactable = false;
	}

	public void unblockRegister2Button()
	{
		((Component)emailCheckButton).GetComponent<CanvasGroup>().alpha = 1f;
		((Selectable)emailCheckButton).interactable = true;
	}

	public void newStatus(string pMessage)
	{
		Debug.Log((object)("new status " + pMessage));
		if (LocalizedTextManager.stringExists(pMessage))
		{
			((Component)textMessage).GetComponent<LocalizedText>().key = pMessage;
			((Component)textMessage).GetComponent<LocalizedText>().updateText();
		}
		else
		{
			textMessage.text = pMessage;
		}
	}

	public void clearStatus()
	{
		newStatus("");
	}

	public void blockRegisterButton()
	{
	}
}
