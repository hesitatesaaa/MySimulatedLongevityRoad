using System;

[Serializable]
public delegate bool WorldAction(BaseSimObject pTarget = null, WorldTile pTile = null);
