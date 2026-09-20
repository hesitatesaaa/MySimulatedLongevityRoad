public class MusicBoxContainerCivs
{
	public MusicAsset asset;

	public int buildings;

	public bool kingdom_exists;

	public bool active;

	public void clear()
	{
		buildings = 0;
		kingdom_exists = false;
		active = false;
	}
}
