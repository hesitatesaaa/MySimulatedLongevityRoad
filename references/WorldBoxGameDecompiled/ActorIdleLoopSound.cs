using FMOD.Studio;

public class ActorIdleLoopSound
{
	internal EventInstance fmod_instance;

	private Actor _actor;

	public ActorIdleLoopSound(ActorAsset pAsset, Actor pActor)
	{
	}

	public void stop()
	{
		stopLoopCallback(_actor);
	}

	internal void stopLoopCallback(Actor pActor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (((EventInstance)(ref fmod_instance)).isValid())
		{
			((EventInstance)(ref fmod_instance)).stop((STOP_MODE)0);
			((EventInstance)(ref fmod_instance)).release();
		}
	}
}
