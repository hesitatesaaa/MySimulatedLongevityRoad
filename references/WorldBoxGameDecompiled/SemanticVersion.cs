using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public sealed class SemanticVersion : IComparable, IComparable<SemanticVersion>, IEquatable<SemanticVersion>
{
	private const RegexOptions _flags = RegexOptions.ExplicitCapture | RegexOptions.Compiled;

	private static readonly Regex _semanticVersionRegex = new Regex("^(?<Version>\\d+(\\s*\\.\\s*\\d+){0,3})(?<Release>-([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+(\\.([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+)*)?(?<Metadata>\\+[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	private static readonly Regex _strictSemanticVersionRegex = new Regex("^(?<Version>([0-9]|[1-9][0-9]*)(\\.([0-9]|[1-9][0-9]*)){2})(?<Release>-([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+(\\.([0]\\b|[0]$|[0][0-9]*[A-Za-z-]+|[1-9A-Za-z-][0-9A-Za-z-]*)+)*)?(?<Metadata>\\+[0-9A-Za-z-]+(\\.[0-9A-Za-z-]+)*)?$", RegexOptions.ExplicitCapture | RegexOptions.Compiled);

	private readonly string _originalString;

	private string _normalizedVersionString;

	public Version Version { get; private set; }

	public string SpecialVersion { get; private set; }

	public string Metadata { get; private set; }

	public SemanticVersion(string version)
		: this(Parse(version))
	{
		_originalString = version;
	}

	public SemanticVersion(int major, int minor, int build, int revision)
		: this(new Version(major, minor, build, revision))
	{
	}

	public SemanticVersion(int major, int minor, int build, string specialVersion)
		: this(new Version(major, minor, build), specialVersion)
	{
	}

	public SemanticVersion(int major, int minor, int build, string specialVersion, string metadata)
		: this(new Version(major, minor, build), specialVersion, metadata)
	{
	}

	public SemanticVersion(Version version)
		: this(version, string.Empty)
	{
	}

	public SemanticVersion(Version version, string specialVersion)
		: this(version, specialVersion, null, null)
	{
	}

	public SemanticVersion(Version version, string specialVersion, string metadata)
		: this(version, specialVersion, metadata, null)
	{
	}

	private SemanticVersion(Version version, string specialVersion, string metadata, string originalString)
	{
		if (version == null)
		{
			throw new ArgumentNullException("version");
		}
		Version = NormalizeVersionValue(version);
		SpecialVersion = specialVersion ?? string.Empty;
		Metadata = metadata;
		_originalString = (string.IsNullOrEmpty(originalString) ? (version.ToString() + ((!string.IsNullOrEmpty(specialVersion)) ? ("-" + specialVersion) : null) + ((!string.IsNullOrEmpty(metadata)) ? ("+" + metadata) : null)) : originalString);
	}

	internal SemanticVersion(SemanticVersion semVer)
	{
		_originalString = semVer.ToOriginalString();
		Version = semVer.Version;
		SpecialVersion = semVer.SpecialVersion;
		Metadata = semVer.Metadata;
	}

	public string[] GetOriginalVersionComponents()
	{
		if (!string.IsNullOrEmpty(_originalString))
		{
			int num = _originalString.IndexOfAny(new char[2] { '-', '+' });
			string version = ((num == -1) ? _originalString : _originalString.Substring(0, num));
			return SplitAndPadVersionString(version);
		}
		return SplitAndPadVersionString(Version.ToString());
	}

	private static string[] SplitAndPadVersionString(string version)
	{
		string[] array = version.Split('.');
		if (array.Length == 4)
		{
			return array;
		}
		string[] array2 = new string[4] { "0", "0", "0", "0" };
		Array.Copy(array, 0, array2, 0, array.Length);
		return array2;
	}

	public static SemanticVersion Parse(string version)
	{
		if (string.IsNullOrEmpty(version))
		{
			throw new ArgumentException("Value cannot be null or an empty string", "version");
		}
		if (!TryParse(version, out var value))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "'{0}' is not a valid version string.", version), "version");
		}
		return value;
	}

	public static bool TryParse(string version, out SemanticVersion value)
	{
		return TryParseInternal(version, _semanticVersionRegex, out value);
	}

	public static bool TryParseStrict(string version, out SemanticVersion value)
	{
		return TryParseInternal(version, _strictSemanticVersionRegex, out value);
	}

	private static bool TryParseInternal(string version, Regex regex, out SemanticVersion semVer)
	{
		semVer = null;
		if (string.IsNullOrEmpty(version))
		{
			return false;
		}
		Match match = regex.Match(version.Trim());
		if (!match.Success || !Version.TryParse(match.Groups["Version"].Value, out Version result))
		{
			return false;
		}
		semVer = new SemanticVersion(NormalizeVersionValue(result), RemoveLeadingChar(match.Groups["Release"].Value), RemoveLeadingChar(match.Groups["Metadata"].Value), version.Replace(" ", ""));
		return true;
	}

	private static string RemoveLeadingChar(string s)
	{
		if (s != null && s.Length > 0)
		{
			return s.Substring(1, s.Length - 1);
		}
		return s;
	}

	public static SemanticVersion ParseOptionalVersion(string version)
	{
		TryParse(version, out var value);
		return value;
	}

	private static Version NormalizeVersionValue(Version version)
	{
		return new Version(version.Major, version.Minor, Math.Max(version.Build, 0), Math.Max(version.Revision, 0));
	}

	public int CompareTo(object obj)
	{
		if (obj == null)
		{
			return 1;
		}
		SemanticVersion semanticVersion = obj as SemanticVersion;
		if (semanticVersion == null)
		{
			throw new ArgumentException("Type to compare must be an instance of SemanticVersion.", "obj");
		}
		return CompareTo(semanticVersion);
	}

	public int CompareTo(SemanticVersion other)
	{
		if ((object)other == null)
		{
			return 1;
		}
		int num = Version.CompareTo(other.Version);
		if (num != 0)
		{
			return num;
		}
		bool flag = string.IsNullOrEmpty(SpecialVersion);
		bool flag2 = string.IsNullOrEmpty(other.SpecialVersion);
		if (flag & flag2)
		{
			return 0;
		}
		if (flag)
		{
			return 1;
		}
		if (flag2)
		{
			return -1;
		}
		string[] version = SpecialVersion.Split('.');
		string[] version2 = other.SpecialVersion.Split('.');
		return CompareReleaseLabels(version, version2);
	}

	public static bool operator ==(SemanticVersion version1, SemanticVersion version2)
	{
		return version1?.Equals(version2) ?? ((object)version2 == null);
	}

	public static bool operator !=(SemanticVersion version1, SemanticVersion version2)
	{
		return !(version1 == version2);
	}

	public static bool operator <(SemanticVersion version1, SemanticVersion version2)
	{
		if (version1 == null)
		{
			throw new ArgumentNullException("version1");
		}
		return version1.CompareTo(version2) < 0;
	}

	public static bool operator <=(SemanticVersion version1, SemanticVersion version2)
	{
		if (!(version1 == version2))
		{
			return version1 < version2;
		}
		return true;
	}

	public static bool operator >(SemanticVersion version1, SemanticVersion version2)
	{
		if (version1 == null)
		{
			throw new ArgumentNullException("version1");
		}
		return version2 < version1;
	}

	public static bool operator >=(SemanticVersion version1, SemanticVersion version2)
	{
		if (!(version1 == version2))
		{
			return version1 > version2;
		}
		return true;
	}

	public override string ToString()
	{
		if (IsSemVer2())
		{
			return ToNormalizedString();
		}
		int num = _originalString.IndexOf('+');
		if (num > -1)
		{
			return _originalString.Substring(0, num);
		}
		return _originalString;
	}

	public string ToNormalizedString()
	{
		if (_normalizedVersionString == null)
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Version.Major).Append('.').Append(Version.Minor)
				.Append('.')
				.Append(Math.Max(0, Version.Build));
			if (Version.Revision > 0)
			{
				stringBuilder.Append('.').Append(Version.Revision);
			}
			if (!string.IsNullOrEmpty(SpecialVersion))
			{
				stringBuilder.Append('-').Append(SpecialVersion);
			}
			_normalizedVersionString = stringBuilder.ToString();
		}
		return _normalizedVersionString;
	}

	public string ToFullString()
	{
		string text = ToNormalizedString();
		if (!string.IsNullOrEmpty(Metadata))
		{
			text = string.Format(CultureInfo.InvariantCulture, "{0}+{1}", text, Metadata);
		}
		return text;
	}

	public string ToOriginalString()
	{
		return _originalString;
	}

	public bool IsSemVer2()
	{
		if (string.IsNullOrEmpty(Metadata))
		{
			if (!string.IsNullOrEmpty(SpecialVersion))
			{
				return SpecialVersion.Contains(".");
			}
			return false;
		}
		return true;
	}

	public bool Equals(SemanticVersion other)
	{
		if ((object)other != null && Version.Equals(other.Version))
		{
			return SpecialVersion.Equals(other.SpecialVersion, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is SemanticVersion other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		int num = Version.GetHashCode();
		if (SpecialVersion != null)
		{
			num = num * 4567 + SpecialVersion.GetHashCode();
		}
		return num;
	}

	private static int CompareReleaseLabels(IEnumerable<string> version1, IEnumerable<string> version2)
	{
		int num = 0;
		using IEnumerator<string> enumerator = version1.GetEnumerator();
		using IEnumerator<string> enumerator2 = version2.GetEnumerator();
		bool flag = enumerator.MoveNext();
		bool flag2 = enumerator2.MoveNext();
		while (flag | flag2)
		{
			if (!flag & flag2)
			{
				return -1;
			}
			if (flag && !flag2)
			{
				return 1;
			}
			num = CompareRelease(enumerator.Current, enumerator2.Current);
			if (num != 0)
			{
				return num;
			}
			flag = enumerator.MoveNext();
			flag2 = enumerator2.MoveNext();
		}
		return num;
	}

	private static int CompareRelease(string version1, string version2)
	{
		int result = 0;
		int result2 = 0;
		int num = 0;
		bool flag = int.TryParse(version1, out result);
		bool flag2 = int.TryParse(version2, out result2);
		if (flag & flag2)
		{
			return result.CompareTo(result2);
		}
		if (flag | flag2)
		{
			if (flag)
			{
				return -1;
			}
			return 1;
		}
		return StringComparer.OrdinalIgnoreCase.Compare(version1, version2);
	}
}
