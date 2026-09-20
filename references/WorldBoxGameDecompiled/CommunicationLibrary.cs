public class CommunicationLibrary : AssetLibrary<CommunicationAsset>
{
	public static CommunicationAsset normal;

	public override void init()
	{
		base.init();
		normal = add(new CommunicationAsset
		{
			id = "normal",
			icon_path = "speech/speech_bubble",
			show_topic = true
		});
		add(new CommunicationAsset
		{
			id = "singing",
			icon_path = "speech/speech_02"
		});
		add(new CommunicationAsset
		{
			id = "exclamation",
			icon_path = "speech/speech_03"
		});
		add(new CommunicationAsset
		{
			id = "questioning",
			icon_path = "speech/speech_04"
		});
		add(new CommunicationAsset
		{
			id = "offensive",
			icon_path = "speech/speech_05"
		});
		add(new CommunicationAsset
		{
			id = "defensive",
			icon_path = "speech/speech_06"
		});
		add(new CommunicationAsset
		{
			id = "mortality",
			icon_path = "speech/speech_07"
		});
		add(new CommunicationAsset
		{
			id = "romantic",
			icon_path = "speech/speech_08"
		});
		add(new CommunicationAsset
		{
			id = "pleasant",
			icon_path = "speech/speech_09"
		});
		add(new CommunicationAsset
		{
			id = "unpleasant",
			icon_path = "speech/speech_10"
		});
	}
}
