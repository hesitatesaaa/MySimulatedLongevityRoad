using System;

[Serializable]
public delegate void DropsLandedAction(Drop pDrop, WorldTile pTile = null, string pDropID = null);
