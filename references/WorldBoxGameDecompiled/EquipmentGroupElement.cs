public class EquipmentGroupElement : AugmentationCategory<EquipmentAsset, EquipmentButton, EquipmentEditorButton>
{
	protected override bool isUnlocked(EquipmentButton pButton)
	{
		if (pButton.getElementAsset().isAvailable())
		{
			return true;
		}
		return false;
	}
}
