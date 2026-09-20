using FMOD.Studio;

public class DebugMusicBoxData
{
	public const float INTERVAL = 3f;

	public float timer = 3f;

	public string path;

	public float x;

	public float y;

	public EventInstance instance;

	public bool isPlaying()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		PLAYBACK_STATE val = default(PLAYBACK_STATE);
		((EventInstance)(ref instance)).getPlaybackState(ref val);
		return (int)val == 0;
	}
}
