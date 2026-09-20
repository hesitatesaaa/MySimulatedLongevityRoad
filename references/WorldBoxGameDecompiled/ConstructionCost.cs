using System;
using Beebyte.Obfuscator;

[Serializable]
[ObfuscateLiterals]
public class ConstructionCost
{
	public int wood;

	public int stone;

	public int common_metals;

	public int gold;

	public ConstructionCost(int pWood = 0, int pStone = 0, int pCommonMetals = 0, int pGold = 0)
	{
		wood = pWood;
		stone = pStone;
		common_metals = pCommonMetals;
		gold = pGold;
	}
}
