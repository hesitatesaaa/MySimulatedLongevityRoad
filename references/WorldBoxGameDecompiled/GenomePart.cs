using System;

public readonly struct GenomePart : IEquatable<GenomePart>
{
	public readonly string id;

	public readonly float value;

	public GenomePart(string id, float pValue)
	{
		if (string.IsNullOrEmpty(id))
		{
			throw new ArgumentNullException("id cannot be null or empty");
		}
		this.id = id;
		value = pValue;
	}

	public override bool Equals(object pObject)
	{
		if (pObject is GenomePart pOther)
		{
			return Equals(pOther);
		}
		return false;
	}

	public bool Equals(GenomePart pOther)
	{
		return string.Equals(id, pOther.id, StringComparison.OrdinalIgnoreCase);
	}

	public override int GetHashCode()
	{
		return StringComparer.OrdinalIgnoreCase.GetHashCode(id);
	}

	public override string ToString()
	{
		return $"{id}: {value}";
	}
}
