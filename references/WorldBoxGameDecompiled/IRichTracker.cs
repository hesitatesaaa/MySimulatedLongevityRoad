public interface IRichTracker
{
	void trackViewing(string pText);

	void trackWatching();

	void trackUsing(string pPower);

	void updateUsing(int pAmount, string pPower);

	void inspectKingdom(string pKingdom);

	void inspectVillage(string pVillage);

	void inspectUnit(string pUnit);

	void spectatingUnit(string pUnit);

	void trackActivity(string pText);
}
