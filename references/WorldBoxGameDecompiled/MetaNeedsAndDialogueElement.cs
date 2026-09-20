public class MetaNeedsAndDialogueElement : MetaNeedsElementBase
{
	protected override string getText(IMetaObject pMeta, out Actor pActorResult)
	{
		pActorResult = null;
		if (pMeta.countUnits() < 5)
		{
			return string.Empty;
		}
		return MetaTextReportHelper.getText(pMeta, pMeta.getMetaTypeAsset(), out pActorResult);
	}
}
