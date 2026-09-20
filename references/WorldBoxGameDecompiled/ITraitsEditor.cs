using UnityEngine;

public interface ITraitsEditor<TTrait> : IAugmentationsEditor where TTrait : BaseTrait<TTrait>
{
	ITraitsOwner<TTrait> getTraitsOwner();

	void scrollToGroupStarter(GameObject pTraitButton);

	void scrollToGroupStarter(GameObject pTraitButton, bool pIgnoreTooltipCheck);

	WindowMetaTab getEditorTab();
}
