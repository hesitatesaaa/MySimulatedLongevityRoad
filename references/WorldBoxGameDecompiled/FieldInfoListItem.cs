using System.Collections.Generic;

public class FieldInfoListItem
{
	public string field_name;

	public string field_value;

	public Dictionary<string, string> collection_data;

	public FieldInfoListItem(string pName, string pValue, Dictionary<string, string> pCollectionData = null)
	{
		field_name = pName;
		field_value = pValue;
		collection_data = pCollectionData;
	}
}
