internal readonly struct TemperatureMod(Actor pActor, int pNewTemperature)
{
	public readonly Actor actor = pActor;

	public readonly int new_temperature = pNewTemperature;
}
