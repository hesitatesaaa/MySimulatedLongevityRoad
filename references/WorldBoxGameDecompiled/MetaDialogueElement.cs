public class MetaDialogueElement : MetaNeedsElementBase
{
	protected override string getText(IMetaObject pMeta, out Actor pActorResult)
	{
		pActorResult = null;
		return MetaTextReportHelper.addSingleUnitTextRandomUnit(pMeta, out pActorResult);
	}
}
