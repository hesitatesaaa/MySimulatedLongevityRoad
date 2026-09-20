using System.Collections.Generic;
using UnityEngine;

public class PrintLibrary : MonoBehaviour
{
	private readonly Color _color_0;

	private readonly Color _color_1;

	private readonly Color _color_2;

	private readonly Color _color_3;

	public List<PrintTemplate> list;

	private readonly Dictionary<string, PrintTemplate> _dict;

	private readonly List<PrintTemplate> _list_quakes;

	private static PrintLibrary _instance;

	private void Awake()
	{
		_instance = this;
		for (int i = 0; i < list.Count; i++)
		{
			PrintTemplate printTemplate = list[i];
			calcSteps(printTemplate);
			_dict.Add(printTemplate.name, printTemplate);
			if (printTemplate.name.Contains("quake"))
			{
				_list_quakes.Add(printTemplate);
				addRotatedQuake(printTemplate, 90);
				addRotatedQuake(printTemplate, 180);
				addRotatedQuake(printTemplate, 360);
				addRotatedQuake(printTemplate, -360);
				addRotatedQuake(printTemplate, -90);
				addRotatedQuake(printTemplate, -180);
				addRotatedQuake(printTemplate, -270);
			}
		}
	}

	private void addRotatedQuake(PrintTemplate pOrigin, int pRotation)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		PrintTemplate printTemplate = new PrintTemplate();
		printTemplate.name = pOrigin.name + "_" + pRotation;
		Texture2D originTexture = Object.Instantiate<Texture2D>(pOrigin.graphics);
		printTemplate.graphics = TextureRotator.Rotate(originTexture, pRotation, new Color32((byte)0, (byte)0, (byte)0, (byte)0));
		calcSteps(printTemplate);
		_list_quakes.Add(printTemplate);
	}

	private void calcSteps(PrintTemplate pPrint)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		List<PrintStep> list = new List<PrintStep>();
		int width = ((Texture)pPrint.graphics).width;
		int height = ((Texture)pPrint.graphics).height;
		for (int i = 1; i < width - 1; i++)
		{
			for (int j = 1; j < height - 1; j++)
			{
				Color pixel = pPrint.graphics.GetPixel(i, j);
				if (!(pixel == _color_0))
				{
					PrintStep item = new PrintStep
					{
						x = i - 1 - width / 2,
						y = j - 1 - height / 2,
						action = 1
					};
					list.Add(item);
					if (pixel == _color_2)
					{
						list.Add(item);
					}
					else if (pixel == _color_3)
					{
						list.Add(item);
						list.Add(item);
					}
				}
			}
		}
		pPrint.steps = list.ToArray();
		pPrint.steps_per_tick = (int)((float)pPrint.steps.Length * 0.005f + 1f);
	}

	public static PrintTemplate getTemplate(string pTemplateID)
	{
		return _instance._dict[pTemplateID];
	}

	public static List<PrintTemplate> getQuakes()
	{
		return _instance._list_quakes;
	}

	public PrintLibrary()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		_color_0 = Toolbox.makeColor("#FFFFFF");
		_color_1 = Toolbox.makeColor("#CCCCCC");
		_color_2 = Toolbox.makeColor("#7F7F7F");
		_color_3 = Toolbox.makeColor("#000000");
		_dict = new Dictionary<string, PrintTemplate>();
		_list_quakes = new List<PrintTemplate>();
		((MonoBehaviour)this)._002Ector();
	}
}
