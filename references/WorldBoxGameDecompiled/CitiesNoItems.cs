public class CitiesNoItems : MetaListNoItems
{
	protected override bool hasMetas()
	{
		return base.meta_object.hasCities();
	}
}
