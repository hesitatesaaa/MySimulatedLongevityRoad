using System;
using UnityEngine;

[Serializable]
public class ContainerItemColor
{
	public string color_id;

	public Color color;

	private Material material;

	private string path_material;

	public ContainerItemColor(string pID, string pMaterialPath)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		color = Toolbox.makeColor(pID);
		color_id = pID;
		path_material = pMaterialPath;
	}

	public Material getMaterial()
	{
		if (string.IsNullOrEmpty(path_material))
		{
			return null;
		}
		Material val = Resources.Load<Material>(path_material);
		material = val;
		return material;
	}
}
