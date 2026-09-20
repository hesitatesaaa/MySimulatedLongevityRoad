using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

public class CubeNode : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IInitializePotentialDragHandler
{
	private const float SCALE_HIGHLIGHTED = 1.6f;

	private const float SCALE_NORMAL = 1f;

	private const float TOOLTIP_SCALE_MIN = 0.4f;

	private const float TOOLTIP_SCALE_MAX = 1f;

	public Vector4 logical_pos;

	internal List<CubeNode> connected_nodes = new List<CubeNode>();

	private List<CubeNodeConnection> connections = new List<CubeNodeConnection>();

	[SerializeField]
	private Image _image;

	[SerializeField]
	private Text _text;

	private CubeOverview _cube_overview;

	internal float render_depth;

	internal float scale_mod_spawn = 1f;

	internal float bonus_scale = 1f;

	internal bool highlighted;

	private float _timer_change;

	private TooltipData _tooltip_data;

	private CubeNodeAssetData _data;

	public BaseUnlockableAsset current_asset => _data.asset;

	private void Start()
	{
		_cube_overview = ((Component)this).gameObject.GetComponentInParent<CubeOverview>();
		initClick();
		initTooltip();
	}

	public void update()
	{
		_timer_change -= Time.deltaTime;
	}

	public void setDebugText(string pText)
	{
		_text.text = pText;
	}

	public void clear()
	{
		connected_nodes.Clear();
		connections.Clear();
		_timer_change = 0f;
	}

	protected void initClick()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Button val = default(Button);
		if (((Component)this).TryGetComponent<Button>(ref val))
		{
			((UnityEvent)val.onClick).AddListener(new UnityAction(setPressed));
		}
	}

	protected void initTooltip()
	{
		TipButton tipButton = default(TipButton);
		if (((Component)this).TryGetComponent<TipButton>(ref tipButton))
		{
			Object.Destroy((Object)(object)tipButton);
		}
	}

	private void showTooltip()
	{
		_cube_overview.setLatestTouched(this);
		KnowledgeAsset knowledgeAsset = AssetManager.knowledge_library.get(_data.knowledge_type);
		_tooltip_data = knowledgeAsset.show_tooltip(((Component)this).transform, _data.asset);
	}

	public void setupAsset(CubeNodeAssetData pData)
	{
		if (!(_timer_change > 0f))
		{
			_timer_change = 2f;
			_data = pData;
			_image.sprite = _data.asset.getSprite();
		}
	}

	public void updateTooltip()
	{
		if (highlighted && Tooltip.isShowingFor(((Component)this).transform))
		{
			_tooltip_data.tooltip_scale = Mathf.Lerp(0.4f, 1f, render_depth);
		}
	}

	public void setHighlighted()
	{
		if (!highlighted)
		{
			highlighted = true;
			scale_mod_spawn = 1.6f;
			showTooltip();
		}
	}

	public void setPressed()
	{
		_cube_overview.isDragging();
	}

	public void setColor(Color pColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Graphic)_image).color = pColor;
	}

	public void addConnection(CubeNode pNode, CubeNodeConnection pConnection)
	{
		connected_nodes.Add(pNode);
		connections.Add(pConnection);
	}

	public void OnInitializePotentialDrag(PointerEventData pEventData)
	{
		CubeOverview cube_overview = _cube_overview;
		if (cube_overview != null)
		{
			((Component)cube_overview).SendMessage("OnInitializePotentialDrag", (object)pEventData);
		}
	}

	public void OnBeginDrag(PointerEventData pEventData)
	{
		CubeOverview cube_overview = _cube_overview;
		if (cube_overview != null)
		{
			((Component)cube_overview).SendMessage("OnBeginDrag", (object)pEventData);
		}
	}

	public void OnDrag(PointerEventData pEventData)
	{
		CubeOverview cube_overview = _cube_overview;
		if (cube_overview != null)
		{
			((Component)cube_overview).SendMessage("OnDrag", (object)pEventData);
		}
	}

	public void OnEndDrag(PointerEventData pEventData)
	{
		CubeOverview cube_overview = _cube_overview;
		if (cube_overview != null)
		{
			((Component)cube_overview).SendMessage("OnEndDrag", (object)pEventData);
		}
	}
}
