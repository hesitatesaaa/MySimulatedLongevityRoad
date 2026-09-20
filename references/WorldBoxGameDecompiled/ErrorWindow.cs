using UnityEngine;
using UnityEngine.UI;

public class ErrorWindow : MonoBehaviour
{
	public Text errorText;

	public static string errorMessage;

	private void OnEnable()
	{
		errorText.text = errorMessage;
	}
}
