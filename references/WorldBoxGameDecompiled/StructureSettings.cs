using System;

[Serializable]
public class StructureSettings
{
	public bool[] enabled = new bool[7];

	public string[][] sets = new string[7][];

	public string[] separator = new string[7];

	public virtual void create(LanguageStructure pStructure, int pSizeMin, int pSizeMax)
	{
	}
}
