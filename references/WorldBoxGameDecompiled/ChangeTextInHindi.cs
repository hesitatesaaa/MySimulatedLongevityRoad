using UnityEngine;
using UnityEngine.UI;

public class ChangeTextInHindi : MonoBehaviour
{
	private void Start()
	{
		string text = ((Component)this).gameObject.GetComponent<Text>().text;
		((Component)this).gameObject.GetComponent<Text>().SetHindiText(text);
	}
}
