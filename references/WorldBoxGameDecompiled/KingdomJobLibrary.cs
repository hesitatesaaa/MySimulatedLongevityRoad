public class KingdomJobLibrary : AssetLibrary<KingdomJob>
{
	public override void init()
	{
		base.init();
		add(new KingdomJob
		{
			id = "kingdom"
		});
		t.addTask("do_checks");
		t.addTask("wait1");
		t.addTask("wait1");
	}
}
