using UnityEngine;
using UnityEngine.UI;

public class PrefabLibrary : MonoBehaviour
{
	internal static PrefabLibrary instance;

	public GameObject graphy;

	public DebugTool debugTool;

	public DragonAsset dragonAsset;

	public DragonAsset zombieDragonAsset;

	public Image iconLock;

	private void Awake()
	{
		instance = this;
	}
}
