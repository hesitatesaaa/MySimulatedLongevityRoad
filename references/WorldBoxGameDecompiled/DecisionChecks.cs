public readonly ref struct DecisionChecks
{
	public readonly bool is_hungry;

	public readonly bool is_fighting;

	public readonly bool is_herd;

	public readonly bool is_adult;

	public readonly bool is_civ;

	public readonly bool is_sapient;

	public readonly bool city_is_in_danger;

	public readonly bool can_capture_city;

	public DecisionChecks(bool pIsHungry, bool pIsFighting, bool pIsHerd, bool pIsAdult, bool pIsCiv, bool pIsSapient, bool pCityIsInDanger, bool pCanCaptureCity)
	{
		is_hungry = pIsHungry;
		is_fighting = pIsFighting;
		is_herd = pIsHerd;
		is_adult = pIsAdult;
		is_civ = pIsCiv;
		is_sapient = pIsSapient;
		city_is_in_danger = pCityIsInDanger;
		can_capture_city = pCanCaptureCity;
	}

	public DecisionChecks(Actor pActor)
	{
		is_hungry = pActor.isHungry();
		is_fighting = pActor.isFighting();
		is_herd = pActor.asset.follow_herd;
		is_adult = pActor.isAdult();
		is_sapient = pActor.isSapient();
		is_civ = pActor.isKingdomCiv();
		city_is_in_danger = pActor.inOwnCityBorders() && pActor.city.isInDanger();
		can_capture_city = pActor.profession_asset?.can_capture ?? false;
	}
}
