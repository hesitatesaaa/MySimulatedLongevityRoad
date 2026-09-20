using UnityEngine;
using UnityEngine.UI;

public class MapSizeTextUpdater : MonoBehaviour
{
	public Text text_counter;

	private void Update()
	{
		updateVars();
	}

	private void updateVars()
	{
		Text component = ((Component)this).GetComponent<Text>();
		string text = LocalizedTextManager.getText(AssetManager.map_sizes.get(Config.customMapSize).getLocaleID());
		component.text = text.ToUpper();
		((Component)component).GetComponent<LocalizedText>().checkSpecialLanguages();
		string[] sizes = MapSizeLibrary.getSizes();
		int num = sizes.IndexOf(Config.customMapSize);
		text_counter.text = num + 1 + "/" + sizes.Length;
	}
}
