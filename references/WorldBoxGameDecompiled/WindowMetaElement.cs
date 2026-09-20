using UnityEngine;

public class WindowMetaElement<TMetaObject, TData> : WindowMetaElementBase where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData
{
	protected TMetaObject meta_object;

	protected WindowMetaGeneric<TMetaObject, TData> window;

	protected override void Awake()
	{
		window = ((Component)this).GetComponentInParent<WindowMetaGeneric<TMetaObject, TData>>();
		base.Awake();
	}

	protected override void OnEnable()
	{
		meta_object = window.getMetaObject();
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		meta_object = null;
	}

	public override bool checkRefreshWindow()
	{
		if (meta_object.isRekt())
		{
			return true;
		}
		return base.checkRefreshWindow();
	}
}
