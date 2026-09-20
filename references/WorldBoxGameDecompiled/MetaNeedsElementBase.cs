using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class MetaNeedsElementBase : WindowMetaElementBase, IRefreshElement
{
	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private Text _text;

	private Actor _actor;

	private IMetaWindow _window;

	protected override void Awake()
	{
		base.Awake();
		setupTooltip();
		setupClickAction();
		_window = ((Component)this).GetComponentInParent<IMetaWindow>();
	}

	protected override IEnumerator showContent()
	{
		if (_window.getCoreObject() is IMetaObject metaObject && metaObject.isAlive())
		{
			string text = getText(metaObject, out var pActorResult);
			_text.text = text;
			_actor = pActorResult;
			if (!string.IsNullOrEmpty(text))
			{
				_container.gameObject.SetActive(true);
			}
		}
		yield break;
	}

	protected virtual string getText(IMetaObject pMeta, out Actor pActorResult)
	{
		throw new NotImplementedException();
	}

	private void setupTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (_container.TryGetComponent<TipButton>(ref tipButton))
		{
			tipButton.hoverAction = tooltipAction;
		}
	}

	private void setupClickAction()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Expected O, but got Unknown
		Button val = default(Button);
		if (_container.TryGetComponent<Button>(ref val))
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(buttonAction));
		}
	}

	private void tooltipAction()
	{
		if (!_actor.isRekt())
		{
			_actor.showTooltip(this);
		}
	}

	private void buttonAction()
	{
		if (!_actor.isRekt())
		{
			ActionLibrary.openUnitWindow(_actor);
		}
	}

	protected override void clear()
	{
		base.clear();
		_container.gameObject.SetActive(false);
	}
}
