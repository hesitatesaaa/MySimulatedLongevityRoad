using System.Collections.Generic;

public class MonthLibrary : AssetLibrary<MonthAsset>
{
	private Dictionary<int, MonthAsset> _month_indexes;

	public override void init()
	{
		base.init();
		add(new MonthAsset
		{
			id = "month_1",
			english_name = "january",
			month_index = 1
		});
		add(new MonthAsset
		{
			id = "month_2",
			english_name = "february",
			month_index = 2
		});
		add(new MonthAsset
		{
			id = "month_3",
			english_name = "march",
			month_index = 3
		});
		add(new MonthAsset
		{
			id = "month_4",
			english_name = "april",
			month_index = 4
		});
		add(new MonthAsset
		{
			id = "month_5",
			english_name = "may",
			month_index = 5
		});
		add(new MonthAsset
		{
			id = "month_6",
			english_name = "june",
			month_index = 6
		});
		add(new MonthAsset
		{
			id = "month_7",
			english_name = "july",
			month_index = 7
		});
		add(new MonthAsset
		{
			id = "month_8",
			english_name = "august",
			month_index = 8
		});
		add(new MonthAsset
		{
			id = "month_9",
			english_name = "september",
			month_index = 9
		});
		add(new MonthAsset
		{
			id = "month_10",
			english_name = "october",
			month_index = 10
		});
		add(new MonthAsset
		{
			id = "month_11",
			english_name = "november",
			month_index = 11
		});
		add(new MonthAsset
		{
			id = "month_12",
			english_name = "december",
			month_index = 12
		});
	}

	public override void linkAssets()
	{
		base.linkAssets();
		_month_indexes = new Dictionary<int, MonthAsset>();
		foreach (MonthAsset item in list)
		{
			_month_indexes.Add(item.month_index, item);
		}
	}

	public MonthAsset getMonth(int pMonthIndex)
	{
		return _month_indexes[pMonthIndex];
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (MonthAsset item in list)
		{
			checkLocale(item, item.getLocaleID());
			checkLocale(item, item.getDescriptionID());
		}
	}
}
