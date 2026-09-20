using UnityEngine;

public interface IStatsElement : IRefreshElement
{
	GameObject gameObject { get; }

	void setIconValue(string pName, float pMainVal, float? pMax = null, string pColor = "", bool pFloat = false, string pEnding = "", char pSeparator = '/');
}
