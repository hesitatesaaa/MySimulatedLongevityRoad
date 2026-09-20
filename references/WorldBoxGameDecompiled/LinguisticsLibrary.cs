public class LinguisticsLibrary : AssetLibrary<LinguisticsAsset>
{
	private const string C = "C";

	private const string V = "V";

	public override void init()
	{
		addPronounce();
		addWordGroups();
		addUnique();
		addMain();
	}

	private void addPronounce()
	{
		add(new LinguisticsAsset
		{
			id = "pron_subj",
			word_group = true,
			add_space = true,
			array = AssetLibrary<LinguisticsAsset>.a<string>("pron_subj_i", "pron_subj_we", "pron_subj_you", "pron_subj_it", "pron_subj_they")
		});
		add(new LinguisticsAsset
		{
			id = "pron_obj",
			word_group = true,
			add_space = true,
			array = AssetLibrary<LinguisticsAsset>.a<string>("pron_obj_me", "pron_obj_us", "pron_obj_you", "pron_obj_it", "pron_obj_them")
		});
		add(new LinguisticsAsset
		{
			id = "pron_poss_adj",
			word_group = true,
			add_space = true,
			array = AssetLibrary<LinguisticsAsset>.a<string>("pron_poss_my", "pron_poss_our", "pron_poss_your", "pron_poss_its", "pron_poss_their")
		});
		add(new LinguisticsAsset
		{
			id = "pron_posspr",
			word_group = true,
			add_space = true,
			array = AssetLibrary<LinguisticsAsset>.a<string>("pron_poss_mine", "pron_poss_ours", "pron_poss_yours", "pron_poss_theirs")
		});
	}

	private void addWordGroups()
	{
		add(new LinguisticsAsset
		{
			id = "word_concept",
			word_group = true,
			add_space = true,
			word_type = WordType.Concept,
			array = AssetLibrary<LinguisticsAsset>.a<string>("concept_love", "concept_death", "concept_nature")
		});
		add(new LinguisticsAsset
		{
			id = "word_action",
			word_group = true,
			add_space = true,
			word_type = WordType.Action,
			array = AssetLibrary<LinguisticsAsset>.a<string>("action_run", "action_walk", "action_fight")
		});
		add(new LinguisticsAsset
		{
			id = "word_object",
			word_group = true,
			add_space = true,
			word_type = WordType.Object,
			array = AssetLibrary<LinguisticsAsset>.a<string>("object_sword", "object_shield", "object_bow")
		});
		add(new LinguisticsAsset
		{
			id = "word_place",
			word_group = true,
			add_space = true,
			word_type = WordType.Place,
			array = AssetLibrary<LinguisticsAsset>.a<string>("place_forest", "place_mountain", "place_hill")
		});
		add(new LinguisticsAsset
		{
			id = "word_creature",
			word_group = true,
			add_space = true,
			word_type = WordType.Creature,
			array = AssetLibrary<LinguisticsAsset>.a<string>("creature_king", "creature_queen", "creature_prince")
		});
	}

	private void addMain()
	{
		add(new LinguisticsAsset
		{
			id = "vowel",
			array = AssetLibrary<LinguisticsAsset>.a<string>("a", "e", "i", "o", "u", "y")
		});
		add(new LinguisticsAsset
		{
			id = "diphthongs",
			array = AssetLibrary<LinguisticsAsset>.a<string>("ai", "ei", "oi", "au", "ou", "ia", "io", "ua", "ue")
		});
		add(new LinguisticsAsset
		{
			id = "consonant",
			array = AssetLibrary<LinguisticsAsset>.a<string>("p", "b", "t", "d", "k", "g", "f", "v", "s", "z", "h", "m", "n", "l", "r", "w", "y", "j")
		});
		add(new LinguisticsAsset
		{
			id = "onset1",
			array = AssetLibrary<LinguisticsAsset>.a<string>("p", "b", "t", "d", "k", "g", "f", "v", "s", "z", "sh", "zh", "m", "n", "l", "r", "w", "y", "ch", "j")
		});
		add(new LinguisticsAsset
		{
			id = "onset2",
			array = AssetLibrary<LinguisticsAsset>.a<string>("pr", "br", "tr", "dr", "kr", "gr", "fr", "vr", "shr", "thr", "pl", "bl", "kl", "gl", "fl", "vl", "tw", "dw", "kw", "gw", "sw", "sk", "st", "sp")
		});
		add(new LinguisticsAsset
		{
			id = "coda1",
			array = AssetLibrary<LinguisticsAsset>.a<string>("n", "m", "l", "r", "s", "sh", "z")
		});
		add(new LinguisticsAsset
		{
			id = "coda2",
			array = AssetLibrary<LinguisticsAsset>.a<string>("nd", "nt", "nk", "mp", "lt", "ld", "lp", "lf", "rk", "rt", "rs", "rz", "st", "sk")
		});
		add(new LinguisticsAsset
		{
			id = "syllable_starts"
		});
		t.addPattern(50, "C", "V");
		t.addPattern(25, "C", "C", "V");
		t.addPattern(20, "C", "V", "C");
		t.addPattern(5, "V");
		add(new LinguisticsAsset
		{
			id = "syllable_mids"
		});
		t.addPattern(40, "C", "V");
		t.addPattern(30, "C", "V", "C");
		t.addPattern(10, "C", "C", "V");
		t.addPattern(20, "V");
		add(new LinguisticsAsset
		{
			id = "syllable_ends"
		});
		t.addPattern(40, "C", "V", "C");
		t.addPattern(30, "V", "C");
		t.addPattern(20, "C", "V");
		t.addPattern(10, "V");
	}

	private void addUnique()
	{
		add(new LinguisticsAsset
		{
			id = "comma",
			simple_text = ","
		});
		add(new LinguisticsAsset
		{
			id = "period",
			simple_text = ".",
			next_uppercase = true
		});
		add(new LinguisticsAsset
		{
			id = "semicolon",
			simple_text = ";",
			next_uppercase = true
		});
		add(new LinguisticsAsset
		{
			id = "colon",
			simple_text = ":"
		});
		add(new LinguisticsAsset
		{
			id = "dash",
			add_space = true,
			simple_text = "—"
		});
		add(new LinguisticsAsset
		{
			id = "hyphen",
			simple_text = "-"
		});
		add(new LinguisticsAsset
		{
			id = "ellipsis",
			simple_text = "...",
			next_uppercase = true
		});
		add(new LinguisticsAsset
		{
			id = "question_mark",
			simple_text = "?",
			next_uppercase = true
		});
		add(new LinguisticsAsset
		{
			id = "exclamation_mark",
			simple_text = "!",
			next_uppercase = true
		});
		add(new LinguisticsAsset
		{
			id = "space",
			simple_text = " "
		});
		add(new LinguisticsAsset
		{
			id = "quotation_marks",
			symbols_around = true,
			add_space = true,
			symbols_around_left = "“",
			symbols_around_right = "”"
		});
		add(new LinguisticsAsset
		{
			id = "parentheses",
			symbols_around = true,
			add_space = true,
			symbols_around_left = "(",
			symbols_around_right = ")"
		});
		add(new LinguisticsAsset
		{
			id = "brackets",
			symbols_around = true,
			add_space = true,
			symbols_around_left = "[",
			symbols_around_right = "]"
		});
		add(new LinguisticsAsset
		{
			id = "braces",
			symbols_around = true,
			add_space = true,
			symbols_around_left = "{",
			symbols_around_right = "}"
		});
		add(new LinguisticsAsset
		{
			id = "apostrophe",
			add_space = true,
			simple_text = "'"
		});
	}
}
