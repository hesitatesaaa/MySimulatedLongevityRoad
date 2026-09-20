using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WorldListWindow : MonoBehaviour
{
	private static WorldListWindow instance;

	public WorldElement worldElementPrefab;

	public GameObject notFound;

	public ScrollWindow windowWorldList;

	private List<WorldElement> elements = new List<WorldElement>();

	public Transform transformContent;

	public Transform listContent;

	public Transform tagContent;

	public GameObject loadingSpinner;

	public GameObject textStatusBG;

	public Text textStatusMessage;

	public LocalizedText windowTitle;

	public static List<MapTagType> tagsActive = new List<MapTagType>();

	public static string authorId;

	public GameObject sectionTextBG;

	public GameObject profileImage;

	public GameObject filterButton;

	public Text sectionText;

	public Image filterTag1;

	public Image filterTag2;
}
