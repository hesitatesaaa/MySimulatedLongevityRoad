namespace ai.behaviours;

public class BehaviourTaskKingdomLibrary : AssetLibrary<BehaviourTaskKingdom>
{
	public override void init()
	{
		base.init();
		BehaviourTaskKingdom obj = new BehaviourTaskKingdom
		{
			id = "nothing"
		};
		BehaviourTaskKingdom pAsset = obj;
		t = obj;
		add(pAsset);
		BehaviourTaskKingdom obj2 = new BehaviourTaskKingdom
		{
			id = "wait1"
		};
		pAsset = obj2;
		t = obj2;
		add(pAsset);
		t.addBeh(new KingdomBehRandomWait());
		BehaviourTaskKingdom obj3 = new BehaviourTaskKingdom
		{
			id = "wait_random"
		};
		pAsset = obj3;
		t = obj3;
		add(pAsset);
		t.addBeh(new KingdomBehRandomWait(0f, 5f));
		BehaviourTaskKingdom obj4 = new BehaviourTaskKingdom
		{
			id = "do_checks"
		};
		pAsset = obj4;
		t = obj4;
		add(pAsset);
		t.addBeh(new KingdomBehCheckCapital());
		t.addBeh(new KingdomBehCheckKing());
		t.addBeh(new KingdomBehRandomWait());
	}

	public override void editorDiagnosticLocales()
	{
	}
}
