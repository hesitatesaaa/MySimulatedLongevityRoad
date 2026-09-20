using System;
using UnityEngine;

public static class PixelDetector
{
	public static bool GetSpritePixelColorUnderMousePointer(MonoBehaviour mono, out Vector2Int pVector)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		pVector = new Vector2Int(-1, -1);
		Vector2 val = Vector2.op_Implicit(Input.mousePosition);
		Vector2 val2 = Vector2.op_Implicit(Camera.main.ScreenToViewportPoint(Vector2.op_Implicit(val)));
		if (val2.x <= 0f || val2.x >= 1f || val2.y <= 0f || val2.y >= 1f)
		{
			return false;
		}
		Ray ray;
		try
		{
			ray = Camera.main.ViewportPointToRay(Vector2.op_Implicit(val2));
		}
		catch (Exception)
		{
			return false;
		}
		return IntersectsSprite(mono, ray, out pVector);
	}

	private static bool IntersectsSprite(MonoBehaviour mono, Ray ray, out Vector2Int pVector)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		SpriteRenderer component = ((Component)mono).GetComponent<SpriteRenderer>();
		pVector = new Vector2Int(-1, -1);
		if ((Object)(object)component == (Object)null)
		{
			return false;
		}
		Sprite sprite = component.sprite;
		if ((Object)(object)sprite == (Object)null)
		{
			return false;
		}
		Texture2D texture = sprite.texture;
		if ((Object)(object)texture == (Object)null)
		{
			return false;
		}
		if (sprite.packed && (int)sprite.packingMode == 0)
		{
			Debug.LogError((object)"SpritePackingMode.Tight atlas packing is not supported!");
			return false;
		}
		Plane val = default(Plane);
		((Plane)(ref val))._002Ector(((Component)mono).transform.forward, ((Component)mono).transform.position);
		float num = default(float);
		if (!((Plane)(ref val)).Raycast(ray, ref num))
		{
			return false;
		}
		Matrix4x4 worldToLocalMatrix = ((Renderer)component).worldToLocalMatrix;
		Vector3 val2 = ((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(((Ray)(ref ray)).origin + ((Ray)(ref ray)).direction * num);
		Rect textureRect = sprite.textureRect;
		float pixelsPerUnit = sprite.pixelsPerUnit;
		float num2 = (float)((Texture)texture).width * 0f;
		float num3 = (float)((Texture)texture).height * 0f;
		int num4 = (int)(val2.x * pixelsPerUnit + num2);
		int num5 = (int)(val2.y * pixelsPerUnit + num3);
		if (num4 < 0 || (float)num4 < ((Rect)(ref textureRect)).x || num4 >= Mathf.FloorToInt(((Rect)(ref textureRect)).xMax))
		{
			return false;
		}
		if (num5 < 0 || (float)num5 < ((Rect)(ref textureRect)).y || num5 >= Mathf.FloorToInt(((Rect)(ref textureRect)).yMax))
		{
			return false;
		}
		pVector = new Vector2Int(num4, num5);
		return true;
	}
}
