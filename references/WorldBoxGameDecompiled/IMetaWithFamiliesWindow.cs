using System.Collections.Generic;

public interface IMetaWithFamiliesWindow
{
	IEnumerable<Family> getFamilies();

	bool hasFamilies();
}
