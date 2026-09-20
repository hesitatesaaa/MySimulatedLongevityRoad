using System.Collections;
using UnityEngine;

public class UnitStatusContainer : UnitElement
{
	[SerializeField]
	private StatusEffectButton _prefab_status;

	[SerializeField]
	private Transform _grid;

	private ObjectPoolGenericMono<StatusEffectButton> _pool_status;

	protected override void Awake()
	{
		_pool_status = new ObjectPoolGenericMono<StatusEffectButton>(_prefab_status, _grid);
		base.Awake();
	}

	protected override IEnumerator showContent()
	{
		if (actor == null || !actor.isAlive() || !actor.hasAnyStatusEffect())
		{
			yield break;
		}
		((Component)_grid).gameObject.SetActive(true);
		yield return (object)new WaitForSecondsRealtime(0.025f);
		foreach (Status tData in actor.getStatuses())
		{
			if (!tData.is_finished)
			{
				yield return CoroutineHelper.wait_for_next_frame;
				loadStatusButton(tData);
			}
		}
	}

	private void loadStatusButton(Status pStatus)
	{
		_pool_status.getNext().load(pStatus);
	}

	protected override void clear()
	{
		_pool_status?.clear();
		base.clear();
	}
}
