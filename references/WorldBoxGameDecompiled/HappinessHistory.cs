public struct HappinessHistory
{
	public int index;

	public double timestamp;

	public int bonus;

	public HappinessAsset asset => AssetManager.happiness_library.list[index];

	public string getAgoString()
	{
		return Date.getAgoString(timestamp);
	}

	public double elapsedSince()
	{
		return World.world.getWorldTimeElapsedSince(timestamp);
	}
}
