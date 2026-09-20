using System;
using System.Collections.Generic;

public class KingdomsMetaContainer : ListMetaContainer<KingdomListElement, Kingdom, KingdomData>
{
	protected override IEnumerable<Kingdom> getMetaList()
	{
		return getMeta().getKingdoms();
	}

	protected override Comparison<Kingdom> getSorting()
	{
		return ComponentListBase<KingdomListElement, Kingdom, KingdomData, KingdomListComponent>.sortByPopulation;
	}
}
