public class AllianceSelectedKingdomsContainer : AllianceKingdomsContainer
{
	protected override void OnEnable()
	{
	}

	public void update(Alliance pAlliance)
	{
		meta_object = pAlliance;
		clear();
		using ListPool<Kingdom> listPool = new ListPool<Kingdom>(base.alliance.kingdoms_hashset);
		track_objects.AddRange(listPool);
		foreach (ref Kingdom item in listPool)
		{
			Kingdom current = item;
			showBanner(current);
		}
	}
}
