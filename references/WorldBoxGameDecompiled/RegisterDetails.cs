using UnityEngine;
using UnityEngine.UI;

public class RegisterDetails : MonoBehaviour
{
	private static bool emailValid;

	private static bool passwordValid;

	private void OnEnable()
	{
		checkButton();
	}

	public void emailCheck(InputField pEmail)
	{
		runEmailCheck(pEmail);
	}

	public static void runEmailCheck(InputField pEmail)
	{
		string text = pEmail.text;
		emailValid = false;
		Debug.Log((object)("Name: " + text));
		if (!Auth.isValidEmail(text))
		{
			newStatus("InvalidEmail");
			checkButton();
			Debug.Log((object)"Not valid");
		}
		else
		{
			clearStatus();
			emailValid = true;
			checkButton();
		}
	}

	public void passwordCheck(InputField pEmail)
	{
		runPasswordCheck(pEmail);
	}

	public static void runPasswordCheck(InputField pPassword)
	{
		string text = pPassword.text;
		passwordValid = false;
		Debug.Log((object)("Pass: " + text));
		if (text.Length < 6)
		{
			newStatus("ShortPassword");
			checkButton();
			Debug.Log((object)"Not valid");
		}
		else
		{
			clearStatus();
			passwordValid = true;
			checkButton();
		}
	}

	private static void checkButton()
	{
		if (emailValid && passwordValid)
		{
			unblockRegisterButton();
		}
		else
		{
			blockRegisterButton();
		}
	}

	private static void blockRegisterButton()
	{
		if (registerWindowExists())
		{
			((Component)ScrollWindow.get("register")).GetComponent<UserRegisterWindow>().blockRegister2Button();
		}
	}

	private static void unblockRegisterButton()
	{
		if (registerWindowExists())
		{
			((Component)ScrollWindow.get("register")).GetComponent<UserRegisterWindow>().unblockRegister2Button();
		}
	}

	private static void newStatus(string pMessage)
	{
		if (registerWindowExists())
		{
			((Component)ScrollWindow.get("register")).GetComponent<UserRegisterWindow>().newStatus(pMessage);
		}
	}

	private static bool registerWindowExists()
	{
		if ((Object)(object)ScrollWindow.get("register") != (Object)null)
		{
			return (Object)(object)((Component)ScrollWindow.get("register")).GetComponent<UserRegisterWindow>() != (Object)null;
		}
		return false;
	}

	private static void clearStatus()
	{
		newStatus("");
	}
}
