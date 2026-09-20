using System.Collections.Generic;
using UnityEngine;

public class DropManager
{
	private List<Drop> _drops;

	private float _timeout_timer;

	private int _activeIndex;

	private GameObject _original_drop;

	private Transform _dropContainer;

	public DropManager(Transform pDropContainer)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		_drops = new List<Drop>();
		base._002Ector();
		_dropContainer = pDropContainer;
		string text = "effects/p_drop";
		_original_drop = (GameObject)Resources.Load(text, typeof(GameObject));
	}

	public Drop spawn(WorldTile pTile, string pDropID, float zHeight = -1f, float pScale = -1f, long pCasterId = -1L)
	{
		DropAsset pAsset = AssetManager.drops.get(pDropID);
		return spawn(pTile, pAsset, zHeight, pScale, pForceSurprise: false, pCasterId);
	}

	public Drop spawn(WorldTile pTile, DropAsset pAsset, float zHeight = -1f, float pScale = -1f, bool pForceSurprise = false, long pCasterId = -1L)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Drop drop = getObject();
		if (pForceSurprise)
		{
			drop.setForceSurprise();
		}
		drop.launchStraight(pTile, pAsset, zHeight);
		if (pScale == -1f)
		{
			pScale = pAsset.default_scale;
		}
		drop.setScale(new Vector3(pScale, pScale, ((Component)drop).transform.localScale.z));
		drop.setCasterId(pCasterId);
		return drop;
	}

	public void spawnParabolicDrop(WorldTile pTile, string pDropID, float pStartHeight = 0f, float pMinHeight = 0f, float pMaxHeight = 0f, float pMinRadius = 0f, float pMaxRadius = 0f, float pScale = -1f)
	{
		spawn(pTile, pDropID, pMinHeight, pScale, -1L).launchParabolic(pStartHeight, pMinHeight, pMaxHeight, pMinRadius, pMaxRadius);
	}

	public void clear()
	{
		List<Drop> drops = _drops;
		for (int i = 0; i < drops.Count; i++)
		{
			drops[i].makeInactive();
		}
		_activeIndex = 0;
	}

	private void killObject(Drop pObject)
	{
		pObject.makeInactive();
		int num = pObject.drop_index - 1;
		int num2 = _activeIndex - 1;
		List<Drop> drops = _drops;
		if (num != num2)
		{
			Drop drop = drops[num2];
			drops[num2] = pObject;
			drops[num] = drop;
			pObject.drop_index = num2 + 1;
			drop.drop_index = num + 1;
		}
		if (_activeIndex > 0)
		{
			_activeIndex--;
		}
	}

	public void landDrop(Drop pObject)
	{
		WorldTile current_tile = pObject.current_tile;
		killObject(pObject);
		if (current_tile != null)
		{
			World.world.flash_effects.flashPixel(current_tile, 14);
		}
	}

	public Drop getObject()
	{
		List<Drop> drops = _drops;
		Drop drop;
		if (drops.Count > _activeIndex)
		{
			drop = drops[_activeIndex];
		}
		else
		{
			drop = Object.Instantiate<GameObject>(_original_drop, _dropContainer).GetComponent<Drop>();
			((Component)drop).gameObject.layer = ((Component)_dropContainer).gameObject.layer;
			((Component)drop).transform.parent = _dropContainer;
			drops.Add(drop);
			drop.drop_index = drops.Count;
		}
		_activeIndex++;
		drop.prepare();
		return drop;
	}

	public void update(float pElapsed)
	{
		Bench.bench("drops", "game_total");
		if (_timeout_timer > 0f)
		{
			_timeout_timer -= World.world.delta_time;
		}
		int num = 0;
		List<Drop> drops = _drops;
		for (num = _activeIndex - 1; num >= 0; num--)
		{
			Drop drop = drops[num];
			if (drop.created && drop.active)
			{
				drop.update(pElapsed);
			}
			else if (_activeIndex == drop.drop_index)
			{
				_activeIndex--;
				Debug.LogError((object)("do we ever hit this??? " + _activeIndex));
			}
		}
		Bench.benchEnd("drops", "game_total", pSaveCounter: false, 0L);
	}

	public void debug(DebugTool pTool)
	{
		pTool.setText("drops total", _drops.Count.ToString() ?? "", 0f, pShowBar: false, 0L);
		pTool.setText("drops active", _activeIndex.ToString() ?? "", 0f, pShowBar: false, 0L);
	}

	public int getActiveIndex()
	{
		return _activeIndex;
	}

	private void debugString()
	{
		string text = "";
		for (int i = 0; i < _drops.Count; i++)
		{
			text = ((!_drops[i].active) ? (text + "x") : (text + "O"));
		}
		Debug.Log((object)(text + " ::: " + _activeIndex));
	}
}
