using System.IO;
using Newtonsoft.Json;

public class CustomJsonArrayWriter : JsonTextWriter
{
	public CustomJsonArrayWriter(TextWriter writer)
		: base(writer)
	{
	}

	protected override void WriteIndent()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Invalid comparison between Unknown and I4
		if ((int)((JsonWriter)this).WriteState != 3)
		{
			((JsonTextWriter)this).WriteIndent();
		}
		else
		{
			((JsonWriter)this).WriteIndentSpace();
		}
	}
}
