using System;
using System.Collections.Generic;
using System.ComponentModel;

[Serializable]
public class ChromosomeData
{
	public List<string> loci = new List<string>();

	public List<int> super_loci = new List<int>();

	public List<int> void_loci = new List<int>();

	[DefaultValue("chromosome_medium")]
	public string chromosome_type = "chromosome_medium";
}
