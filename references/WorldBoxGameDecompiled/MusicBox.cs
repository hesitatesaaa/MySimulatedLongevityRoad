using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class MusicBox : MonoBehaviour
{
	private const int MUSIC_ZONES_SIZE = 3;

	private const int IDLE_SOUND_TIMER_MIN = 5;

	private const int IDLE_SOUND_TIMER_MAX = 12;

	public static MusicBox inst;

	private readonly HashSet<string> _flags_to_enable = new HashSet<string>();

	private EventInstance _music_event;

	internal MusicBoxDebug debug_box;

	private float _timer;

	private const float INTERVAL_UPDATE = 1f;

	public static bool music_on = true;

	public static bool sounds_on = true;

	public static bool debug_sounds = true;

	private VCA _vca_sound_effects;

	private VCA _vca_music;

	private VCA _vca_ui;

	private Bus _bus_master;

	private Bus _bus_idle;

	private float _volume_idle = 1f;

	private EVENT_CALLBACK _music_callback;

	private TimelineInfo _timeline_info;

	private GCHandle _timeline_handle;

	public static bool new_world_on_start_played = false;

	private readonly Dictionary<string, EventInstance> _environment_sounds = new Dictionary<string, EventInstance>();

	private readonly Dictionary<string, EventInstance> _drawing_sounds = new Dictionary<string, EventInstance>();

	private static readonly Dictionary<string, bool> _events_cache = new Dictionary<string, bool>();

	private static readonly Dictionary<string, GUID> _events_guids = new Dictionary<string, GUID>();

	private static GameObject _sound_object;

	private int _tiles_sand;

	private int _tiles_shallow_water;

	public MusicState music_state;

	private MusicBoxLibrary _lib;

	public MusicBoxIdle idle;

	private GameObject _camera_listener;

	private bool _created;

	private static System _studio_system
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return RuntimeManager.StudioSystem;
		}
	}

	private static bool fmod_disabled
	{
		get
		{
			if (!music_on)
			{
				return !sounds_on;
			}
			return false;
		}
	}

	private void Awake()
	{
		create();
	}

	internal void create()
	{
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (_created)
		{
			return;
		}
		_created = true;
		inst = this;
		debug_box = new MusicBoxDebug();
		_lib = AssetManager.music_box;
		idle = new MusicBoxIdle();
		ScrollWindow.addCallbackHide(hideWindowCallback);
		if (!fmod_disabled)
		{
			try
			{
				_bus_master = RuntimeManager.GetBus("bus:/");
				if (((Bus)(ref _bus_master)).isValid())
				{
					((Bus)(ref _bus_master)).setVolume(0f);
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)("MusicBox failed to init: " + ex));
				music_on = false;
				sounds_on = false;
				return;
			}
			Platform val = Settings.Instance.FindCurrentPlatform();
			if (debug_sounds)
			{
				PropertyAccessor<TriStateBool> liveUpdate = PropertyAccessors.LiveUpdate;
				liveUpdate.Set(val, (TriStateBool)1);
				liveUpdate = PropertyAccessors.Overlay;
				liveUpdate.Set(val, (TriStateBool)2);
			}
			else
			{
				PropertyAccessor<TriStateBool> liveUpdate = PropertyAccessors.LiveUpdate;
				liveUpdate.Set(val, (TriStateBool)0);
				liveUpdate = PropertyAccessors.Overlay;
				liveUpdate.Set(val, (TriStateBool)0);
			}
			createMusicEvent();
			assignCallback();
			startMusic();
		}
		reserveFlag(MusicBoxLibrary.Neutral_001.id);
		clearParams();
		_sound_object = new GameObject("musicbox_pan");
		_camera_listener = new GameObject("fmod_listener");
		_camera_listener.transform.parent = ((Component)Camera.main).transform;
		_camera_listener.AddComponent<StudioListener>();
	}

	private void setMusicState(MusicState pState)
	{
		music_state = pState;
		if (pState == MusicState.Menu)
		{
			reserveFlag("Menu");
		}
	}

	private void checkDrawingSounds()
	{
		if (!sounds_on)
		{
			return;
		}
		bool flag = false;
		if (InputHelpers.mouseSupported)
		{
			if (!Input.GetMouseButton(0))
			{
				flag = true;
			}
			else if (!ControllableUnit.isControllingUnit() && World.world.isOverUI())
			{
				flag = true;
			}
		}
		else if (Input.touchCount == 0)
		{
			flag = true;
		}
		if (flag)
		{
			inst.stopDrawingSounds();
		}
	}

	private void checkIdleVolume()
	{
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (World.world.isPaused())
		{
			_volume_idle -= Time.deltaTime;
			if (_volume_idle < 0f)
			{
				_volume_idle = 0f;
			}
		}
		else
		{
			_volume_idle += Time.deltaTime;
			if (_volume_idle > 1f)
			{
				_volume_idle = 1f;
			}
		}
		if (!((Bus)(ref _bus_idle)).isValid())
		{
			_bus_idle = RuntimeManager.GetBus("bus:/Idle");
		}
		checkBusVolume(_volume_idle, _bus_idle);
	}

	private void checkVolumes()
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		bool flag = ((VCA)(ref _vca_sound_effects)).isValid();
		if (!flag)
		{
			_vca_sound_effects = RuntimeManager.GetVCA("vca:/Sound Effects");
			_vca_music = RuntimeManager.GetVCA("vca:/Music");
			_vca_ui = RuntimeManager.GetVCA("vca:/UI");
			_bus_master = RuntimeManager.GetBus("bus:/");
			if (!flag)
			{
				return;
			}
		}
		checkBusVolume("volume_master_sound", _bus_master);
		checkVcaVolume("volume_sound_effects", _vca_sound_effects);
		checkVcaVolume("volume_music", _vca_music);
		checkVcaVolume("volume_ui", _vca_ui);
	}

	private void checkBusVolume(float pVolume, Bus pBus)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		float num = default(float);
		((Bus)(ref pBus)).getVolume(ref num);
		if (num != pVolume)
		{
			((Bus)(ref pBus)).setVolume(pVolume);
		}
	}

	private void checkBusVolume(string pOptionParam, Bus pBus)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)PlayerConfig.getIntValue(pOptionParam) / 100f;
		float num2 = default(float);
		((Bus)(ref pBus)).getVolume(ref num2);
		if (num2 != num)
		{
			((Bus)(ref pBus)).setVolume(num);
		}
	}

	private void checkVcaVolume(string pOptionParam, VCA pVCA)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		float num = (float)PlayerConfig.getIntValue(pOptionParam) / 100f;
		float num2 = default(float);
		((VCA)(ref pVCA)).getVolume(ref num2);
		if (num2 != num)
		{
			((VCA)(ref pVCA)).setVolume(num);
		}
	}

	public void update(float pElapsed)
	{
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		if (fmod_disabled)
		{
			return;
		}
		Bench.bench("music_box", "music_box_total");
		Bench.bench("check_volume", "music_box");
		checkVolumes();
		checkIdleVolume();
		Bench.benchEnd("check_volume", "music_box", pSaveCounter: false, 0L);
		Bench.bench("update_idle", "music_box");
		idle.update(pElapsed);
		Bench.benchEnd("update_idle", "music_box", pSaveCounter: false, 0L);
		Bench.bench("update_debug", "music_box");
		debug_box.update();
		Bench.benchEnd("update_debug", "music_box", pSaveCounter: false, 0L);
		Bench.bench("update_drawing", "music_box");
		checkDrawingSounds();
		Bench.benchEnd("update_drawing", "music_box", pSaveCounter: false, 0L);
		Bench.bench("update_fmod_params", "music_box");
		Vector3 localPosition = default(Vector3);
		((Vector3)(ref localPosition))._002Ector(0f, 0f, World.world.camera.orthographicSize * 1.5f);
		_camera_listener.transform.localPosition = localPosition;
		updateMainFmodParams();
		Bench.benchEnd("update_fmod_params", "music_box", pSaveCounter: false, 0L);
		if (_timer > 0f)
		{
			_timer -= pElapsed;
			return;
		}
		_timer = 1f;
		Bench.bench("clearParams", "music_box");
		clearParams();
		Bench.benchEnd("clearParams", "music_box", pSaveCounter: false, 0L);
		Bench.bench("drawFmodDebugZones", "music_box");
		drawFmodDebugZones();
		Bench.benchEnd("drawFmodDebugZones", "music_box", pSaveCounter: false, 0L);
		Bench.bench("countZonesUnits", "music_box");
		countUnitsInZones();
		Bench.benchEnd("countZonesUnits", "music_box", pSaveCounter: false, 0L);
		Bench.bench("countSpecialTiles", "music_box");
		countSpecialTilesInChunks();
		Bench.benchEnd("countSpecialTiles", "music_box", pSaveCounter: false, 0L);
		Bench.bench("checkUnitsParams", "music_box");
		checkUnitsParams();
		Bench.benchEnd("checkUnitsParams", "music_box", pSaveCounter: false, 0L);
		Bench.bench("checkCamera", "music_box");
		checkCamera();
		Bench.benchEnd("checkCamera", "music_box", pSaveCounter: false, 0L);
		Bench.bench("music_params_1", "music_box");
		foreach (MusicBoxContainerTiles c_list_param in _lib.c_list_params)
		{
			if (c_list_param.enabled)
			{
				enableMusicParameter(c_list_param.asset.id);
			}
			else
			{
				disableMusicParameter(c_list_param.asset.id);
			}
		}
		Bench.benchEnd("music_params_1", "music_box", pSaveCounter: false, 0L);
		Bench.bench("music_params_2", "music_box");
		foreach (MusicBoxContainerUnits value in _lib.c_dict_units.Values)
		{
			if (value.enabled)
			{
				enableMusicParameter(value.asset.id);
			}
			else
			{
				disableMusicParameter(value.asset.id);
			}
		}
		Bench.benchEnd("music_params_2", "music_box", pSaveCounter: false, 0L);
		Bench.bench("flags", "music_box");
		if (_flags_to_enable.Any())
		{
			foreach (string item in _flags_to_enable)
			{
				enableMusicParameter(item);
			}
			_flags_to_enable.Clear();
		}
		Bench.benchEnd("flags", "music_box", pSaveCounter: false, 0L);
		Bench.bench("check_environment", "music_box");
		foreach (MusicBoxContainerTiles c_list_environment in _lib.c_list_environments)
		{
			checkEnvironmentSound(c_list_environment);
		}
		Bench.benchEnd("check_environment", "music_box", pSaveCounter: false, 0L);
		Bench.benchEnd("music_box", "music_box_total", pSaveCounter: false, 0L);
	}

	private void updateMainFmodParams()
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		System studio_system;
		if (World.world.quality_changer.isLowRes())
		{
			studio_system = _studio_system;
			((System)(ref studio_system)).setParameterByName("MiniMap", 1f, false);
		}
		else
		{
			studio_system = _studio_system;
			((System)(ref studio_system)).setParameterByName("MiniMap", 0f, false);
		}
		float zoomRatioLow = World.world.quality_changer.getZoomRatioLow();
		float zoomRatioHigh = World.world.quality_changer.getZoomRatioHigh();
		float zoomRatioFull = World.world.quality_changer.getZoomRatioFull();
		studio_system = _studio_system;
		((System)(ref studio_system)).setParameterByName("Zoom_Low", zoomRatioLow, false);
		studio_system = _studio_system;
		((System)(ref studio_system)).setParameterByName("Zoom_High", zoomRatioHigh, false);
		studio_system = _studio_system;
		((System)(ref studio_system)).setParameterByName("Zoom_Full", zoomRatioFull, false);
	}

	public static void clearAllSounds()
	{
		if (!fmod_disabled)
		{
			inst.idle.clearAllSounds();
			inst.debug_box.clear();
		}
	}

	public void clearParams()
	{
		foreach (Kingdom kingdom in World.world.kingdoms)
		{
			if (_lib.c_dict_civs.TryGetValue(kingdom.getSpecies(), out var value))
			{
				value.kingdom_exists = true;
			}
		}
		_tiles_sand = 0;
		_tiles_shallow_water = 0;
		foreach (MusicBoxContainerCivs value2 in _lib.c_dict_civs.Values)
		{
			value2.clear();
		}
		foreach (MusicAsset item in _lib.list)
		{
			item.container_tiles?.clear();
		}
		foreach (MusicBoxContainerUnits value3 in _lib.c_dict_units.Values)
		{
			value3.clear();
		}
		DebugLayer.fmod_zones_to_draw.Clear();
	}

	private void hideWindowCallback(string pWindowID)
	{
	}

	private void assignCallback()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Expected O, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		_music_callback = new EVENT_CALLBACK(beatEventCallback);
		_timeline_info = new TimelineInfo();
		_timeline_handle = GCHandle.Alloc(_timeline_info, GCHandleType.Pinned);
		((EventInstance)(ref _music_event)).setUserData(GCHandle.ToIntPtr(_timeline_handle));
		((EventInstance)(ref _music_event)).setCallback(_music_callback, (EVENT_CALLBACK_TYPE)6144);
	}

	public static EventInstance getNewInstance(string pID)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return RuntimeManager.CreateInstance(pID);
	}

	public static EventInstance attachToObject(string pID, GameObject pObject, bool pPlay = true)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!sounds_on)
		{
			return default(EventInstance);
		}
		EventInstance newInstance = getNewInstance(pID);
		RuntimeManager.AttachInstanceToGameObject(newInstance, pObject.transform, false);
		if (pPlay)
		{
			((EventInstance)(ref newInstance)).start();
		}
		return newInstance;
	}

	private void createMusicEvent()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_music_event = getNewInstance("event:/MUSIC/ConsolidatedMusicEvent");
	}

	private void startMusic()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (music_on)
		{
			((EventInstance)(ref _music_event)).start();
		}
	}

	[MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
	private unsafe static RESULT beatEventCallback(EVENT_CALLBACK_TYPE pType, IntPtr pInstancePtr, IntPtr pParameterPtr)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Invalid comparison between Unknown and I4
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		IntPtr intPtr = default(IntPtr);
		RESULT userData = ((EventInstance)(ref inst._music_event)).getUserData(ref intPtr);
		if ((int)userData != 0)
		{
			Debug.LogError((object)("Timeline Callback error: " + ((object)(*(RESULT*)(&userData))/*cast due to constrained. prefix*/).ToString()));
		}
		else if (intPtr != IntPtr.Zero)
		{
			TimelineInfo timelineInfo = (TimelineInfo)GCHandle.FromIntPtr(intPtr).Target;
			if ((int)pType == 2048)
			{
				TIMELINE_MARKER_PROPERTIES val = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(pParameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
				timelineInfo.lastMarker = val.name;
				inst.markerReached(StringWrapper.op_Implicit(timelineInfo.lastMarker));
			}
		}
		return (RESULT)0;
	}

	private void loadBanks()
	{
	}

	private void checkEnvironmentSound(MusicBoxContainerTiles pContainer)
	{
		MusicAsset asset = pContainer.asset;
		bool flag = true;
		if (asset.mini_map_only)
		{
			if (!World.world.quality_changer.isLowRes())
			{
				flag = false;
			}
		}
		else if (World.world.quality_changer.isLowRes())
		{
			flag = false;
		}
		else if (asset.min_zoom <= World.world.camera.orthographicSize)
		{
			flag = false;
		}
		if (flag && asset.min_tiles_to_play != 0 && pContainer.amount < asset.min_tiles_to_play)
		{
			flag = false;
		}
		pContainer.enabled = flag;
		if (flag)
		{
			playEnvironmentSound(pContainer);
		}
		else
		{
			stopEnvironmentSound(pContainer);
		}
	}

	public static void playIdleSoundVisibleOnly(string pSoundPath, WorldTile pTile)
	{
		if (sounds_on)
		{
			playSoundVisibleOnly(pSoundPath, pTile);
		}
	}

	public static void playSoundVisibleOnly(string pSoundPath, WorldTile pTile)
	{
		if (sounds_on)
		{
			playSound(pSoundPath, pTile, pGameViewOnly: true, pVisibleOnly: true);
		}
	}

	public static void playSound(string pSoundPath, WorldTile pTile, bool pGameViewOnly = false, bool pVisibleOnly = false)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		if (!string.IsNullOrEmpty(pSoundPath) && (!pVisibleOnly || pTile.zone.visible))
		{
			Vector2Int pos = pTile.pos;
			float pX = ((Vector2Int)(ref pos)).x;
			pos = pTile.pos;
			playSound(pSoundPath, pX, ((Vector2Int)(ref pos)).y, pGameViewOnly);
		}
	}

	public static void playSoundWorld(string pSoundPath)
	{
	}

	public static void playSoundUI(string pSoundPath)
	{
		playSound(pSoundPath);
	}

	public static EventInstance PlayOneShot(GUID pGuid, Vector3 pPosition, bool pSet3D = true)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		EventInstance result = RuntimeManager.CreateInstance(pGuid);
		if (pSet3D)
		{
			((EventInstance)(ref result)).set3DAttributes(RuntimeUtils.To3DAttributes(pPosition));
		}
		else
		{
			Vector3 position = ((Component)World.world.move_camera).transform.position;
			float orthographicSize = World.world.move_camera.main_camera.orthographicSize;
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(position.x, position.y, orthographicSize);
			((EventInstance)(ref result)).set3DAttributes(RuntimeUtils.To3DAttributes(val));
		}
		((EventInstance)(ref result)).start();
		((EventInstance)(ref result)).release();
		return result;
	}

	private static bool isEventExists(string pEventPath)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		if (!_events_cache.TryGetValue(pEventPath, out var value))
		{
			System studioSystem = RuntimeManager.StudioSystem;
			EventDescription val = default(EventDescription);
			value = (int)((System)(ref studioSystem)).getEvent(pEventPath, ref val) == 0;
			_events_cache.Add(pEventPath, value);
			if (!value)
			{
				Debug.LogWarning((object)("[FMOD] Missing event : " + pEventPath));
			}
			else
			{
				_events_guids[pEventPath] = RuntimeManager.PathToGUID(pEventPath);
			}
		}
		return value;
	}

	public static void playSound(string pSoundPath, float pX = -1f, float pY = -1f, bool pGameViewOnly = false, bool pVisibleOnly = false)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (sounds_on && (!pGameViewOnly || !World.world.quality_changer.isLowRes()) && isEventExists(pSoundPath))
		{
			GUID pGuid = _events_guids[pSoundPath];
			EventInstance? val = null;
			try
			{
				val = ((pX == -1f || pY == -1f) ? new EventInstance?(PlayOneShot(pGuid, Vector3.zero, pSet3D: false)) : new EventInstance?(PlayOneShot(pGuid, new Vector3(pX, pY, 0f))));
			}
			catch (EventNotFoundException)
			{
			}
			if (DebugConfig.isOn(DebugOption.OverlaySounds) || DebugConfig.isOn(DebugOption.OverlaySoundsActive))
			{
				inst.debug_box.add(pSoundPath.Split('/').Last(), pX, pY, val.Value);
			}
		}
	}

	public void playEnvironmentSound(MusicBoxContainerTiles pContainer)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		if (sounds_on)
		{
			MusicAsset asset = pContainer.asset;
			EventInstance val;
			if (_environment_sounds.ContainsKey(asset.fmod_path))
			{
				val = _environment_sounds[asset.fmod_path];
			}
			else
			{
				val = getNewInstance(asset.fmod_path);
				_environment_sounds.Add(asset.fmod_path, val);
			}
			setPan(val, pContainer.cur_pan.x, pContainer.cur_pan.y);
			if (!isPlaying(val))
			{
				((EventInstance)(ref val)).start();
			}
		}
	}

	public void stopEnvironmentSound(MusicBoxContainerTiles pContainer)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		MusicAsset asset = pContainer.asset;
		if (_environment_sounds.ContainsKey(asset.fmod_path))
		{
			EventInstance pInstance = _environment_sounds[asset.fmod_path];
			if (isPlaying(pInstance))
			{
				((EventInstance)(ref pInstance)).stop((STOP_MODE)0);
			}
		}
	}

	public void playDrawingSound(string pSoundPath, float pX = -1f, float pY = -1f)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		if (sounds_on)
		{
			EventInstance val;
			if (_drawing_sounds.ContainsKey(pSoundPath))
			{
				val = _drawing_sounds[pSoundPath];
			}
			else
			{
				val = getNewInstance(pSoundPath);
				_drawing_sounds.Add(pSoundPath, val);
			}
			setPan(val, pX, pY);
			((EventInstance)(ref val)).setParameterByName("cursor_speed", MapBox.cursor_speed.fmod_speed, false);
			if (!isPlaying(val))
			{
				((EventInstance)(ref val)).start();
			}
		}
	}

	public static void setPan(EventInstance pInstance, float pX, float pY)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (pX != -1f || pY != -1f)
		{
			float num = 0f;
			_sound_object.transform.position = new Vector3(pX, pY, num);
			ATTRIBUTES_3D val = RuntimeUtils.To3DAttributes(_sound_object);
			((EventInstance)(ref pInstance)).set3DAttributes(val);
		}
	}

	public void stopDrawingSound(string pID)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (_drawing_sounds.ContainsKey(pID))
		{
			EventInstance pInstance = _drawing_sounds[pID];
			if (isPlaying(pInstance))
			{
				((EventInstance)(ref pInstance)).stop((STOP_MODE)0);
			}
		}
	}

	public void stopDrawingSounds()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		foreach (EventInstance value in _drawing_sounds.Values)
		{
			EventInstance current = value;
			if (isPlaying(current))
			{
				((EventInstance)(ref current)).stop((STOP_MODE)0);
			}
		}
	}

	public static bool isPlaying(EventInstance pInstance)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		PLAYBACK_STATE val = default(PLAYBACK_STATE);
		((EventInstance)(ref pInstance)).getPlaybackState(ref val);
		return (int)val != 2;
	}

	private void drawFmodDebugZones()
	{
	}

	private void countUnitsInZones()
	{
		foreach (MapChunk visibleChunk in World.world.zone_camera.getVisibleChunks())
		{
			if (!visibleChunk.objects.isEmpty())
			{
				countUnits(visibleChunk);
			}
		}
	}

	private void checkCamera()
	{
		if ((_tiles_sand < 50 || _tiles_shallow_water < 50) && _tiles_sand >= 100 && _tiles_shallow_water < 20)
		{
			MusicBoxLibrary.Locations_Desert.container_tiles.amount = _tiles_sand + _tiles_shallow_water;
		}
		_lib.c_list_params.Sort(sorter);
		float num = 0f;
		for (int i = 0; i < _lib.c_list_params.Count; i++)
		{
			MusicBoxContainerTiles musicBoxContainerTiles = _lib.c_list_params[i];
			musicBoxContainerTiles.enabled = false;
			num += (float)musicBoxContainerTiles.amount;
		}
		float num2 = 0f;
		int num3 = 0;
		for (int j = 0; j < _lib.c_list_params.Count; j++)
		{
			MusicBoxContainerTiles musicBoxContainerTiles2 = _lib.c_list_params[j];
			musicBoxContainerTiles2.calculatePan();
			musicBoxContainerTiles2.percent = (float)musicBoxContainerTiles2.amount / num;
			num2 += musicBoxContainerTiles2.percent;
			if (musicBoxContainerTiles2.amount > 50)
			{
				if (num3 >= 2)
				{
					break;
				}
				musicBoxContainerTiles2.enabled = true;
				num3++;
			}
		}
	}

	private void checkUnitsParams()
	{
		MusicBoxContainerUnits musicBoxContainerUnits = null;
		MusicBoxContainerUnits musicBoxContainerUnits2 = null;
		foreach (MusicBoxContainerUnits value in _lib.c_dict_units.Values)
		{
			value.asset.special_delegate_units?.Invoke(value);
			if (value.units > 0)
			{
				if (value.asset.priority == MusicLayerPriority.High)
				{
					musicBoxContainerUnits2 = value;
				}
				else if (value.asset.priority == MusicLayerPriority.Medium)
				{
					musicBoxContainerUnits = value;
				}
			}
		}
		if (musicBoxContainerUnits2 != null)
		{
			musicBoxContainerUnits = null;
		}
		if (musicBoxContainerUnits2 != null || musicBoxContainerUnits != null)
		{
			foreach (MusicBoxContainerUnits value2 in _lib.c_dict_units.Values)
			{
				if ((musicBoxContainerUnits2 == null || value2 != musicBoxContainerUnits2) && (musicBoxContainerUnits == null || value2 != musicBoxContainerUnits))
				{
					value2.units = 0;
				}
			}
		}
		foreach (MusicBoxContainerUnits value3 in _lib.c_dict_units.Values)
		{
			if (value3.units > 0)
			{
				value3.enabled = true;
			}
		}
	}

	public static int sorter(MusicBoxContainerTiles pV1, MusicBoxContainerTiles pV2)
	{
		return pV2.amount.CompareTo(pV1.amount);
	}

	private void countSpecialTilesInChunks()
	{
		List<MapChunk> visibleChunks = World.world.zone_camera.getVisibleChunks();
		int i = 0;
		for (int count = visibleChunks.Count; i < count; i++)
		{
			MapChunk pChunk = visibleChunks[i];
			countSpecialTilesForZone(pChunk);
		}
	}

	private void countSpecialTilesForZone(MapChunk pChunk)
	{
		List<MusicBoxTileData> simpleData = pChunk.getSimpleData();
		TileTypeBase[] array_tiles = TileLibrary.array_tiles;
		int i = 0;
		for (int count = simpleData.Count; i < count; i++)
		{
			MusicBoxTileData musicBoxTileData = simpleData[i];
			TileTypeBase tileTypeBase = array_tiles[musicBoxTileData.tile_type_id];
			int amount = musicBoxTileData.amount;
			if (amount == 0)
			{
				continue;
			}
			List<MusicAsset> music_assets = tileTypeBase.music_assets;
			if (music_assets != null)
			{
				int j = 0;
				for (int count2 = music_assets.Count; j < count2; j++)
				{
					music_assets[j].container_tiles.count(amount, pChunk.world_center_x, pChunk.world_center_y);
				}
			}
		}
	}

	private void countUnits(MapChunk pChunk)
	{
		foreach (long kingdom2 in pChunk.objects.kingdoms)
		{
			Kingdom kingdom = World.world.kingdoms.get(kingdom2);
			if (kingdom != null)
			{
				ActorAsset actorAsset = kingdom.getActorAsset();
				if (actorAsset != null && actorAsset.has_music_theme)
				{
					_lib.c_dict_units[actorAsset.music_theme].units++;
				}
			}
		}
	}

	private void enableMusicParameter(string pID)
	{
		setMusicParameter(pID, 1f);
	}

	private void disableMusicParameter(string pID)
	{
		setMusicParameter(pID, 0f);
	}

	private void setMusicParameter(string pID, float pValue)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		((EventInstance)(ref _music_event)).setParameterByName(pID, pValue, false);
	}

	private void markerReached(string pMarker)
	{
		if (pMarker == "Intro")
		{
			return;
		}
		MusicAsset musicAsset = _lib.get(pMarker);
		if (musicAsset != null)
		{
			if (musicAsset.disable_param_after_start)
			{
				disableMusicParameter(pMarker);
			}
			if (musicAsset.action != null)
			{
				musicAsset.action();
			}
		}
	}

	public static void reserveFlag(string pID, bool pValue = true)
	{
		if (music_on)
		{
			inst._timer = -1f;
			inst._flags_to_enable.Add(pID);
		}
	}

	public unsafe static void debug_fmod(DebugTool pTool)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		if (!fmod_disabled)
		{
			System studio_system = _studio_system;
			Bank[] array = default(Bank[]);
			((System)(ref studio_system)).getBankList(ref array);
			studio_system = _studio_system;
			EventDescription val2 = default(EventDescription);
			RESULT val = ((System)(ref studio_system)).getEvent("event:/MUSIC/ConsolidatedMusicEvent", ref val2);
			int num = -1;
			float num2 = -1f;
			PLAYBACK_STATE val3 = (PLAYBACK_STATE)3;
			((EventInstance)(ref inst._music_event)).getParameterByName("new_world", ref num2);
			((EventInstance)(ref inst._music_event)).getTimelinePosition(ref num);
			((EventInstance)(ref inst._music_event)).getPlaybackState(ref val3);
			pTool.setText("Zoom_Low:", World.world.quality_changer.getZoomRatioLow(), 0f, pShowBar: false, 0L);
			pTool.setText("Zoom_High:", World.world.quality_changer.getZoomRatioHigh(), 0f, pShowBar: false, 0L);
			pTool.setText("Zoom_Full:", World.world.quality_changer.getZoomRatioFull(), 0f, pShowBar: false, 0L);
			pTool.setSeparator();
			pTool.setText("idle_sim_objects:", inst.idle.CountCurrentSounds(), 0f, pShowBar: false, 0L);
			pTool.setText("music state:", inst.music_state, 0f, pShowBar: false, 0L);
			pTool.setText("IsInitialized:", RuntimeManager.IsInitialized, 0f, pShowBar: false, 0L);
			pTool.setText("Banks count:", array.Length, 0f, pShowBar: false, 0L);
			pTool.setText("AnySampleDataLoading:", RuntimeManager.AnySampleDataLoading(), 0f, pShowBar: false, 0L);
			pTool.setText("Bank Master:", RuntimeManager.HasBankLoaded("Master"), 0f, pShowBar: false, 0L);
			pTool.setText("Bank Master.strings:", RuntimeManager.HasBankLoaded("Master.strings"), 0f, pShowBar: false, 0L);
			pTool.setText("MUSIC_EVENT by name:", ((object)(*(RESULT*)(&val))/*cast due to constrained. prefix*/).ToString(), 0f, pShowBar: false, 0L);
			pTool.setText("tParam_new_world:", num2, 0f, pShowBar: false, 0L);
			pTool.setText("tTimelinePos:", num, 0f, pShowBar: false, 0L);
			pTool.setText("getPlaybackState:", ((object)(*(PLAYBACK_STATE*)(&val3))/*cast due to constrained. prefix*/).ToString(), 0f, pShowBar: false, 0L);
		}
	}

	public void debug_params(DebugTool pTool)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (fmod_disabled)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < _lib.list.Count; i++)
		{
			string id = _lib.list[i].id;
			((EventInstance)(ref inst._music_event)).getParameterByName(id, ref num);
			if (num == 1f)
			{
				pTool.setText(id + ":", num, 0f, pShowBar: false, 0L);
			}
		}
	}

	public void debug_world_params(DebugTool pTool)
	{
		if (fmod_disabled)
		{
			return;
		}
		foreach (MusicBoxContainerCivs value in _lib.c_dict_civs.Values)
		{
			if (value.active)
			{
				pTool.setText(value.asset.id, value.buildings + " " + value.kingdom_exists + " " + value.active, 0f, pShowBar: false, 0L);
			}
		}
		foreach (MusicAsset item in _lib.list)
		{
			MusicBoxContainerTiles container_tiles = item.container_tiles;
			if (container_tiles != null && container_tiles.enabled)
			{
				pTool.setText(container_tiles.asset.id, container_tiles.amount + " " + container_tiles.enabled + " " + container_tiles.percent.ToText() + "%", 0f, pShowBar: false, 0L);
			}
		}
		pTool.setText("", "", 0f, pShowBar: false, 0L);
	}

	public void debug_unit_params(DebugTool pTool)
	{
		if (fmod_disabled || _lib.c_dict_units.Count == 0)
		{
			return;
		}
		foreach (MusicBoxContainerUnits value in _lib.c_dict_units.Values)
		{
			if (value.units != 0)
			{
				pTool.setText(value.asset.id, value.units + " " + value.enabled, 0f, pShowBar: false, 0L);
			}
		}
		pTool.setText("", "", 0f, pShowBar: false, 0L);
	}
}
