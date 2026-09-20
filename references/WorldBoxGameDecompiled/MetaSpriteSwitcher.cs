public class MetaSpriteSwitcher : SpriteSwitcher
{
	public MetaType meta_type;

	protected override bool hasAny()
	{
		return AssetManager.meta_type_library.getAsset(meta_type).has_any();
	}
}
