using System.Collections.Generic;
using UnityEngine;

public class EffectDragParticlesManager : MonoBehaviour
{
	public static EffectDragParticlesManager instance;

	private ObjectPoolGenericMono<EffectParticlesCursor> _pool;

	[SerializeField]
	private EffectParticlesCursor _prefab;

	[SerializeField]
	private List<SpriteSet> _sprite_sets;

	[SerializeField]
	public float _spawn_interval = 10f;

	private void Awake()
	{
		_pool = new ObjectPoolGenericMono<EffectParticlesCursor>(_prefab, ((Component)this).transform);
		instance = this;
	}

	private void Update()
	{
		updateSpawn();
		updateAnimation();
	}

	private void updateAnimation()
	{
		if (_pool.countActive() == 0)
		{
			return;
		}
		foreach (EffectParticlesCursor item in _pool.getListTotal())
		{
			if (((Behaviour)item).isActiveAndEnabled)
			{
				item.update();
			}
		}
	}

	private void updateSpawn()
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (Config.isDraggingItem() && Config.dragging_item_object != null && Config.dragging_item_object.spawn_particles_on_drag && (float)Time.frameCount % _spawn_interval == 0f)
		{
			spawnNew(Config.dragging_item_object.transform.position);
		}
	}

	public void spawnNew(Vector3 pPos)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		EffectParticlesCursor next = _pool.getNext();
		next.setFrames(_sprite_sets.GetRandom().sprites);
		next.launch();
		next.getAnimation().setActionFinish(finishingEffectAction);
		Vector2 val = Vector2.op_Implicit(pPos);
		val.x += Randy.randomFloat(-1f, 1f);
		val.y += Randy.randomFloat(-1f, 1f);
		((Component)next).transform.position = Vector2.op_Implicit(val);
	}

	private void finishingEffectAction(MonoBehaviour pEffectObject)
	{
		_pool.release(((Component)pEffectObject).GetComponent<EffectParticlesCursor>());
	}
}
