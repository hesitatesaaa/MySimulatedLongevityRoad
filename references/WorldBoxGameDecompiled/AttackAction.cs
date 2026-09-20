using System;

[Serializable]
public delegate bool AttackAction(BaseSimObject pSelf, BaseSimObject pTarget, WorldTile pTile = null);
