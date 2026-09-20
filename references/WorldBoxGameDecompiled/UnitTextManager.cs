using System.Collections.Generic;
using UnityEngine;

public class UnitTextManager : MonoBehaviour
{
	[SerializeField]
	private DragSnapElement _follow_object;

	private List<UnitTextPhrases> _phrases = new List<UnitTextPhrases>();

	private void Start()
	{
		UnitTextPhrases[] componentsInChildren = ((Component)this).GetComponentsInChildren<UnitTextPhrases>(true);
		foreach (UnitTextPhrases item in componentsInChildren)
		{
			_phrases.Add(item);
		}
	}

	private void Update()
	{
		if (SelectedUnit.isSet() && !SelectedUnit.unit.isLying() && !((Object)(object)_follow_object == (Object)null) && Time.frameCount % 50 == 0 && Config.isDraggingItem() && Config.dragging_item_object.HasComponent<UnitAvatarLoader>())
		{
			startNewCurse();
		}
	}

	public void startNew(string pText)
	{
		using ListPool<UnitTextPhrases> listPool = new ListPool<UnitTextPhrases>();
		foreach (UnitTextPhrases phrase in _phrases)
		{
			if (!phrase.isTweening())
			{
				listPool.Add(phrase);
			}
		}
		if (listPool.Count != 0)
		{
			listPool.GetRandom().startNewTween(pText, Config.dragging_item_object?.transform);
		}
	}

	public void startNewCurse()
	{
		string randomInsultText = getRandomInsultText();
		startNew(randomInsultText);
	}

	public void startNewWhat()
	{
		if (SelectedUnit.unit.isLying())
		{
			return;
		}
		if (Randy.randomChance(0.3f))
		{
			startNewCurse();
			return;
		}
		int num = Randy.randomInt(1, 7);
		bool flag = Randy.randomBool();
		using StringBuilderPool stringBuilderPool = new StringBuilderPool(num);
		for (int i = 0; i < num; i++)
		{
			if (flag)
			{
				stringBuilderPool.Append('?');
			}
			else
			{
				stringBuilderPool.Append('!');
			}
		}
		string pText = stringBuilderPool.ToString();
		startNew(pText);
	}

	public void spawnAvatarText(Actor pActor = null)
	{
		if ((!pActor.isRekt() && pActor.isLying()) || (pActor == null && SelectedUnit.unit.isLying()))
		{
			startNewCurse();
			return;
		}
		ActorAsset pAsset = (pActor.isRekt() ? null : pActor.getActorAsset());
		string assetText = getAssetText(pAsset);
		startNew(assetText);
	}

	public string getAssetText(ActorAsset pAsset = null)
	{
		ActorAsset actorAsset = pAsset ?? SelectedUnit.unit.asset;
		int num = Randy.randomInt(1, 3);
		string text = "click_" + actorAsset.id + "_" + num;
		if (LocalizedTextManager.instance.contains(text))
		{
			return LocalizedTextManager.getText(text);
		}
		return getRandomInsultText();
	}

	private string getRandomInsultText()
	{
		return InsultStringGenerator.getRandomText();
	}
}
