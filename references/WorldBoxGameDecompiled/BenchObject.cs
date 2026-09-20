public class BenchObject
{
	public int derp;

	public void update(float pElapsed)
	{
		updateMove(pElapsed);
		updateMove(pElapsed);
		updateMove(pElapsed);
		updateMove(pElapsed);
		updateMove(pElapsed);
	}

	public void updateMove(float pElapsed)
	{
		derp += 22;
		if (derp == 1000)
		{
			derp += 10;
			if (derp < 10)
			{
				derp += 5;
			}
			else
			{
				derp -= 5;
			}
		}
	}
}
