using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneEditor : MonoBehaviour
{
	[SerializeField]
	private Text _text_unlocked_genes;

	[SerializeField]
	private Transform _transform_chromosomes;

	[SerializeField]
	private Transform _transform_loci;

	[SerializeField]
	private Transform _transform_gene_selector;

	[SerializeField]
	private ChromosomeElement _prefab_chromosome_element;

	[SerializeField]
	private LocusElement _prefab_locus_element;

	[SerializeField]
	private GeneButton _prefab_gene_button;

	private bool _initialized;

	private Dictionary<GeneAsset, GeneButton> _dictionary_gene_buttons = new Dictionary<GeneAsset, GeneButton>();

	private ObjectPoolGenericMono<ChromosomeElement> _pool_elements_chromosomes;

	private ObjectPoolGenericMono<LocusElement> _pool_elements_loci;

	private LocusElement _selected_locus;

	private Chromosome _selected_chromosome;

	public Image selection_locus;

	public Image selection_gene_asset;

	public Text genome_counter_text;

	private SubspeciesWindow _window_subspecies;

	private Subspecies _meta_object => SelectedMetas.selected_subspecies;

	internal void load()
	{
		init();
		clear();
		loadChromosomes();
		reloadButtons();
		recolorGenePoolButtons();
	}

	private void init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_window_subspecies = ((Component)this).GetComponentInParent<SubspeciesWindow>();
			_pool_elements_chromosomes = new ObjectPoolGenericMono<ChromosomeElement>(_prefab_chromosome_element, _transform_chromosomes);
			_pool_elements_loci = new ObjectPoolGenericMono<LocusElement>(_prefab_locus_element, _transform_loci);
			loadGeneButtons();
		}
	}

	private void clear()
	{
		_pool_elements_chromosomes.clear();
		_pool_elements_loci.clear();
		_selected_chromosome = null;
		_selected_locus = null;
	}

	private void OnEnable()
	{
		load();
	}

	private void OnDisable()
	{
		clear();
	}

	public void debugRandomizeGenes()
	{
		_meta_object.addDNAMutationToSeed();
		_meta_object.generateNucleus();
		_meta_object.genesChangedEvent();
		_meta_object.eventGMO();
		load();
	}

	public void debugShuffleGenes()
	{
		_meta_object.unstableGenomeEvent();
		load();
	}

	private void loadChromosomes(bool pSelectFirstChromosome = true)
	{
		foreach (Chromosome chromosome in _meta_object.nucleus.chromosomes)
		{
			_pool_elements_chromosomes.getNext().show(chromosome, clickChromosome);
		}
		if (pSelectFirstChromosome && _meta_object.nucleus.chromosomes.Count > 0)
		{
			clickChromosome(_meta_object.nucleus.chromosomes[0]);
		}
	}

	private void recolorGenePoolButtons()
	{
		foreach (GeneButton value in _dictionary_gene_buttons.Values)
		{
			value.colorChains();
		}
	}

	private void loadGeneButtons()
	{
		foreach (GeneAsset item in AssetManager.gene_library.list)
		{
			if (!item.is_empty)
			{
				GeneButton geneButton = Object.Instantiate<GeneButton>(_prefab_gene_button, _transform_gene_selector);
				_dictionary_gene_buttons.Add(item, geneButton);
				geneButton.load(item);
				geneButton.is_editor_button = true;
				geneButton.addElementUnlockedAction(reloadButtons);
				geneButton.addGeneClickCallback(clickGeneAssetAction);
				((Behaviour)((Component)geneButton).GetComponent<DraggableLayoutElement>()).enabled = item.isAvailable();
			}
		}
	}

	public void clickChromosome(Chromosome pChromosome)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		foreach (ChromosomeElement item in _pool_elements_chromosomes.getListTotal())
		{
			if (((Component)item).gameObject.activeSelf)
			{
				if (item.chromosome == pChromosome)
				{
					((Graphic)item.image).color = Color.white;
				}
				else
				{
					((Graphic)item.image).color = Color.gray;
				}
			}
		}
		_selected_chromosome = pChromosome;
		showGenes(pChromosome);
		selectFirstNormalLocus();
	}

	private void selectFirstNormalLocus()
	{
		foreach (LocusElement item in _pool_elements_loci.getListTotal())
		{
			if (!item.isSpecialLocus())
			{
				selectLocus(item);
				break;
			}
		}
	}

	internal void selectLocus(LocusElement pElement)
	{
		_selected_locus = pElement;
	}

	private void clickGeneAssetAction(GeneAsset pGeneAsset)
	{
		if (!((Object)(object)_selected_locus == (Object)null) && pGeneAsset.isAvailable())
		{
			if (pGeneAsset != _selected_locus.getGeneAsset())
			{
				AchievementLibrary.engineered_evolution.check();
			}
			if (!Config.hasPremium)
			{
				ScrollWindow.showWindow("premium_menu");
				return;
			}
			_selected_chromosome.setGene(pGeneAsset, _selected_locus.locus_index);
			chromosomeUpdatedEvent();
		}
	}

	private void chromosomeUpdatedEvent()
	{
		_selected_chromosome.setDirty();
		_selected_chromosome.recalculate();
		_meta_object.genesChangedEvent();
		_meta_object.eventGMO();
		showGenes(_selected_chromosome);
		AchievementLibrary.simple_stupid_genetics.check();
		AchievementLibrary.fast_living.check();
		AchievementLibrary.long_living.check();
		AchievementLibrary.master_weaver.check();
		_pool_elements_chromosomes.clear();
		loadChromosomes(pSelectFirstChromosome: false);
	}

	public void showGenes(Chromosome pChromosome)
	{
		_pool_elements_loci.clear();
		for (int i = 0; i < pChromosome.genes.Count; i++)
		{
			GeneAsset pGene = pChromosome.genes[i];
			LocusElement next = _pool_elements_loci.getNext();
			next.show(i, pChromosome, pGene, pChromosome.getLocusType(i), selectLocus);
			next.addElementUnlockedAction(reloadButtons);
			next.addChromosomeUpdatedEvent(chromosomeUpdatedEvent);
		}
		_window_subspecies.updateStats();
	}

	private void updateTextGenome()
	{
		int num = _selected_chromosome.countNonEmpty();
		int amount_loci = _selected_chromosome.getAsset().amount_loci;
		genome_counter_text.text = num + " / " + amount_loci;
	}

	private void Update()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (_meta_object == null || _selected_chromosome == null)
		{
			return;
		}
		((Component)selection_gene_asset).gameObject.SetActive((Object)(object)_selected_locus != (Object)null);
		((Component)selection_locus).gameObject.SetActive((Object)(object)_selected_locus != (Object)null);
		if ((Object)(object)_selected_locus != (Object)null)
		{
			((Component)selection_locus).gameObject.transform.position = ((Component)_selected_locus).transform.position;
			GeneButton currentGeneAssetButton = getCurrentGeneAssetButton();
			((Component)selection_gene_asset).gameObject.transform.position = ((Component)currentGeneAssetButton).transform.position;
			if (!Config.isDraggingItem())
			{
				_ = (Object)(object)currentGeneAssetButton != (Object)null;
			}
		}
	}

	private GeneButton getCurrentGeneAssetButton()
	{
		GeneAsset geneAsset = _selected_locus.getGeneAsset();
		if (geneAsset == null)
		{
			return null;
		}
		if (_dictionary_gene_buttons.ContainsKey(geneAsset))
		{
			return _dictionary_gene_buttons[geneAsset];
		}
		return null;
	}

	private void reloadButtons()
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int num2 = 0;
		foreach (GeneButton value in _dictionary_gene_buttons.Values)
		{
			bool flag = value.getElementAsset().isAvailable();
			num2++;
			if (flag)
			{
				num++;
				((Graphic)value.image).color = Toolbox.color_white;
			}
			else
			{
				((Graphic)value.image).color = Toolbox.color_black;
			}
			((Behaviour)((Component)value).GetComponent<DraggableLayoutElement>()).enabled = flag;
		}
		_text_unlocked_genes.text = num + "/" + num2;
		AchievementLibrary.genes_explorer.checkBySignal();
	}

	protected virtual bool hasGene(GeneAsset pTrait)
	{
		return _selected_chromosome.hasGene(pTrait);
	}
}
