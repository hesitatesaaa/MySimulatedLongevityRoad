using UnityEngine.UI;

public interface ISapientListComponent
{
	void setSapientCounter(Text pCounter);

	void setNonSapientCounter(Text pCounter);

	void setShowSapientOnly();

	void setShowNonSapientOnly();

	void setDefault();
}
