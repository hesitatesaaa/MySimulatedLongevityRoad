using System;

[Serializable]
public enum BuildingState
{
	[Obsolete]
	None,
	Normal,
	[Obsolete]
	CivKingdom,
	[Obsolete]
	CivAbandoned,
	Ruins,
	Removed
}
