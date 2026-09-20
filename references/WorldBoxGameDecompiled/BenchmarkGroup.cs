using System.Collections.Generic;

public class BenchmarkGroup
{
	public string id;

	public Dictionary<string, ToolBenchmarkData> dict_data = new Dictionary<string, ToolBenchmarkData>();

	public void flatten()
	{
		foreach (ToolBenchmarkData value in dict_data.Values)
		{
			value.end(0.0);
		}
	}
}
