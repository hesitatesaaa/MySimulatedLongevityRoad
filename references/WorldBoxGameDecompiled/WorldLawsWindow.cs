using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WorldLawsWindow : TabbedWindow
{
	[SerializeField]
	private WorldLawElement _cursed_world_button;

	[SerializeField]
	private GameObject _blackhole_butt;

	[SerializeField]
	private Image _center_blackhole;

	[SerializeField]
	private Transform _blackhole_container;

	[SerializeField]
	private Sprite _blackhole_normal;

	[SerializeField]
	private Sprite _blackhole_normal_eye;

	[SerializeField]
	private Sprite _blackhole_normal_eye_open;

	[SerializeField]
	private LayoutElement _background_star_mark_element;

	[SerializeField]
	private GameObject _description_forbidden_knowledge_1_before_sacrifice;

	[SerializeField]
	private GameObject _description_forbidden_knowledge_2_non_cursed;

	[SerializeField]
	private GameObject _description_forbidden_knowledge_3_cursed;

	[SerializeField]
	private GameObject _description_forbidden_knowledge_warn;

	protected override void create()
	{
		base.create();
		initCursedWorld();
	}

	private void initCursedWorld()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		_cursed_world_button.init(WorldLawLibrary.world_law_cursed_world);
		_cursed_world_button.addListener((UnityAction)delegate
		{
			checkShakeAndClose();
		});
	}

	private void checkShakeAndClose()
	{
		if (CursedSacrifice.justGotCursedWorld())
		{
			World.world.startShake(0.3f, 0.01f, 2f, pShakeX: true);
			checkForbiddenKnowledgeElements();
		}
		WorldLawsTextInsult.removeInsultTimeout();
		((IShakable)scroll_window).shake();
	}

	private void OnEnable()
	{
		checkForbiddenKnowledgeElements();
	}

	private void checkForbiddenKnowledgeElements()
	{
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		bool flag = WorldLawLibrary.world_law_cursed_world.isEnabled();
		bool flag2 = CursedSacrifice.isWorldReadyForCURSE();
		_description_forbidden_knowledge_3_cursed.SetActive(flag);
		if (flag)
		{
			_description_forbidden_knowledge_1_before_sacrifice.SetActive(false);
			_description_forbidden_knowledge_2_non_cursed.SetActive(false);
		}
		else
		{
			_description_forbidden_knowledge_1_before_sacrifice.SetActive(!flag2);
			_description_forbidden_knowledge_2_non_cursed.SetActive(flag2);
		}
		_description_forbidden_knowledge_warn.SetActive(flag2);
		if (flag2)
		{
			_background_star_mark_element.minHeight = 180f;
		}
		else
		{
			_background_star_mark_element.minHeight = 205f;
		}
		float num = Mathf.Lerp(0.5f, 1f, CursedSacrifice.getCurseProgressRatioForBlackhole());
		((Component)_blackhole_container).transform.localScale = new Vector3(num, num, num);
		if (flag)
		{
			_center_blackhole.sprite = _blackhole_normal_eye_open;
		}
		else if (flag2)
		{
			_center_blackhole.sprite = _blackhole_normal_eye;
		}
		else
		{
			_center_blackhole.sprite = _blackhole_normal;
		}
		((Component)_cursed_world_button).gameObject.SetActive(flag2);
		_blackhole_butt.SetActive(flag2);
	}
}
