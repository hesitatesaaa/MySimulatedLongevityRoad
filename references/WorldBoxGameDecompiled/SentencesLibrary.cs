public class SentencesLibrary : AssetLibrary<SentenceAsset>
{
	public override void init()
	{
		add(new SentenceAsset
		{
			id = "declarative"
		});
		t.addTemplate("pron_obj", "word_concept", "comma", "word_concept", "word_action", "word_concept", "word_creature");
		t.addTemplate("pron_obj", "word_concept", "pron_poss_adj", "word_place");
		t.addTemplate("pron_obj", "word_concept", "pron_poss_adj", "word_place");
		add(new SentenceAsset
		{
			id = "interrogative"
		});
		add(new SentenceAsset
		{
			id = "imperative"
		});
		add(new SentenceAsset
		{
			id = "exclamatory"
		});
	}
}
