public class WorldTimeScaleLibrary : AssetLibrary<WorldTimeScaleAsset>
{
	public override void init()
	{
		add(new WorldTimeScaleAsset
		{
			id = "slow_mo",
			locale_key = "speed_slow_mo",
			multiplier = 0.5f,
			ticks = 1,
			conway_ticks = 1,
			path_icon = "ui/Icons/iconClockX0.5"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x1",
			locale_key = "speed_x1",
			multiplier = 1f,
			ticks = 1,
			conway_ticks = 1,
			path_icon = "ui/Icons/iconClockX1"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x2",
			locale_key = "speed_x2",
			multiplier = 2f,
			ticks = 1,
			conway_ticks = 2,
			path_icon = "ui/Icons/iconClockX2"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x3",
			locale_key = "speed_x3",
			multiplier = 3f,
			ticks = 1,
			conway_ticks = 3,
			path_icon = "ui/Icons/iconClockX3"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x4",
			locale_key = "speed_x4",
			multiplier = 4f,
			ticks = 1,
			conway_ticks = 4,
			path_icon = "ui/Icons/iconClockX4"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x5",
			locale_key = "speed_x5",
			multiplier = 5f,
			ticks = 1,
			conway_ticks = 5,
			path_icon = "ui/Icons/iconClockX5"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x10",
			locale_key = "speed_x10",
			multiplier = 10f,
			ticks = 1,
			conway_ticks = 10,
			path_icon = "ui/Icons/iconClockX5"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x15",
			locale_key = "speed_x15",
			multiplier = 15f,
			ticks = 1,
			conway_ticks = 15,
			path_icon = "ui/Icons/iconClockX5"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x20",
			locale_key = "speed_x20",
			multiplier = 20f,
			ticks = 1,
			conway_ticks = 20,
			path_icon = "ui/Icons/iconClockX5"
		});
		add(new WorldTimeScaleAsset
		{
			id = "x40",
			locale_key = "speed_x40",
			multiplier = 20f,
			sonic = true,
			ticks = 2,
			conway_ticks = 40,
			path_icon = "ui/Icons/iconClockXSonic"
		});
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (WorldTimeScaleAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
		}
	}
}
