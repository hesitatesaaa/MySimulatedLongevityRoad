using UnityEngine;

public class RegisterButton : MonoBehaviour
{
	public UserRegisterWindow userRegisterWindow;

	public void usernameCheck()
	{
		Debug.Log((object)("Name:  " + userRegisterWindow.inputTextUsername.text));
		userRegisterWindow.setPage2();
	}

	public void tryRegister()
	{
		clearStatus();
		Debug.Log((object)("Name:  " + userRegisterWindow.inputTextUsername.text));
		Debug.Log((object)("Email: " + userRegisterWindow.inputTextEmail.text));
		string text = userRegisterWindow.inputTextUsername.text;
		string text2 = userRegisterWindow.inputTextEmail.text;
		string text3 = userRegisterWindow.inputTextPassword.text;
		if (text2 == "" || text3 == "")
		{
			newStatus("EmailPasswordEmpty");
		}
		else if (!Auth.isValidEmail(text2))
		{
			newStatus("InvalidEmail");
		}
		else if (text3.Length < 6)
		{
			newStatus("ShortPassword");
		}
		else
		{
			userRegisterWindow.RegisterNewAccount(text, text3, text2);
		}
	}

	private void sendVerification()
	{
		Debug.Log((object)"send verification");
	}

	private void newStatus(string pMessage)
	{
		Debug.Log((object)("new status " + pMessage));
		if (LocalizedTextManager.stringExists(pMessage))
		{
			((Component)userRegisterWindow.textMessage).GetComponent<LocalizedText>().key = pMessage;
			((Component)userRegisterWindow.textMessage).GetComponent<LocalizedText>().updateText();
		}
		else
		{
			userRegisterWindow.textMessage.text = pMessage;
		}
	}

	private void clearStatus()
	{
		newStatus("");
	}
}
