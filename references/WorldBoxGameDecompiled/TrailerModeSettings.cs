using System;
using System.IO;
using UnityEngine;

[Serializable]
public class TrailerModeSettings
{
	public bool cityUseResources = true;

	public bool sonicSpeed = true;

	public bool fastSpawn = true;

	public float cameraMoveSpeed = 0.001f;

	public float cameraMoveMax = 0.02f;

	public float cameraZoomSpeed = 3.8f;

	public bool superOrcs = true;

	public static void startEvent()
	{
		string path = Application.persistentDataPath + "/trailer_settings";
		TrailerModeSettings trailerModeSettings;
		if (!File.Exists(path))
		{
			trailerModeSettings = new TrailerModeSettings();
			string text = JsonUtility.ToJson((object)trailerModeSettings);
			text = text.Replace(",", ",\n");
			text = text.Replace("{", "{\n");
			text = text.Replace("}", "\n}");
			File.WriteAllText(path, text);
		}
		else
		{
			trailerModeSettings = JsonUtility.FromJson<TrailerModeSettings>(File.ReadAllText(path));
		}
		trailerModeSettings.applyTrailerSettings();
	}

	public void applyTrailerSettings()
	{
		if (superOrcs)
		{
			AssetManager.actor_library.get("unit_orc").base_stats["damage"] = 10000f;
		}
		else
		{
			AssetManager.actor_library.get("unit_orc").base_stats["damage"] = 18f;
		}
		DebugConfig.setOption(DebugOption.FastSpawn, fastSpawn);
		DebugConfig.setOption(DebugOption.SonicSpeed, sonicSpeed);
		World.world.move_camera.camera_move_speed = cameraMoveSpeed;
		World.world.move_camera.camera_move_max = cameraMoveMax;
		World.world.move_camera.camera_zoom_speed = cameraZoomSpeed;
		Globals.TRAILER_MODE_USE_RESOURCES = cityUseResources;
	}
}
