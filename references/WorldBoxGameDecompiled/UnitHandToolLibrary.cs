public class UnitHandToolLibrary : AssetLibrary<UnitHandToolAsset>
{
	public override void init()
	{
		base.init();
		add(new UnitHandToolAsset
		{
			id = "flag",
			animated = true,
			colored = true
		});
		add(new UnitHandToolAsset
		{
			id = "axe"
		});
		add(new UnitHandToolAsset
		{
			id = "basket"
		});
		add(new UnitHandToolAsset
		{
			id = "book"
		});
		add(new UnitHandToolAsset
		{
			id = "bucket"
		});
		add(new UnitHandToolAsset
		{
			id = "coffee_cup",
			animated = true
		});
		add(new UnitHandToolAsset
		{
			id = "hammer"
		});
		add(new UnitHandToolAsset
		{
			id = "hoe"
		});
		add(new UnitHandToolAsset
		{
			id = "notebook"
		});
		add(new UnitHandToolAsset
		{
			id = "pickaxe"
		});
	}

	public override void post_init()
	{
		base.post_init();
		foreach (UnitHandToolAsset item in list)
		{
			if (string.IsNullOrEmpty(item.path_gameplay_sprite))
			{
				item.path_gameplay_sprite = "items/tools/tool_" + item.id;
			}
		}
	}

	public void loadSprites()
	{
		foreach (UnitHandToolAsset item in list)
		{
			item.gameplay_sprites = SpriteTextureLoader.getSpriteList(item.path_gameplay_sprite);
		}
	}
}
