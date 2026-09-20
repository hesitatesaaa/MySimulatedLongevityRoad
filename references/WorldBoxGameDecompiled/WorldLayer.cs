public class WorldLayer : MapLayer
{
	public override void update(float pElapsed)
	{
	}

	public override void draw(float pElapsed)
	{
		UpdateDirty(pElapsed);
	}
}
