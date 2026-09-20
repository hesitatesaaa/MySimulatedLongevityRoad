using System.Collections.Generic;

public class AvatarsCombineDataContainer
{
	private Dictionary<string, AvatarsCombineDataElement> _dict = new Dictionary<string, AvatarsCombineDataElement>();

	private List<AvatarsCombineDataElement> _list = new List<AvatarsCombineDataElement>();

	public void add(string pId, int pAmount)
	{
		AvatarsCombineDataElement avatarsCombineDataElement = new AvatarsCombineDataElement(_dict.Count + 1, pAmount);
		_dict.Add(pId, avatarsCombineDataElement);
		_list.Add(avatarsCombineDataElement);
	}

	public int getListIndex(int pIndex, string pId)
	{
		AvatarsCombineDataElement avatarsCombineDataElement = _dict[pId];
		int num = avatarsCombineDataElement.order_index - 1;
		int num2 = 1;
		for (int i = num + 1; i < _list.Count; i++)
		{
			num2 *= _list[i].total_amount;
		}
		return pIndex / num2 % avatarsCombineDataElement.total_amount;
	}

	public void clear()
	{
		_dict.Clear();
		_list.Clear();
	}

	public int totalCombinations()
	{
		int num = 1;
		for (int i = 0; i < _list.Count; i++)
		{
			num *= _list[i].total_amount;
		}
		return num;
	}
}
