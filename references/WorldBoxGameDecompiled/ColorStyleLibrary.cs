public class ColorStyleLibrary : AssetLibrary<ColorStyleAsset>
{
	public static ColorStyleAsset m;

	public override void init()
	{
		base.init();
		m = add(new ColorStyleAsset
		{
			id = "main"
		});
	}
}
