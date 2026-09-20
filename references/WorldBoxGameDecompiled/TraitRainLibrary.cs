using System;

[Serializable]
public class TraitRainLibrary : AssetLibrary<TraitRainAsset>
{
	public override void init()
	{
		base.init();
		add(new TraitRainAsset
		{
			id = "traits_delta_rain_edit"
		});
		t.get_list = () => PlayerConfig.instance.data.trait_editor_delta;
		t.get_state = () => PlayerConfig.instance.data.trait_editor_delta_state;
		t.set_state = delegate(RainState pState)
		{
			PlayerConfig.instance.data.trait_editor_delta_state = pState;
		};
		t.path_art = "ui/illustrations/art_trait_rain_delta";
		t.path_art_void = "ui/illustrations/art_trait_rain_delta_void";
		add(new TraitRainAsset
		{
			id = "traits_gamma_rain_edit"
		});
		t.get_list = () => PlayerConfig.instance.data.trait_editor_gamma;
		t.get_state = () => PlayerConfig.instance.data.trait_editor_gamma_state;
		t.set_state = delegate(RainState pState)
		{
			PlayerConfig.instance.data.trait_editor_gamma_state = pState;
		};
		t.path_art = "ui/illustrations/art_trait_rain_gamma";
		t.path_art_void = "ui/illustrations/art_trait_rain_gamma_void";
		add(new TraitRainAsset
		{
			id = "traits_omega_rain_edit"
		});
		t.get_list = () => PlayerConfig.instance.data.trait_editor_omega;
		t.get_state = () => PlayerConfig.instance.data.trait_editor_omega_state;
		t.set_state = delegate(RainState pState)
		{
			PlayerConfig.instance.data.trait_editor_omega_state = pState;
		};
		t.path_art = "ui/illustrations/art_trait_rain_omega";
		t.path_art_void = "ui/illustrations/art_trait_rain_omega_void";
	}
}
