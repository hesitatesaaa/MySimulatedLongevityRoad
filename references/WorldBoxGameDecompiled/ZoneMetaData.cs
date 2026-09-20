public struct ZoneMetaData
{
	public double timestamp;

	public double timestamp_new;

	public IMetaObject meta_object;

	public int previous_priority_amount;

	public TileZone zone;

	public float getDiffTime()
	{
		return getDiffTime(World.world.getCurWorldTime());
	}

	public float getDiffTime(double pWorldTime)
	{
		return (float)(pWorldTime - timestamp);
	}
}
