using System;

[Serializable]
public delegate bool GetHitAction(BaseSimObject pSelf, BaseSimObject pAttackedBy = null, WorldTile pTile = null);
