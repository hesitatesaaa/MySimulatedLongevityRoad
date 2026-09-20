public interface IEquipmentWindow : IAugmentationsWindow<IEquipmentEditor>
{
	void reloadEquipment()
	{
		GetComponentInChildren<UnitEquipmentContainer>().reloadEquipment(pAnimated: false);
	}

	void checkEquipmentTabIcon();
}
