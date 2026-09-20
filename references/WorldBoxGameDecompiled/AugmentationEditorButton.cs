using UnityEngine;
using UnityEngine.UI;

public class AugmentationEditorButton<TAugmentationButton, TAugmentation> : MonoBehaviour where TAugmentationButton : AugmentationButton<TAugmentation> where TAugmentation : BaseAugmentationAsset
{
	public TAugmentationButton augmentation_button;

	public Image selected_icon;
}
