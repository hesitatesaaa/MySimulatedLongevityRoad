using System;
using UnityEngine;

public class MetaListNoItems : MonoBehaviour
{
	private GameObject _inner;

	private IMetaWindow _window;

	protected IMetaObject meta_object => _window.getCoreObject() as IMetaObject;

	private void Awake()
	{
		_inner = ((Component)((Component)this).transform.GetChild(0)).gameObject;
		_window = ((Component)this).GetComponentInParent<IMetaWindow>();
	}

	private void OnEnable()
	{
		_inner.SetActive(!hasMetas());
	}

	protected virtual bool hasMetas()
	{
		throw new NotImplementedException();
	}
}
