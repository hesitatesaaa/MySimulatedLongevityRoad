using UnityEngine;

public class TextureScale
{
	public class ThreadData
	{
		public int start;

		public int end;

		public ThreadData(int s, int e)
		{
			start = s;
			end = e;
		}
	}

	private static Color[] texColors;

	private static Color[] newColors;

	private static int w;

	private static float ratioX;

	private static float ratioY;

	private static int w2;

	private static int finishCount;

	public static void Point(Texture2D tex, int newWidth, int newHeight)
	{
		ThreadedScale(tex, newWidth, newHeight, useBilinear: false);
	}

	public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
	{
		ThreadedScale(tex, newWidth, newHeight, useBilinear: true);
	}

	private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
	{
		texColors = tex.GetPixels();
		newColors = (Color[])(object)new Color[newWidth * newHeight];
		if (useBilinear)
		{
			ratioX = 1f / ((float)newWidth / (float)(((Texture)tex).width - 1));
			ratioY = 1f / ((float)newHeight / (float)(((Texture)tex).height - 1));
		}
		else
		{
			ratioX = (float)((Texture)tex).width / (float)newWidth;
			ratioY = (float)((Texture)tex).height / (float)newHeight;
		}
		w = ((Texture)tex).width;
		w2 = newWidth;
		int num = Mathf.Min(SystemInfo.processorCount, newHeight);
		_ = newHeight / num;
		finishCount = 0;
		ThreadData obj = new ThreadData(0, newHeight);
		if (useBilinear)
		{
			BilinearScale(obj);
		}
		else
		{
			PointScale(obj);
		}
		tex.Reinitialize(newWidth, newHeight);
		tex.SetPixels(newColors);
		tex.Apply();
		texColors = null;
		newColors = null;
	}

	public static void BilinearScale(object obj)
	{
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		ThreadData threadData = (ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)Mathf.Floor((float)i * ratioY);
			int num2 = num * w;
			int num3 = (num + 1) * w;
			int num4 = i * w2;
			for (int j = 0; j < w2; j++)
			{
				int num5 = (int)Mathf.Floor((float)j * ratioX);
				float value = (float)j * ratioX - (float)num5;
				newColors[num4 + j] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[num2 + num5], texColors[num2 + num5 + 1], value), ColorLerpUnclamped(texColors[num3 + num5], texColors[num3 + num5 + 1], value), (float)i * ratioY - (float)num);
			}
		}
		finishCount++;
	}

	public static void PointScale(object obj)
	{
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		ThreadData threadData = (ThreadData)obj;
		for (int i = threadData.start; i < threadData.end; i++)
		{
			int num = (int)(ratioY * (float)i) * w;
			int num2 = i * w2;
			for (int j = 0; j < w2; j++)
			{
				newColors[num2 + j] = texColors[(int)((float)num + ratioX * (float)j)];
			}
		}
		finishCount++;
	}

	private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		return new Color(c1.r + (c2.r - c1.r) * value, c1.g + (c2.g - c1.g) * value, c1.b + (c2.b - c1.b) * value, c1.a + (c2.a - c1.a) * value);
	}
}
