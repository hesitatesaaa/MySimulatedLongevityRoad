using UnityEngine;

public class FamiliesNoItems : MonoBehaviour
{
	private GameObject _inner;

	private IMetaWithFamiliesWindow _families_window;

	private void Awake()
	{
		_inner = ((Component)((Component)this).transform.GetChild(0)).gameObject;
		_families_window = ((Component)this).GetComponentInParent<IMetaWithFamiliesWindow>();
	}

	private void OnEnable()
	{
		_inner.SetActive(!_families_window.hasFamilies());
	}
}
