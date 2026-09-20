using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ChainElement : TraitButton<GeneAsset>
{
	public Image chain_left;

	public Image chain_right;

	public Image chain_up;

	public Image chain_down;

	internal int locus_index = -1;

	protected GeneAsset gene => augmentation_asset;

	public GeneAsset getGeneAsset()
	{
		return gene;
	}

	public override void load(GeneAsset pAsset)
	{
		base.load(pAsset);
		((Object)((Component)this).gameObject).name = gene.id;
		colorChains();
	}

	public void colorChains()
	{
		if (!gene.show_genepool_nucleobases)
		{
			hideChains();
			return;
		}
		showChain(chain_left, pShow: true, gene.genetic_code_left);
		showChain(chain_right, pShow: true, gene.genetic_code_right);
		showChain(chain_up, pShow: true, gene.genetic_code_up);
		showChain(chain_down, pShow: true, gene.genetic_code_down);
	}

	protected void hideChains()
	{
		hideChain(chain_left);
		hideChain(chain_right);
		hideChain(chain_up);
		hideChain(chain_down);
	}

	protected void showChain(Image pChainImage, bool pShow, char pGeneticCode, Color? pColor = null)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		((Component)pChainImage).gameObject.SetActive(pShow);
		if (pColor.HasValue)
		{
			colorChain(pChainImage, pColor.Value);
		}
		else if (pShow)
		{
			colorChain(pChainImage, NucleobaseHelper.getColor(pGeneticCode));
		}
	}

	protected void hideChain(Image pChain)
	{
		((Component)pChain).gameObject.SetActive(false);
	}

	protected void colorChain(Image pChain, Color pColor)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)pChain).color = pColor;
	}
}
