using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class LocusElement : ChainElement, IDropHandler, IEventSystemHandler
{
	private Chromosome _chromosome;

	private LocusClickEvent _locus_click_event;

	private Action _chromosome_updated_event;

	private LocusType locus_type;

	public Image sprite_background;

	public Image effect_amplifier;

	public Image effect_locus_amplifier_bad;

	public Sprite sprite_locus_bg_normal;

	public Sprite sprite_locus_bg_synergy;

	public Sprite sprite_locus_bg_bad;

	[SerializeField]
	private LocusDot _dot_left;

	[SerializeField]
	private LocusDot _dot_right;

	[SerializeField]
	private LocusDot _dot_up;

	[SerializeField]
	private LocusDot _dot_down;

	private float _normal_size = 0.8f;

	private float _super_size = 0.8f;

	private int _locus_x;

	private int _locus_y;

	private SpriteAnimation _animation_amplifier;

	private SpriteAnimation _animation_amplifier_bad;

	[SerializeField]
	private GeneButton _gene_button;

	protected override void create()
	{
		base.create();
		is_editor_button = false;
		_animation_amplifier = ((Component)effect_amplifier).GetComponent<SpriteAnimation>();
		_animation_amplifier_bad = ((Component)effect_locus_amplifier_bad).GetComponent<SpriteAnimation>();
	}

	protected override void Update()
	{
		base.Update();
		if (isAmplifier())
		{
			if (((Behaviour)_animation_amplifier).isActiveAndEnabled)
			{
				_animation_amplifier.update(Time.deltaTime);
			}
			if (((Behaviour)_animation_amplifier_bad).isActiveAndEnabled)
			{
				_animation_amplifier_bad.update(Time.deltaTime);
			}
		}
	}

	private void click()
	{
		if (base.gene.can_drop_and_grab)
		{
			_locus_click_event(this);
			checkSprite();
			if (!InputHelpers.mouseSupported)
			{
				((Component)this).GetComponent<TipButton>().hoverAction();
			}
		}
	}

	private void clearLocus()
	{
		_locus_click_event(null);
		checkSprite();
	}

	private void checkSprite()
	{
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Expected O, but got Unknown
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		bool num = isEmptyLocus();
		bool flag = isAmplifier();
		bool active = !num && !flag;
		bool flag2 = _chromosome.isNextToBad(_locus_x, _locus_y);
		if (flag)
		{
			if (flag2)
			{
				((Component)effect_amplifier).gameObject.SetActive(false);
				((Component)effect_locus_amplifier_bad).gameObject.SetActive(true);
			}
			else
			{
				((Component)effect_amplifier).gameObject.SetActive(true);
				((Component)effect_locus_amplifier_bad).gameObject.SetActive(false);
			}
		}
		else
		{
			((Component)effect_amplifier).gameObject.SetActive(false);
			((Component)effect_locus_amplifier_bad).gameObject.SetActive(false);
		}
		if (shouldBeBadLocus())
		{
			sprite_background.sprite = sprite_locus_bg_bad;
		}
		else if (shouldBeGoldenLocus())
		{
			sprite_background.sprite = sprite_locus_bg_synergy;
		}
		else
		{
			sprite_background.sprite = sprite_locus_bg_normal;
		}
		((Component)sprite_background).gameObject.SetActive(active);
		checkChainsColors();
		if (num | flag)
		{
			((Component)_gene_button).gameObject.SetActive(false);
		}
		else
		{
			((Component)_gene_button).gameObject.SetActive(true);
			_gene_button.load(base.gene);
			_gene_button.is_editor_button = true;
			_gene_button.locusChild(new UnityAction(click), locus_index);
		}
		if (isAmplifier())
		{
			((Component)this).transform.localScale = new Vector3(_super_size, _super_size, _super_size);
		}
		else
		{
			((Component)this).transform.localScale = new Vector3(_normal_size, _normal_size, _normal_size);
		}
		((Component)this).GetComponent<TipButton>().setDefaultScale(((Component)this).transform.localScale);
	}

	private bool shouldBeBadChainSide(int pX, int pY, int pOffsetX, int pOffsetY)
	{
		return shouldBeBadChain(pX, pY, pX + pOffsetX, pY + pOffsetY);
	}

	private bool shouldBeBadChain(int pX, int pY, int pToX, int pToY)
	{
		if (base.gene.is_bad)
		{
			return true;
		}
		GeneAsset geneAt = _chromosome.getGeneAt(pToX, pToY);
		if (geneAt != null && geneAt.is_bad)
		{
			return true;
		}
		if (_chromosome.hasAmplifierBad(pX, pY))
		{
			return true;
		}
		if (_chromosome.hasAmplifierBad(pToX, pToY))
		{
			return true;
		}
		return false;
	}

	private void checkChainsColors()
	{
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		int locus_x = _locus_x;
		int locus_y = _locus_y;
		Chromosome chromosome = _chromosome;
		GeneAsset geneLeft = chromosome.getGeneLeft(locus_x, locus_y);
		GeneAsset geneRight = chromosome.getGeneRight(locus_x, locus_y);
		GeneAsset geneUp = chromosome.getGeneUp(locus_x, locus_y);
		GeneAsset geneDown = chromosome.getGeneDown(locus_x, locus_y);
		bool flag = !chromosome.hasBoundLeft(locus_x, locus_y);
		bool flag2 = !chromosome.hasBoundRight(locus_x, locus_y);
		bool flag3 = !chromosome.hasBoundUp(locus_x, locus_y);
		bool flag4 = !chromosome.hasBoundDown(locus_x, locus_y);
		bool flag5 = chromosome.hasSynergyConnectionLeft(locus_x, locus_y);
		bool flag6 = chromosome.hasSynergyConnectionRight(locus_x, locus_y);
		bool flag7 = chromosome.hasSynergyConnectionUp(locus_x, locus_y);
		bool flag8 = chromosome.hasSynergyConnectionDown(locus_x, locus_y);
		if (!flag5)
		{
			hideChain(chain_left);
		}
		else if (shouldBeBadChain(locus_x, locus_y, locus_x - 1, locus_y))
		{
			showChain(chain_left, pShow: true, base.gene.genetic_code_left, NucleobaseHelper.color_bad);
		}
		else if (chromosome.isForcedSynergyLeft(locus_x, locus_y))
		{
			showChain(chain_left, pShow: true, base.gene.genetic_code_left);
		}
		else
		{
			showChain(chain_left, pShow: true, geneLeft.genetic_code_right);
		}
		if (!flag6)
		{
			hideChain(chain_right);
		}
		else if (shouldBeBadChain(locus_x, locus_y, locus_x + 1, locus_y))
		{
			showChain(chain_right, pShow: true, base.gene.genetic_code_right, NucleobaseHelper.color_bad);
		}
		else if (chromosome.isForcedSynergyRight(locus_x, locus_y))
		{
			showChain(chain_right, pShow: true, base.gene.genetic_code_right);
		}
		else
		{
			showChain(chain_right, pShow: true, geneRight.genetic_code_left);
		}
		if (!flag7)
		{
			hideChain(chain_up);
		}
		else if (shouldBeBadChain(locus_x, locus_y, locus_x, locus_y - 1))
		{
			showChain(chain_up, pShow: true, base.gene.genetic_code_up, NucleobaseHelper.color_bad);
		}
		else if (chromosome.isForcedSynergyUp(locus_x, locus_y))
		{
			showChain(chain_up, pShow: true, base.gene.genetic_code_up);
		}
		else
		{
			showChain(chain_up, pShow: true, geneUp.genetic_code_down);
		}
		if (!flag8)
		{
			hideChain(chain_down);
		}
		else if (shouldBeBadChain(locus_x, locus_y, locus_x, locus_y + 1))
		{
			showChain(chain_down, pShow: true, base.gene.genetic_code_down, NucleobaseHelper.color_bad);
		}
		else if (chromosome.isForcedSynergyDown(locus_x, locus_y))
		{
			showChain(chain_down, pShow: true, base.gene.genetic_code_down);
		}
		else
		{
			showChain(chain_down, pShow: true, geneDown.genetic_code_up);
		}
		showDot(_dot_left, flag && !flag5, base.gene.genetic_code_left);
		showDot(_dot_right, flag2 && !flag6, base.gene.genetic_code_right);
		showDot(_dot_up, flag3 && !flag7, base.gene.genetic_code_up);
		showDot(_dot_down, flag4 && !flag8, base.gene.genetic_code_down);
	}

	public override void load(GeneAsset pAsset)
	{
		throw new NotImplementedException("Use show instead");
	}

	internal override void load(string pElementID)
	{
		throw new NotImplementedException("Use show instead");
	}

	public void show(int pLocusIndex, Chromosome pChromosome, GeneAsset pGene, LocusType pLocusType, LocusClickEvent pLocusClickEvent)
	{
		base.load(pGene);
		clearActions();
		_chromosome = pChromosome;
		locus_index = pLocusIndex;
		(int, int) xYFromIndex = _chromosome.getXYFromIndex(pLocusIndex);
		int item = xYFromIndex.Item1;
		int item2 = xYFromIndex.Item2;
		_locus_x = item;
		_locus_y = item2;
		_locus_click_event = pLocusClickEvent;
		locus_type = pLocusType;
		((Object)((Component)this).gameObject).name = "Locus " + base.gene.id;
		colorChains();
		checkSprite();
	}

	protected override void clearActions()
	{
		base.clearActions();
		_chromosome_updated_event = null;
	}

	public bool shouldBeBadLocus()
	{
		bool is_bad = base.gene.is_bad;
		bool flag = _chromosome.isNextToBad(_locus_x, _locus_y);
		return is_bad | flag;
	}

	public bool shouldBeGoldenLocus()
	{
		if (isAmplifier())
		{
			return true;
		}
		if (base.gene.synergy_sides_always)
		{
			return true;
		}
		if (_chromosome.hasFullSynergy(locus_index))
		{
			return true;
		}
		return false;
	}

	public bool isAmplifier()
	{
		return locus_type == LocusType.Amplifier;
	}

	public bool isAmplifierBad()
	{
		return _chromosome.hasAmplifierBad(_locus_x, _locus_y);
	}

	public bool isEmptyLocus()
	{
		return locus_type == LocusType.Empty;
	}

	protected override void fillTooltipData(GeneAsset pElement)
	{
		Tooltip.show(this, "gene", tooltipDataBuilder());
	}

	protected override TooltipData tooltipDataBuilder()
	{
		return new TooltipData
		{
			gene = base.gene,
			locus = this,
			chromosome = _chromosome
		};
	}

	public bool canAddGene()
	{
		return _chromosome.canAddToLocus(locus_index);
	}

	public bool isSpecialLocus()
	{
		return _chromosome.isSpecialLocus(locus_index);
	}

	public void OnDrop(PointerEventData pEventData)
	{
		if ((Object)(object)pEventData.pointerDrag == (Object)null || isAmplifier())
		{
			return;
		}
		if (!Config.hasPremium)
		{
			ScrollWindow.showWindow("premium_menu");
			return;
		}
		GeneButton component = pEventData.pointerDrag.GetComponent<GeneButton>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		GeneAsset elementAsset = component.getElementAsset();
		if (elementAsset.can_drop_and_grab)
		{
			if (component.locus_index > -1)
			{
				GeneAsset pAsset = _chromosome.getGene(locus_index);
				_chromosome.setGene(pAsset, component.locus_index);
			}
			GeneAsset geneAsset = getGeneAsset();
			_chromosome.setGene(elementAsset, locus_index);
			_chromosome_updated_event();
			SelectedMetas.selected_subspecies.eventGMO();
			if (elementAsset != geneAsset)
			{
				AchievementLibrary.engineered_evolution.check();
			}
			fillTooltipData(base.gene);
		}
	}

	public void addChromosomeUpdatedEvent(Action pChromosomeUpdatedEvent)
	{
		_chromosome_updated_event = pChromosomeUpdatedEvent;
	}

	protected void showDot(LocusDot pChainDot, bool pShow, char pGeneticCode)
	{
		((Component)pChainDot).gameObject.SetActive(pShow);
		if (pShow)
		{
			pChainDot.colorDot(pGeneticCode);
		}
	}

	protected override void startSignal()
	{
		AchievementLibrary.genes_explorer.checkBySignal();
	}

	protected override bool unlockElement()
	{
		bool result = base.unlockElement();
		isElementUnlocked();
		return result;
	}
}
