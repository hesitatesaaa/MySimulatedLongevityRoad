using UnityEngine;

public static class TextureExtensions
{
	public static Texture2D getAsReadable(this Texture2D pSourceTexture)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected O, but got Unknown
		RenderTexture active = RenderTexture.active;
		RenderTexture temporary = RenderTexture.GetTemporary(((Texture)pSourceTexture).width, ((Texture)pSourceTexture).height, 0, (RenderTextureFormat)7, (RenderTextureReadWrite)((!((Texture)pSourceTexture).isDataSRGB) ? 1 : 2));
		Graphics.Blit((Texture)(object)pSourceTexture, temporary);
		RenderTexture.active = temporary;
		Texture2D val = new Texture2D(((Texture)pSourceTexture).width, ((Texture)pSourceTexture).height);
		val.ReadPixels(new Rect(0f, 0f, (float)((Texture)temporary).width, (float)((Texture)temporary).height), 0, 0);
		val.Apply();
		RenderTexture.active = active;
		RenderTexture.ReleaseTemporary(temporary);
		return val;
	}
}
