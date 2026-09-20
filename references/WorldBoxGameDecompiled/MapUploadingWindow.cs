using UnityEngine;
using UnityEngine.UI;

public class MapUploadingWindow : MonoBehaviour
{
	public Button doneButton;

	public Image loadingImage;

	public Image doneImage;

	public GameObject mapIDGroup;

	public Text mapIDText;

	public Text statusMessage;

	public Text percents;

	public Image bar;

	public Image mask;

	public static bool uploading;
}
