using UnityEngine;

public class CameraRender : MonoBehaviour
{
	public Material PostProcessMaterial;

	public Camera BackgroundCamera;

	public Camera MainCamera;

	private RenderTexture mainRenderTexture;

	private void Start()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Expected O, but got Unknown
		mainRenderTexture = new RenderTexture(Screen.width, Screen.height, 16, (RenderTextureFormat)0);
		mainRenderTexture.Create();
		BackgroundCamera.targetTexture = mainRenderTexture;
		MainCamera.targetTexture = mainRenderTexture;
	}

	private void Update()
	{
	}

	private void OnPostRender()
	{
		Graphics.Blit((Texture)(object)mainRenderTexture, PostProcessMaterial);
	}
}
