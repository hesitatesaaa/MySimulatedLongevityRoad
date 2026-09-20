using UnityEngine;

public class ActorSelectedContainerEquipment : SelectedElementBase<EquipmentButton>
{
	[SerializeField]
	private EquipmentButton _prefab_equipment;

	private void Awake()
	{
		_pool = new ObjectPoolGenericMono<EquipmentButton>(_prefab_equipment, _grid);
	}

	public void update(Actor pActor)
	{
		if (!pActor.hasEquipment())
		{
			clear();
		}
		else
		{
			refresh(pActor);
		}
	}

	protected override void refresh(NanoObject pNano)
	{
		clear();
		foreach (Item item in ((Actor)pNano).equipment.getItems())
		{
			loadEquipmentButton(item);
		}
	}

	private void loadEquipmentButton(Item pItem)
	{
		_pool.getNext().load(pItem);
	}
}
