public class DeadKingdom : Kingdom
{
	public override void loadData(KingdomData pData)
	{
		setData(pData);
		data.load();
		ActorAsset actorAsset = getActorAsset();
		asset = AssetManager.kingdoms.get(actorAsset.kingdom_id_civilization);
	}

	public override int getAge()
	{
		int year = Date.getYear(data.created_time);
		return Date.getYear(data.died_time) - year;
	}

	public override string getMotto()
	{
		return data.motto;
	}
}
