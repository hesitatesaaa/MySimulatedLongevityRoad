using System;
using UnityEngine;

public class DiplomacyRelation : CoreSystemObject<DiplomacyRelationData>
{
	public Kingdom kingdom1;

	public Kingdom kingdom2;

	internal KingdomOpinion opinion_k1;

	internal KingdomOpinion opinion_k2;

	public override BaseSystemManager manager => World.world.diplomacy;

	protected sealed override void setDefaultValues()
	{
		base.setDefaultValues();
	}

	public KingdomOpinion getOpinion(Kingdom pMain, Kingdom pTarget)
	{
		recalculate(pMain, pTarget);
		if (opinion_k1.target == pTarget)
		{
			return opinion_k1;
		}
		return opinion_k2;
	}

	private void recalculate(Kingdom k1 = null, Kingdom k2 = null)
	{
		if (opinion_k1 == null)
		{
			opinion_k1 = new KingdomOpinion(kingdom1, kingdom2);
			opinion_k2 = new KingdomOpinion(kingdom2, kingdom1);
		}
		try
		{
			opinion_k1.calculate(kingdom1, kingdom2, this);
			opinion_k2.calculate(kingdom2, kingdom1, this);
		}
		catch (Exception ex)
		{
			Debug.LogError((object)ex);
			Debug.LogError((object)data.id);
			Debug.LogError((object)data.kingdom1_id);
			Debug.LogError((object)data.kingdom2_id);
			Debug.LogError((object)("kingdom1 " + (kingdom1 == null)));
			Debug.LogError((object)("kingdom2 " + (kingdom2 == null)));
			Debug.LogError((object)(kingdom1 == k1).ToString());
			Debug.LogError((object)(kingdom2 == k2).ToString());
			Debug.LogError((object)JsonUtility.ToJson((object)kingdom1));
			Debug.LogError((object)JsonUtility.ToJson((object)kingdom2));
			Debug.LogError((object)JsonUtility.ToJson((object)k1));
			Debug.LogError((object)JsonUtility.ToJson((object)k2));
			throw ex;
		}
	}

	public override void Dispose()
	{
		opinion_k1?.Dispose();
		opinion_k2?.Dispose();
		opinion_k1 = null;
		opinion_k2 = null;
		kingdom1 = null;
		kingdom2 = null;
		base.Dispose();
	}
}
