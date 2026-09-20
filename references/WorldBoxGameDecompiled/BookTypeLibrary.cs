public class BookTypeLibrary : AssetLibrary<BookTypeAsset>
{
	public override void init()
	{
		base.init();
		add(new BookTypeAsset
		{
			id = "family_story",
			name_template = "book_name_love_story",
			color_text = "#FFA94D",
			writing_rate = 3,
			path_icons = "family_story/",
			requirement_check = (Actor a, BookTypeAsset _) => a.hasFamily() ? true : false
		});
		t.base_stats["mana"] = 10f;
		t.base_stats["happiness"] = 5f;
		t.base_stats["experience"] = 5f;
		add(new BookTypeAsset
		{
			id = "love_story",
			name_template = "book_name_love_story",
			color_text = "#FF6B6B",
			writing_rate = 3,
			path_icons = "love_story/",
			requirement_check = (Actor a, BookTypeAsset _) => a.hasFamily() ? true : false
		});
		t.base_stats["mana"] = 10f;
		t.base_stats["happiness"] = 10f;
		t.base_stats["experience"] = 5f;
		add(new BookTypeAsset
		{
			id = "friendship_story",
			name_template = "book_name_love_story",
			color_text = "#74C0FC",
			writing_rate = 3,
			path_icons = "friendship_story/",
			requirement_check = (Actor a, BookTypeAsset _) => a.hasFamily() ? true : false
		});
		t.base_stats["mana"] = 10f;
		t.base_stats["happiness"] = 5f;
		t.base_stats["experience"] = 5f;
		add(new BookTypeAsset
		{
			id = "bad_story_about_king",
			name_template = "book_name_bad_story",
			color_text = "#FFD700",
			writing_rate = 3,
			path_icons = "bad_story/",
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (a.isKing())
				{
					return false;
				}
				Actor king = a.kingdom.king;
				if (!king.hasFamily())
				{
					return false;
				}
				return king.hasClan() ? true : false;
			}
		});
		t.base_stats["happiness"] = -10f;
		t.base_stats["experience"] = 10f;
		add(new BookTypeAsset
		{
			id = "fable",
			name_template = "book_name_fable",
			color_text = "#9ACD32",
			writing_rate = 3,
			path_icons = "fable/",
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.hasWeapon())
				{
					return false;
				}
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				return a.city.hasLeader() ? true : false;
			}
		});
		t.base_stats["mana"] = 20f;
		t.base_stats["happiness"] = 10f;
		t.base_stats["experience"] = 30f;
		add(new BookTypeAsset
		{
			id = "warfare_manual",
			name_template = "book_name_warfare_manual",
			color_text = "#E8590C",
			path_icons = "warfare_manual/",
			rate_calc = (Actor a, BookTypeAsset _) => (int)a.stats["warfare"],
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				if (!a.city.hasLeader())
				{
					return false;
				}
				return (a.city.countWarriors() != 0) ? true : false;
			}
		});
		t.base_stats["warfare"] = 1f;
		t.base_stats["intelligence"] = 1f;
		t.base_stats["experience"] = 5f;
		add(new BookTypeAsset
		{
			id = "economy_manual",
			name_template = "book_name_economy_manual",
			path_icons = "economy_manual/",
			color_text = "#EAC645",
			rate_calc = (Actor a, BookTypeAsset b) => (int)a.stats["stewardship"],
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				return a.city.hasLeader() ? true : false;
			}
		});
		t.base_stats["happiness"] = -10f;
		t.base_stats["stewardship"] = 1f;
		t.base_stats["intelligence"] = 1f;
		t.base_stats["experience"] = 10f;
		add(new BookTypeAsset
		{
			id = "stewardship_manual",
			name_template = "book_name_stewardship_manual",
			color_text = "#D4A373",
			path_icons = "stewardship_manual/",
			rate_calc = (Actor a, BookTypeAsset b) => (int)a.stats["stewardship"],
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				return a.city.hasLeader() ? true : false;
			}
		});
		t.base_stats["happiness"] = -5f;
		t.base_stats["stewardship"] = 1f;
		t.base_stats["intelligence"] = 1f;
		t.base_stats["experience"] = 10f;
		add(new BookTypeAsset
		{
			id = "diplomacy_manual",
			name_template = "book_name_diplomacy_manual",
			color_text = "#66D9E8",
			path_icons = "diplomacy_manual/",
			rate_calc = (Actor a, BookTypeAsset _) => (int)a.stats["diplomacy"],
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				return a.city.hasLeader() ? true : false;
			}
		});
		t.base_stats["happiness"] = -10f;
		t.base_stats["diplomacy"] = 1f;
		t.base_stats["experience"] = 10f;
		add(new BookTypeAsset
		{
			id = "mathbook",
			name_template = "book_name_math",
			color_text = "#3BC9DB",
			path_icons = "mathbook/",
			rate_calc = (Actor a, BookTypeAsset _) => (int)a.stats["intelligence"],
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				if (!a.city.hasLeader())
				{
					return false;
				}
				return a.hasCulture() ? true : false;
			}
		});
		t.base_stats["happiness"] = -20f;
		t.base_stats["intelligence"] = 1f;
		t.base_stats["experience"] = 30f;
		add(new BookTypeAsset
		{
			id = "biology_book",
			name_template = "book_name_biology",
			path_icons = "biology_book/",
			color_text = "#80C980",
			rate_calc = (Actor a, BookTypeAsset _) => (int)a.stats["intelligence"],
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				if (!a.city.hasLeader())
				{
					return false;
				}
				return a.hasSubspecies() ? true : false;
			}
		});
		t.base_stats["happiness"] = 10f;
		t.base_stats["intelligence"] = 1f;
		t.base_stats["experience"] = 30f;
		add(new BookTypeAsset
		{
			id = "history_book",
			name_template = "book_name_history",
			path_icons = "history_book/",
			color_text = "#C49A6C",
			rate_calc = (Actor a, BookTypeAsset _) => (int)a.stats["intelligence"],
			requirement_check = delegate(Actor a, BookTypeAsset _)
			{
				if (!a.kingdom.hasKing())
				{
					return false;
				}
				if (!a.kingdom.king.hasClan())
				{
					return false;
				}
				if (!a.hasCity())
				{
					return false;
				}
				if (!a.city.hasLeader())
				{
					return false;
				}
				return a.hasSubspecies() ? true : false;
			}
		});
		t.base_stats["happiness"] = 5f;
		t.base_stats["warfare"] = 1f;
		t.base_stats["diplomacy"] = 1f;
		t.base_stats["intelligence"] = 1f;
		t.base_stats["experience"] = 30f;
	}

	public override void editorDiagnostic()
	{
		base.editorDiagnostic();
		foreach (BookTypeAsset item in list)
		{
			checkSpriteExists("path_icons", item.getFullIconPath(), item);
		}
	}

	public override void editorDiagnosticLocales()
	{
		base.editorDiagnosticLocales();
		foreach (BookTypeAsset item in list)
		{
			checkLocale(item, item.getDescriptionID());
			checkLocale(item, item.getLocaleID());
		}
	}
}
