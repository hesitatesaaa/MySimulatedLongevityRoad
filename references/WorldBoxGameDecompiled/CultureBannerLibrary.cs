using System.Collections.Generic;

public class CultureBannerLibrary : GenericBannerLibrary
{
	public override void init()
	{
		base.init();
		main = add(new BannerAsset
		{
			id = "main",
			icons = new List<string> { "cultures/culture_element_0", "cultures/culture_element_1", "cultures/culture_element_2", "cultures/culture_element_3", "cultures/culture_element_4", "cultures/culture_element_5", "cultures/culture_element_6", "cultures/culture_element_7" },
			backgrounds = new List<string> { "cultures/culture_decor_0", "cultures/culture_decor_1", "cultures/culture_decor_2", "cultures/culture_decor_3", "cultures/culture_decor_4", "cultures/culture_decor_5", "cultures/culture_decor_6", "cultures/culture_decor_7", "cultures/culture_decor_8" }
		});
	}
}
