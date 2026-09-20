using UnityEngine;

public class MusicBoxContainerTiles
{
	public int amount;

	public float percent;

	public bool enabled;

	public Vector2 cur_pan;

	private Vector2 _last_pan;

	private float _chunks;

	public MusicAsset asset;

	public void clear()
	{
		amount = 0;
		((Vector2)(ref _last_pan)).Set(-1f, -1f);
		_chunks = 0f;
	}

	public void count(int pAmount, float pWhereFromX, float pWhereFromY)
	{
		amount += pAmount;
		_chunks++;
		_last_pan.x += pWhereFromX;
		_last_pan.y += pWhereFromY;
	}

	public void calculatePan()
	{
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		_last_pan.x /= _chunks + 1f;
		_last_pan.y /= _chunks + 1f;
		if (_chunks == 0f)
		{
			((Vector2)(ref cur_pan)).Set(-1f, -1f);
		}
		else if (cur_pan.x == -1f && cur_pan.y == -1f)
		{
			((Vector2)(ref cur_pan)).Set(_last_pan.x, _last_pan.y);
		}
		else
		{
			cur_pan = Vector2.MoveTowards(cur_pan, _last_pan, 5f);
		}
	}
}
