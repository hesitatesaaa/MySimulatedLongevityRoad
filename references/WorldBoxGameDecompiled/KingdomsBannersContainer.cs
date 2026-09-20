using System.Collections.Generic;

public class KingdomsBannersContainer : BannersMetaContainer<KingdomBanner, Kingdom, KingdomData>
{
	protected override IEnumerable<Kingdom> getMetaList(IMetaObject pMeta)
	{
		return pMeta.getKingdoms();
	}
}
