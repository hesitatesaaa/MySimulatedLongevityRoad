using UnityEngine;

public class KnowledgeWindow : TabbedWindow
{
	[SerializeField]
	private Transform _elements_parent;

	[SerializeField]
	private KnowledgeElement _element_prefab;

	[SerializeField]
	private StatBar _progress_bar;

	[SerializeField]
	private CubeOverview _cube_overview_big;

	[SerializeField]
	private WindowMetaTab _cube_tab;

	protected override void create()
	{
		base.create();
		foreach (KnowledgeAsset item in AssetManager.knowledge_library.list)
		{
			if (item.show_in_knowledge_window)
			{
				KnowledgeElement knowledgeElement = Object.Instantiate<KnowledgeElement>(_element_prefab, _elements_parent);
				knowledgeElement.setAsset(item);
				knowledgeElement.setCube(_cube_overview_big, _cube_tab);
			}
		}
	}

	private void OnEnable()
	{
		int num = 0;
		int num2 = 0;
		foreach (KnowledgeAsset item in AssetManager.knowledge_library.list)
		{
			if (item.show_in_knowledge_window)
			{
				num += item.countUnlockedByPlayer();
				num2 += item.countTotal();
			}
		}
		_progress_bar.setBar(num, num2, "/" + num2.ToText());
	}
}
