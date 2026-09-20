public class KingdomsNoItems : MetaListNoItems
{
	protected override bool hasMetas()
	{
		return base.meta_object.hasKingdoms();
	}
}
