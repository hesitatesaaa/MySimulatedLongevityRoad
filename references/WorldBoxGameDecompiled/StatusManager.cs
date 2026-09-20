using System.Collections.Generic;

public class StatusManager : CoreSystemManager<Status, StatusData>
{
	public StatusManager()
	{
		type_id = "statuses";
	}

	public Status newStatus(BaseSimObject pSimObject, StatusAsset pAsset, float pOverrideTimer)
	{
		Status status = newObject();
		status.start(pSimObject, pAsset);
		if (pOverrideTimer > 0f)
		{
			status.setDuration(pOverrideTimer);
		}
		return status;
	}

	public override void update(float pElapsed)
	{
		base.update(pElapsed);
		updateStatuses(pElapsed);
		checkDead();
	}

	public override void removeObject(Status pObject)
	{
		base.removeObject(pObject);
		pObject.sim_object.removeFinishedStatusEffect(pObject);
	}

	private void updateStatuses(float pElapsed)
	{
		float pWorldTime = (float)World.world.getCurWorldTime();
		bool flag = World.world.isPaused();
		List<Status> list = base.list;
		for (int i = 0; i < list.Count; i++)
		{
			Status status = list[i];
			if (!status.is_finished)
			{
				if (!flag)
				{
					status.update(pElapsed, pWorldTime);
				}
				if (!flag || status.asset.is_animated_in_pause)
				{
					status.updateAnimationFrame(pElapsed);
				}
			}
		}
	}

	private void checkDead()
	{
		for (int num = list.Count - 1; num >= 0; num--)
		{
			Status status = list[num];
			if (status.is_finished)
			{
				removeObject(status);
			}
		}
	}
}
