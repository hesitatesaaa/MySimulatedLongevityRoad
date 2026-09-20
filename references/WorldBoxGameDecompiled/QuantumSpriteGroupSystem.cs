using UnityEngine;

public class QuantumSpriteGroupSystem : SpriteGroupSystem<QuantumSprite>
{
	private string _path;

	private QuantumSpriteAsset _asset;

	private QuantumSpriteCacheData _cache_data;

	public void create(QuantumSpriteAsset pAsset)
	{
		_path = pAsset.id_prefab;
		_asset = pAsset;
		create();
		((Object)((Component)this).transform).name = pAsset.id;
	}

	public QuantumSpriteCacheData getCacheData(int pSize)
	{
		if (_cache_data == null)
		{
			_cache_data = new QuantumSpriteCacheData(pSize);
		}
		else
		{
			_cache_data.checkSize(pSize);
		}
		return _cache_data;
	}

	public override void deactivate(QuantumSprite pObject)
	{
	}

	public override void checkActiveAction(QuantumSprite pObject)
	{
	}

	protected override QuantumSprite createNew()
	{
		QuantumSprite quantumSprite = base.createNew();
		if (_asset.create_object != null)
		{
			_asset.create_object(_asset, quantumSprite);
		}
		return quantumSprite;
	}

	public override void create()
	{
		base.create();
		prefab = Resources.Load<QuantumSprite>("civ/" + _path);
		if ((Object)(object)prefab == (Object)null)
		{
			Debug.LogError((object)("Missing Prefab " + _path));
		}
	}
}
