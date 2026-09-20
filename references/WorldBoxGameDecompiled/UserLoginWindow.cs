using UnityEngine;
using UnityEngine.UI;

public class UserLoginWindow : MonoBehaviour
{
	public GameObject groupLogged;

	public GameObject groupLogin;

	public GameObject groupLoading;

	public Text usernameText;

	public Text windowTitle;

	public InputField inputTextUser;

	public InputField inputTextPassword;

	public Text textMessage;

	private bool isLoggedIn;

	public void Start()
	{
		checkState();
		if (PlayerConfig.dict["username"].stringVal != "")
		{
			inputTextUser.text = PlayerConfig.dict["username"].stringVal;
		}
	}

	public void checkState()
	{
		Debug.Log((object)"Check Login Window State");
		if (Auth.isLoggedIn)
		{
			if (Auth.displayName != "" && Auth.displayName != null)
			{
				Debug.Log((object)"displayName found");
				usernameText.text = Auth.displayName;
			}
			else if (Auth.userName != "" && Auth.userName != null)
			{
				Debug.Log((object)"userName found");
				usernameText.text = Auth.userName;
			}
			else
			{
				Debug.Log((object)"emailAddress found");
				usernameText.text = Auth.emailAddress;
			}
			setLogout();
		}
		else
		{
			setLogin();
		}
		isLoggedIn = Auth.isLoggedIn;
	}

	public void Update()
	{
		if (isLoggedIn != Auth.isLoggedIn)
		{
			checkState();
		}
	}

	public void setLoading()
	{
		((Component)windowTitle).GetComponent<LocalizedText>().key = "logging_in";
		((Component)windowTitle).GetComponent<LocalizedText>().updateText();
		groupLogin.SetActive(false);
		groupLogged.SetActive(false);
		groupLoading.SetActive(true);
	}

	public void setLogin()
	{
		((Component)windowTitle).GetComponent<LocalizedText>().key = "Login";
		((Component)windowTitle).GetComponent<LocalizedText>().updateText();
		groupLogged.SetActive(false);
		groupLoading.SetActive(false);
		groupLogin.SetActive(true);
	}

	public void setLogout()
	{
		((Component)windowTitle).GetComponent<LocalizedText>().key = "welcome_worldnet";
		((Component)windowTitle).GetComponent<LocalizedText>().updateText();
		groupLogin.SetActive(false);
		groupLoading.SetActive(false);
		groupLogged.SetActive(true);
	}

	public void clearWindow(string pMessage = "...")
	{
		textMessage.text = pMessage;
		inputTextPassword.text = "";
		inputTextUser.text = "";
	}

	public void clearCredentials()
	{
		inputTextPassword.text = "";
		inputTextUser.text = "";
		if (PlayerConfig.dict["username"].stringVal != "")
		{
			inputTextUser.text = PlayerConfig.dict["username"].stringVal;
		}
	}
}
