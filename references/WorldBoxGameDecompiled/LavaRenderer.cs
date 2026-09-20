using UnityEngine;
using UnityEngine.Rendering;

public class LavaRenderer : MonoBehaviour
{
	public Camera curCamera;

	public Camera targetCamera;

	private RenderTexture renderTexture;

	private void Start()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		renderTexture = new RenderTexture(Screen.width, Screen.height, 8, (RenderTextureFormat)0);
		((Texture)renderTexture).dimension = (TextureDimension)2;
		renderTexture.antiAliasing = 1;
		((Texture)renderTexture).anisoLevel = 0;
		((Texture)renderTexture).filterMode = (FilterMode)0;
		renderTexture.Create();
		curCamera.targetTexture = renderTexture;
	}

	private void OnPreRender()
	{
		targetCamera.targetTexture = renderTexture;
	}

	private void OnPostRender()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		targetCamera.targetTexture = null;
		Graphics.DrawTexture(new Rect(0f, 0f, (float)(Screen.width / 2), (float)(Screen.height / 2)), (Texture)(object)renderTexture, (Material)null);
	}
}
