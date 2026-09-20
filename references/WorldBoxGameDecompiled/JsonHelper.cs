using Newtonsoft.Json;

public static class JsonHelper
{
	private static JsonSerializer _writer;

	private static JsonSerializer _reader;

	private static JsonSerializerSettings _settings;

	public static JsonSerializer writer
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Expected O, but got Unknown
			if (_writer == null)
			{
				_writer = JsonSerializer.Create(new JsonSerializerSettings
				{
					DefaultValueHandling = (DefaultValueHandling)3
				});
			}
			return _writer;
		}
	}

	public static JsonSerializer reader
	{
		get
		{
			if (_reader == null)
			{
				_reader = JsonSerializer.Create(read_settings);
			}
			return _reader;
		}
	}

	public static JsonSerializerSettings read_settings
	{
		get
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			if (_settings == null)
			{
				_settings = new JsonSerializerSettings();
				_settings.DefaultValueHandling = (DefaultValueHandling)3;
				_settings.Converters.Add((JsonConverter)(object)new LongJsonConverter());
				_settings.Converters.Add((JsonConverter)(object)new LongListJsonConverter());
				_settings.Converters.Add((JsonConverter)(object)new NullableLongJsonConverter());
				_settings.Converters.Add((JsonConverter)(object)new NullableLongListJsonConverter());
			}
			return _settings;
		}
	}
}
