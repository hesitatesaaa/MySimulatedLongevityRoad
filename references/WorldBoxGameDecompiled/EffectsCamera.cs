using UnityEngine;

public class EffectsCamera : MonoBehaviour
{
	private Camera _mainCamera;

	private Camera _effectsCamera;

	internal RenderTexture renderTexture;

	private void Awake()
	{
		_effectsCamera = ((Component)this).GetComponent<Camera>();
	}

	private void Start()
	{
		_mainCamera = World.world.camera;
	}

	private void LateUpdate()
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Expected O, but got Unknown
		_effectsCamera.orthographicSize = _mainCamera.orthographicSize;
		int num = Screen.width / 3;
		int num2 = Screen.height / 3;
		if ((Object)(object)renderTexture == (Object)null || ((Texture)renderTexture).width != num || ((Texture)renderTexture).height != num2)
		{
			renderTexture = new RenderTexture(num, num2, 0);
			((Texture)renderTexture).filterMode = (FilterMode)0;
			((Texture)renderTexture).wrapMode = (TextureWrapMode)1;
			_effectsCamera.targetTexture = renderTexture;
		}
	}
}
