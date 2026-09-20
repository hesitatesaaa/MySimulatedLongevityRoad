public class SimGlobals : AssetLibrary<SimGlobalAsset>
{
	public static SimGlobalAsset m;

	public override void init()
	{
		base.init();
		m = add(new SimGlobalAsset
		{
			id = "main"
		});
	}
}
