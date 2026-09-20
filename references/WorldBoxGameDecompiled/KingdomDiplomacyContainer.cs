using UnityEngine;

public class KingdomDiplomacyContainer<TBanner, TMetaObject, TData> : KingdomElement where TBanner : BannerGeneric<TMetaObject, TData> where TMetaObject : CoreSystemObject<TData> where TData : BaseSystemData
{
	protected ObjectPoolGenericMono<TBanner> pool_elements;

	[SerializeField]
	private TBanner _prefab;

	[SerializeField]
	private Transform _container;

	protected override void Awake()
	{
		pool_elements = new ObjectPoolGenericMono<TBanner>(_prefab, _container);
		base.Awake();
	}

	protected override void clear()
	{
		pool_elements.clear();
		base.clear();
	}
}
