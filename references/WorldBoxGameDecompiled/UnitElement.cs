using UnityEngine;

public abstract class UnitElement : WindowMetaElementBase, IRefreshElement
{
	protected Actor actor;

	protected UnitWindow unit_window;

	protected override void Awake()
	{
		checkSetWindow();
		base.Awake();
	}

	protected virtual void checkSetWindow()
	{
		unit_window = ((Component)this).GetComponentInParent<UnitWindow>();
	}

	protected override void OnEnable()
	{
		checkSetActor();
		base.OnEnable();
	}

	protected virtual void checkSetActor()
	{
		setActor(unit_window.actor);
	}

	protected virtual void setActor(Actor pActor)
	{
		actor = pActor;
	}

	public override bool checkRefreshWindow()
	{
		if (actor.isRekt())
		{
			return true;
		}
		if (!actor.hasHealth())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		actor = null;
	}
}
