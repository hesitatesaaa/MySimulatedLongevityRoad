using ai.behaviours;

public class BehActorGiveTax : BehCitizenActionCity
{
	public override BehResult execute(Actor pActor)
	{
		if (pActor.isKing())
		{
			pActor.takeAllOwnLoot();
			return BehResult.Continue;
		}
		if (!pActor.city.hasLeader())
		{
			pActor.takeAllOwnLoot();
			return BehResult.Continue;
		}
		Actor leader = pActor.city.leader;
		if (pActor.isCityLeader())
		{
			if (!pActor.kingdom.hasKing())
			{
				pActor.takeAllOwnLoot();
			}
			else
			{
				payTributeToKing(pActor, pActor.kingdom.king, pActor.kingdom.getTaxRateTribute());
			}
		}
		else
		{
			payTaxToLeader(pActor, leader, pActor.kingdom.getTaxRateLocal());
		}
		return BehResult.Continue;
	}

	private void payTributeToKing(Actor pActor, Actor pKing, float pTaxRate)
	{
		if (pActor.loot > 0)
		{
			int loot = pActor.loot;
			int num = (int)((float)loot * pTaxRate);
			int num2 = loot - num;
			int num3 = (int)((float)num2 * 0.5f);
			num2 -= num3;
			pActor.city.addResourcesToRandomStockpile("gold", num3);
			pActor.addMoney(num2);
			pKing.addLoot(num);
			pActor.paidTax(pTaxRate, "fx_money_paid_tribute");
		}
	}

	private void payTaxToLeader(Actor pActor, Actor pTarget, float pTaxRate)
	{
		if (pActor.loot > 0)
		{
			int loot = pActor.loot;
			int num = (int)((float)loot * pTaxRate);
			int pValue = loot - num;
			pActor.addMoney(pValue);
			pTarget.addLoot(num);
			pActor.paidTax(pTaxRate, "fx_money_paid_tax");
		}
	}
}
