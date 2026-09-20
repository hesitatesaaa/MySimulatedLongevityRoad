using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CubeOverview : MonoBehaviour, IInitializePotentialDragHandler, IEventSystemHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	[SerializeField]
	private DragSnapElement _knob_perspective_strength_x;

	[SerializeField]
	private DragSnapElement _knob_perspective_strength_y;

	[SerializeField]
	private DragSnapElement _knob_perspective_strength_z;

	[SerializeField]
	private DragSnapElement _knob_perspective_strength_main;

	[SerializeField]
	private DragSnapElement _knob_warp;

	[SerializeField]
	private DragSnapElement _knob_lense;

	[SerializeField]
	private DragSnapElement _knob_spacing;

	[SerializeField]
	private DragSnapElement _knob_speed_outer;

	[SerializeField]
	private DragSnapElement _knob_speed_inner;

	[SerializeField]
	private DragSnapElement _knob_speed_4d;

	[SerializeField]
	private DragSnapElement _knob_icon_size;

	[SerializeField]
	private DragSnapElement _knob_connection_size;

	[SerializeField]
	private DragSnapElement _knob_reset;

	private CubeNode _active_node;

	[SerializeField]
	private CubeNode _prefab_node;

	[SerializeField]
	private CubeNodeConnection _prefab_connection;

	[SerializeField]
	private RectTransform _parent_connections;

	[SerializeField]
	private RectTransform _parent_nodes;

	[SerializeField]
	private GameObject _object_main;

	private float _offset_target_x;

	private float _offset_target_y;

	private bool _is_dragging;

	private Vector2 _last_mouse_delta;

	private float _offset_x;

	private float _offset_y;

	internal bool highlighted;

	private List<CubeNode> _nodes_by_index;

	private List<CubeNode> _nodes;

	private ObjectPoolGenericMono<CubeNode> _pool_nodes;

	private ObjectPoolGenericMono<CubeNodeConnection> _pool_connections;

	private Quaternion _rotation_q;

	private Quaternion _rotation_q_2;

	private List<CubeNodeAssetData> _all_available_assets;

	private CubeNode _latest_touched_node;

	private KnowledgeAsset _filter_asset;

	private float _angle_4d;

	private const float DRAGGING_SMOOTHING_TIME = 0.1f;

	private const float ROTATION_BOUNDS = 0.7f;

	private const float ROTATION_BOUNDS_MARGIN = 1.05f;

	private const float DRAG_SPEED = 0.46f;

	private const float DRAG_ROTATE_SPEED = 0.005f;

	private const float MIN_NODE_CURSOR_DISTANCE = 40f;

	public float RADIUS_NODE_PLACEMENT;

	private const float NODE_SCALE_MIN = 0.4f;

	private const float NODE_SCALE_MAX = 1.2f;

	private Color _color_node_back;

	private Color _color_node_front;

	private Color _node_highlighted;

	private Color _color_connection_back;

	private Color _color_connection_default;

	private const float PERSPECTIVE_STRENGTH_MAIN = 3f;

	private const float PERSPECTIVE_STRENGTH_MAIN_MOD = 1f;

	private const float PERSPECTIVE_STRENGTH_AXIS = 1f;

	private const float SPACING_MOD = 1f;

	private const float SPEED_MOD_OUTER = 0.2f;

	private const float SPEED_MOD_INNER = 0.2f;

	private const float SPEED_MOD_4D = 0.3f;

	private const float MOD_NODE_SIZE = 1f;

	private const float MOD_CONNECTION_SIZE = 1f;

	private const float WARP_MOD = 0f;

	private const float LENSE_MOD = 0f;

	private const float FOLD_MOD = 0f;

	private float _perspective_strength_main_mod;

	private float _perspective_strength_main;

	private float _perspective_strength_x;

	private float _perspective_strength_y;

	private float _perspective_strength_z;

	private float _mod_lense;

	private float _mod_warp;

	private float _spacing_mod;

	private float _speed_mod_inner;

	private float _speed_mod_outer;

	private float _speed_mod_4d;

	private float _mod_node_size;

	private float _mod_connection_size;

	public float spacing;

	private static readonly Vector4[] _hypercube_positions;

	private static readonly int[,] _hypercube_connections;

	protected void Awake()
	{
		_pool_nodes = new ObjectPoolGenericMono<CubeNode>(_prefab_node, (Transform)(object)_parent_nodes);
		_pool_connections = new ObjectPoolGenericMono<CubeNodeConnection>(_prefab_connection, (Transform)(object)_parent_connections);
	}

	private void initStartPositions()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _hypercube_positions.Length; i++)
		{
			CubeNodeAssetData random = _all_available_assets.GetRandom();
			CubeNode next = _pool_nodes.getNext();
			next.setupAsset(random);
			next.logical_pos = _hypercube_positions[i];
			next.setDebugText(i.ToString() ?? "");
			((Object)((Component)next).gameObject).name = i.ToString();
			_nodes.Add(next);
			_nodes_by_index.Add(next);
		}
		updateNodesVisual();
	}

	private void prepareConnections()
	{
		for (int i = 0; i < _hypercube_connections.GetLength(0); i++)
		{
			int index = _hypercube_connections[i, 0];
			int index2 = _hypercube_connections[i, 1];
			CubeNode pNode = _nodes_by_index[index];
			CubeNode pNode2 = _nodes_by_index[index2];
			makeConnection(pNode, pNode2);
		}
	}

	private Vector3 project4Dto3D(Vector4 p)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		float num = _perspective_strength_main * _perspective_strength_main_mod;
		float num2 = Mathf.Exp((0f - Mathf.Abs(p.w)) * _mod_lense);
		float num3 = p.w;
		float num4 = Mathf.Sin(num3 * _mod_warp);
		if (_mod_warp > 0f)
		{
			num3 = num4;
		}
		float num5 = num - num3;
		if (Mathf.Abs(num5) < 0.01f)
		{
			num5 = 0.01f * Mathf.Sign(num5);
		}
		float num6 = ((num5 == 0f) ? 0f : (num / num5));
		num6 *= num2;
		return new Vector3(p.x * num6 * _perspective_strength_x, p.y * num6 * _perspective_strength_y, p.z * num6 * _perspective_strength_z);
	}

	private void updateRotationAndSpeeds()
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		if (!_is_dragging)
		{
			_angle_4d += Time.deltaTime * _speed_mod_4d;
		}
		if (Input.GetMouseButton(0))
		{
			_perspective_strength_main = Mathf.Lerp(_perspective_strength_main, 4f, 0.1f);
		}
		else
		{
			_perspective_strength_main = Mathf.Lerp(_perspective_strength_main, 3f, 0.1f);
		}
		float num = 0f - _offset_x;
		float num2 = 0f - _offset_y;
		float num3 = _offset_y;
		float num4 = _offset_y;
		if (!_is_dragging)
		{
			num += _speed_mod_inner;
			num2 += _speed_mod_inner;
			num3 += _speed_mod_outer;
			num4 += _speed_mod_outer;
		}
		Quaternion val = Quaternion.Euler(num, num2, 0f);
		_rotation_q = val * _rotation_q;
		Quaternion val2 = Quaternion.Euler(num3, num4, 0f);
		_rotation_q_2 = val2 * _rotation_q_2;
	}

	private void updateNodesVisual()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		float angle_4d = _angle_4d;
		foreach (CubeNode node in _nodes)
		{
			bool num = node.logical_pos.w < 0f;
			float num2 = spacing * _spacing_mod;
			Vector4 p = rotate4D(node.logical_pos, angle_4d);
			Vector3 val = project4Dto3D(p) * num2;
			Vector3 localPosition = (num ? _rotation_q : _rotation_q_2) * val;
			((Component)node).transform.localPosition = localPosition;
			calculateNodeDepth(node, RADIUS_NODE_PLACEMENT);
			updateNodeColorAndScale(node);
		}
		sortNodesByDepth();
	}

	private Vector4 rotate4D(Vector4 pPoint, float pAngle)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		float num = Mathf.Cos(pAngle);
		float num2 = Mathf.Sin(pAngle);
		float num3 = pPoint.x * num - pPoint.w * num2;
		float num4 = pPoint.x * num2 + pPoint.w * num;
		float num5 = pPoint.y * num - pPoint.z * num2;
		float num6 = pPoint.y * num2 + pPoint.z * num;
		return new Vector4(num3, num5, num6, num4);
	}

	protected void OnEnable()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		ShortcutExtensions.DOKill((Component)(object)_object_main.transform, false);
		_object_main.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		TweenSettingsExtensions.SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(ShortcutExtensions.DOScale(_object_main.transform, 1f, 0.6f), (Ease)27);
		fillAssets();
		clearContent();
		initStartPositions();
		prepareConnections();
		_is_dragging = false;
	}

	private CubeNodeConnection makeConnection(CubeNode pNode1, CubeNode pNode2)
	{
		CubeNodeConnection next = _pool_connections.getNext();
		next.node_1 = pNode1;
		next.node_2 = pNode2;
		pNode1.addConnection(pNode2, next);
		pNode2.addConnection(pNode1, next);
		if (pNode1.logical_pos.w < 0f && pNode2.logical_pos.w < 0f)
		{
			next.setConnection(pInner: true);
		}
		else
		{
			next.setConnection(pInner: false);
		}
		((Object)((Component)next).gameObject).name = "connection " + ((Object)((Component)pNode1).gameObject).name + "-" + ((Object)((Component)pNode2).gameObject).name;
		return next;
	}

	private void fillAssets()
	{
		_all_available_assets.Clear();
		if (_filter_asset != null)
		{
			loadUnlockables(_filter_asset.get_library(), _filter_asset.id);
			_filter_asset = null;
			return;
		}
		loadUnlockables(AssetManager.actor_library, "units");
		loadUnlockables(AssetManager.items, "items");
		loadUnlockables(AssetManager.gene_library, "genes");
		loadUnlockables(AssetManager.traits, "traits");
		loadUnlockables(AssetManager.subspecies_traits, "subspecies_traits");
		loadUnlockables(AssetManager.culture_traits, "culture_traits");
		loadUnlockables(AssetManager.language_traits, "language_traits");
		loadUnlockables(AssetManager.clan_traits, "clan_traits");
		loadUnlockables(AssetManager.religion_traits, "religion_traits");
		loadUnlockables(AssetManager.kingdoms_traits, "kingdom_traits");
		loadUnlockables(AssetManager.plots_library, "plots");
	}

	private void loadUnlockables(ILibraryWithUnlockables pLibrary, string pType)
	{
		foreach (BaseUnlockableAsset item in pLibrary.elements_list)
		{
			if (item.show_in_knowledge_window && !item.isTemplateAsset())
			{
				_all_available_assets.Add(new CubeNodeAssetData(item, pType));
			}
		}
	}

	private void Update()
	{
		if (InputHelpers.mouseSupported || (Object)(object)_latest_touched_node == (Object)null || !Tooltip.isShowingFor(((Component)_latest_touched_node).transform))
		{
			updateRotationAndSpeeds();
		}
		foreach (CubeNode node in _nodes)
		{
			node.update();
		}
		if (!_is_dragging)
		{
			smoothOffsets();
			_active_node = getHighlightedNode();
			highlightNode(_active_node);
		}
		updateNodesVisual();
		updateConnectionPositions();
		updateKnobs();
	}

	private void updateKnobs()
	{
		float num = 0.05f;
		if ((Object)(object)_knob_perspective_strength_main != (Object)null)
		{
			float num2 = _knob_perspective_strength_main.getDragMod() * 0.03f;
			_perspective_strength_main_mod += num2 * num;
			_perspective_strength_main_mod = Mathf.Clamp(_perspective_strength_main_mod, 0.1f, 1f);
		}
		if ((Object)(object)_knob_perspective_strength_x != (Object)null)
		{
			float dragMod = _knob_perspective_strength_x.getDragMod();
			_perspective_strength_x += dragMod * num;
			_perspective_strength_x = Mathf.Clamp(_perspective_strength_x, 0f, 2f);
		}
		if ((Object)(object)_knob_perspective_strength_y != (Object)null)
		{
			float dragMod2 = _knob_perspective_strength_y.getDragMod();
			_perspective_strength_y += dragMod2 * num;
			_perspective_strength_y = Mathf.Clamp(_perspective_strength_y, 0f, 2f);
		}
		if ((Object)(object)_knob_perspective_strength_z != (Object)null)
		{
			float dragMod3 = _knob_perspective_strength_z.getDragMod();
			_perspective_strength_z += dragMod3 * num;
			_perspective_strength_z = Mathf.Clamp(_perspective_strength_z, 0f, 2f);
		}
		if ((Object)(object)_knob_spacing != (Object)null)
		{
			float dragMod4 = _knob_spacing.getDragMod();
			_spacing_mod += dragMod4 * num;
			_spacing_mod = Mathf.Clamp(_spacing_mod, 0f, 3f);
		}
		if ((Object)(object)_knob_warp != (Object)null)
		{
			float dragMod5 = _knob_warp.getDragMod();
			_mod_warp += dragMod5 * num;
			_mod_warp = Mathf.Clamp(_mod_warp, 0f, 10f);
		}
		if ((Object)(object)_knob_lense != (Object)null)
		{
			float dragMod6 = _knob_lense.getDragMod();
			_mod_lense += dragMod6 * num;
			_mod_lense = Mathf.Clamp(_mod_lense, 0f, 2f);
		}
		if ((Object)(object)_knob_speed_outer != (Object)null)
		{
			float dragMod7 = _knob_speed_outer.getDragMod();
			_speed_mod_outer += dragMod7 * num;
			_speed_mod_outer = Mathf.Clamp(_speed_mod_outer, 0f, 20f);
		}
		if ((Object)(object)_knob_speed_inner != (Object)null)
		{
			float dragMod8 = _knob_speed_inner.getDragMod();
			_speed_mod_inner += dragMod8 * num;
			_speed_mod_inner = Mathf.Clamp(_speed_mod_inner, 0f, 20f);
		}
		if ((Object)(object)_knob_connection_size != (Object)null)
		{
			float dragMod9 = _knob_connection_size.getDragMod();
			_mod_connection_size += dragMod9 * num;
			_mod_connection_size = Mathf.Clamp(_mod_connection_size, 0f, 10f);
		}
		if ((Object)(object)_knob_icon_size != (Object)null)
		{
			float dragMod10 = _knob_icon_size.getDragMod();
			_mod_node_size += dragMod10 * num;
			_mod_node_size = Mathf.Clamp(_mod_node_size, 0f, 20f);
		}
		if ((Object)(object)_knob_speed_4d != (Object)null)
		{
			float dragMod11 = _knob_speed_4d.getDragMod();
			_speed_mod_4d += dragMod11 * num;
			_speed_mod_4d = Mathf.Clamp(_speed_mod_4d, 0f, 20f);
		}
		if ((Object)(object)_knob_reset != (Object)null)
		{
			float dragMod12 = _knob_reset.getDragMod();
			dragMod12 = Math.Abs(dragMod12);
			_perspective_strength_main = Mathf.Lerp(_perspective_strength_main, 3f, dragMod12 * num);
			_perspective_strength_x = Mathf.Lerp(_perspective_strength_x, 1f, dragMod12 * num);
			_perspective_strength_y = Mathf.Lerp(_perspective_strength_y, 1f, dragMod12 * num);
			_perspective_strength_z = Mathf.Lerp(_perspective_strength_z, 1f, dragMod12 * num);
			_spacing_mod = Mathf.Lerp(_spacing_mod, 1f, dragMod12 * num);
			_speed_mod_outer = Mathf.Lerp(_speed_mod_outer, 0.2f, dragMod12 * num);
			_speed_mod_inner = Mathf.Lerp(_speed_mod_inner, 0.2f, dragMod12 * num);
			_speed_mod_4d = Mathf.Lerp(_speed_mod_4d, 0.3f, dragMod12 * num);
			_mod_connection_size = Mathf.Lerp(_mod_connection_size, 1f, dragMod12 * num);
			_mod_node_size = Mathf.Lerp(_mod_node_size, 1f, dragMod12 * num);
			_perspective_strength_main_mod = Mathf.Lerp(_perspective_strength_main_mod, 1f, dragMod12 * num);
			_mod_warp = Mathf.Lerp(_mod_warp, 0f, dragMod12 * num);
			_mod_lense = Mathf.Lerp(_mod_lense, 0f, dragMod12 * num);
		}
	}

	private void updateConnectionPositions()
	{
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		foreach (CubeNodeConnection item in _pool_connections.getListTotal())
		{
			item.update();
			float num = 1f;
			CubeNode node_ = item.node_1;
			CubeNode node_2 = item.node_2;
			if (item.inner_cube)
			{
				num = 3f;
			}
			if (node_.highlighted || node_2.highlighted)
			{
				num = 6f;
			}
			num *= _mod_connection_size;
			float num2 = ((!(node_.render_depth > node_2.render_depth)) ? node_2.render_depth : node_.render_depth);
			Color color = Color.Lerp(_color_connection_back, _color_connection_default, num2);
			((Graphic)item.image).color = color;
			Vector2 val = Vector2.op_Implicit(((Component)node_).transform.localPosition);
			Vector2 val2 = Vector2.op_Implicit(((Component)node_2).transform.localPosition);
			Vector2 val3 = (val + val2) / 2f;
			((Component)item).transform.localPosition = Vector2.op_Implicit(val3);
			float num3 = Vector3.Distance(Vector2.op_Implicit(val), Vector2.op_Implicit(val2));
			((Component)item).transform.localScale = new Vector3(num3, num, 1f);
			Vector3 val4 = Vector2.op_Implicit(val2 - val);
			float num4 = Mathf.Atan2(val4.y, val4.x) * 57.29578f;
			((Component)item).transform.rotation = Quaternion.Euler(0f, 0f, num4);
		}
	}

	public CubeNodeAssetData getRandom()
	{
		return _all_available_assets.GetRandom();
	}

	public void setLatestTouched(CubeNode pNode)
	{
		_latest_touched_node = pNode;
	}

	public void setFilterAsset(KnowledgeAsset pAsset)
	{
		_filter_asset = pAsset;
	}

	private void highlightAllConnectonsFromDrag(float pLight)
	{
		foreach (CubeNodeConnection item in _pool_connections.getListTotal())
		{
			if (!(item.mod_light > pLight))
			{
				item.mod_light = pLight;
			}
		}
	}

	private void highlightNode(CubeNode pHighlighted = null)
	{
		foreach (CubeNode node in _nodes)
		{
			if (!((Object)(object)node == (Object)(object)pHighlighted) && node.highlighted)
			{
				node.highlighted = false;
				Tooltip.hideTooltipNow();
			}
		}
		pHighlighted?.setHighlighted();
	}

	private CubeNode getClosestNodeToCursor()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		CubeNode result = null;
		float num = float.MaxValue;
		Vector2 val = Vector2.op_Implicit(Input.mousePosition);
		if (!InputHelpers.mouseSupported && InputHelpers.touchCount == 0)
		{
			return _active_node;
		}
		foreach (CubeNode node in _nodes)
		{
			Vector2 val2 = Vector2.op_Implicit(((Component)node).transform.position);
			float num2 = Vector2.Distance(val, val2);
			if (!(num2 > 40f))
			{
				if ((Object)(object)node == (Object)(object)_active_node)
				{
					return node;
				}
				if (num2 < num)
				{
					num = num2;
					result = node;
				}
			}
		}
		return result;
	}

	private void smoothOffsets()
	{
		_offset_x = Mathf.Lerp(_offset_x, _offset_target_x, 0.1f);
		_offset_y = Mathf.Lerp(_offset_y, _offset_target_y, 0.1f);
	}

	internal bool isDragging()
	{
		return _is_dragging;
	}

	private void calculateNodeDepth(CubeNode pElement, float pRadius)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		float z = ((Component)pElement).transform.localPosition.z;
		float render_depth = Mathf.InverseLerp(0f - pRadius, pRadius, z);
		pElement.render_depth = render_depth;
	}

	private void sortNodesByDepth()
	{
		foreach (CubeNode node in _nodes)
		{
			((Component)node).transform.SetAsLastSibling();
		}
		_nodes.Sort((CubeNode a, CubeNode b) => a.render_depth.CompareTo(b.render_depth));
	}

	private void clearContent()
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		foreach (CubeNode node in _nodes)
		{
			node.clear();
		}
		foreach (CubeNodeConnection item in _pool_connections.getListTotal())
		{
			item.clear();
		}
		_rotation_q = Quaternion.identity;
		_rotation_q_2 = Quaternion.identity;
		_pool_connections.clear();
		_pool_nodes.clear();
		_nodes.Clear();
		_nodes_by_index.Clear();
	}

	public void OnDrag(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		_is_dragging = true;
		Vector2 delta = eventData.delta;
		if (((Vector2)(ref delta)).magnitude > ((Vector2)(ref _last_mouse_delta)).magnitude)
		{
			highlightAllConnectonsFromDrag(0.35f);
		}
		_last_mouse_delta = delta;
		_offset_x = (0f - delta.y) * 0.46f;
		_offset_y = delta.x * 0.46f;
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		_is_dragging = false;
		Vector2 delta = eventData.delta;
		_offset_target_x += (0f - delta.y) * 0.005f;
		_offset_target_y += delta.x * 0.005f;
		if (Mathf.Abs(_offset_target_x) > 0.7f || Mathf.Abs(_offset_target_y) > 0.7f)
		{
			if (Mathf.Abs(_offset_target_x) > Mathf.Abs(_offset_target_y))
			{
				_offset_target_y = _offset_target_y / Mathf.Abs(_offset_target_x) * 0.7f;
			}
			else
			{
				_offset_target_x = _offset_target_x / Mathf.Abs(_offset_target_y) * 0.7f;
			}
		}
		_offset_target_x = Mathf.Clamp(_offset_target_x, -0.7f, 0.7f);
		_offset_target_y = Mathf.Clamp(_offset_target_y, -0.7f, 0.7f);
		highlightAllConnectonsFromDrag(1f);
	}

	public void OnInitializePotentialDrag(PointerEventData eventData)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		eventData.useDragThreshold = false;
		_last_mouse_delta = Vector2.zero;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		_offset_x = (_offset_target_x = 0f);
		_offset_y = (_offset_target_y = 0f);
		Tooltip.hideTooltipNow();
	}

	private void updateNodeColorAndScale(CubeNode pNode)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((!pNode.current_asset.isUnlockedByPlayer()) ? Toolbox.color_black : ((!pNode.highlighted) ? Color.Lerp(_color_node_back, _color_node_front, pNode.render_depth) : Color.Lerp(_color_node_back, _node_highlighted, pNode.render_depth)));
		pNode.setColor(color);
		float num = Mathf.Lerp(0.4f, 1.2f, pNode.render_depth);
		if (Mathf.Approximately(num, 0.4f))
		{
			pNode.setupAsset(getRandom());
		}
		num *= pNode.scale_mod_spawn * pNode.bonus_scale;
		num *= _mod_node_size;
		((Component)pNode).transform.localScale = new Vector3(num, num, num);
		pNode.updateTooltip();
	}

	private CubeNode getHighlightedNode()
	{
		if (_is_dragging)
		{
			return null;
		}
		if (_offset_x > 1.05f || _offset_x < -1.05f)
		{
			return null;
		}
		if (_offset_y > 1.05f || _offset_y < -1.05f)
		{
			return null;
		}
		return getClosestNodeToCursor();
	}

	public CubeOverview()
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		_offset_target_x = -0.015f;
		_offset_target_y = 0.07f;
		_nodes_by_index = new List<CubeNode>();
		_nodes = new List<CubeNode>();
		_rotation_q = Quaternion.identity;
		_rotation_q_2 = Quaternion.identity;
		_all_available_assets = new List<CubeNodeAssetData>();
		RADIUS_NODE_PLACEMENT = 30f;
		_color_node_back = Toolbox.makeColor("#1D7A74");
		_color_node_front = Toolbox.makeColor("#DDDDDD");
		_node_highlighted = Toolbox.makeColor("#FFFFFF");
		_color_connection_back = Toolbox.makeColor("#1D7A74", 0.5f);
		_color_connection_default = Toolbox.makeColor("#3AFFF5", 1f);
		_perspective_strength_main_mod = 1f;
		_perspective_strength_main = 3f;
		_perspective_strength_x = 1f;
		_perspective_strength_y = 1f;
		_perspective_strength_z = 1f;
		_spacing_mod = 1f;
		_speed_mod_inner = 0.2f;
		_speed_mod_outer = 0.2f;
		_speed_mod_4d = 0.3f;
		_mod_node_size = 1f;
		_mod_connection_size = 1f;
		spacing = 25f;
		((MonoBehaviour)this)._002Ector();
	}

	static CubeOverview()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0204: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		_hypercube_positions = (Vector4[])(object)new Vector4[16]
		{
			new Vector4(-1f, -1f, -1f, -1f),
			new Vector4(1f, -1f, -1f, -1f),
			new Vector4(-1f, 1f, -1f, -1f),
			new Vector4(1f, 1f, -1f, -1f),
			new Vector4(-1f, -1f, 1f, -1f),
			new Vector4(1f, -1f, 1f, -1f),
			new Vector4(-1f, 1f, 1f, -1f),
			new Vector4(1f, 1f, 1f, -1f),
			new Vector4(-1f, -1f, -1f, 1f),
			new Vector4(1f, -1f, -1f, 1f),
			new Vector4(-1f, 1f, -1f, 1f),
			new Vector4(1f, 1f, -1f, 1f),
			new Vector4(-1f, -1f, 1f, 1f),
			new Vector4(1f, -1f, 1f, 1f),
			new Vector4(-1f, 1f, 1f, 1f),
			new Vector4(1f, 1f, 1f, 1f)
		};
		_hypercube_connections = new int[32, 2]
		{
			{ 0, 1 },
			{ 0, 2 },
			{ 0, 4 },
			{ 0, 8 },
			{ 1, 3 },
			{ 1, 5 },
			{ 1, 9 },
			{ 2, 3 },
			{ 2, 6 },
			{ 2, 10 },
			{ 3, 7 },
			{ 3, 11 },
			{ 4, 5 },
			{ 4, 6 },
			{ 4, 12 },
			{ 5, 7 },
			{ 5, 13 },
			{ 6, 7 },
			{ 6, 14 },
			{ 7, 15 },
			{ 8, 9 },
			{ 8, 10 },
			{ 8, 12 },
			{ 9, 11 },
			{ 9, 13 },
			{ 10, 11 },
			{ 10, 14 },
			{ 11, 15 },
			{ 12, 13 },
			{ 12, 14 },
			{ 13, 15 },
			{ 14, 15 }
		};
	}
}
