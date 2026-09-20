using UnityEngine;

public class SpawnEffect : BaseEffect
{
	private string _event;

	private SpriteAnimation _animation;

	private bool _eventUsed;

	private WorldTile _tile;

	internal override void create()
	{
		base.create();
		_animation = ((Component)this).GetComponent<SpriteAnimation>();
	}

	internal override void spawnOnTile(WorldTile pTile)
	{
		prepare(pTile);
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		if (!_eventUsed && _animation.currentFrameIndex == 14)
		{
			_eventUsed = true;
			if (_event == "crabzilla")
			{
				GodPower godPower = AssetManager.powers.get("crabzilla");
				World.world.units.createNewUnit(godPower.actor_asset_id, _tile, pMiracleSpawn: false, godPower.actor_spawn_height);
			}
		}
	}

	public void setEvent(string pEvent, WorldTile pTile)
	{
		_tile = pTile;
		_eventUsed = false;
		_event = pEvent;
	}
}
