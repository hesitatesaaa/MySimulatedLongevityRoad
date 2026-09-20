using System;
using System.Runtime.InteropServices;

public class PreciseLocale
{
	private interface PlatformBridge
	{
		string GetRegion();

		string GetLanguage();

		string GetLanguageID();

		string GetCurrencyCode();

		string GetCurrencySymbol();
	}

	private class EditorBridge : PlatformBridge
	{
		public string GetRegion()
		{
			return "US";
		}

		public string GetLanguage()
		{
			return "en";
		}

		public string GetLanguageID()
		{
			return "en_US";
		}

		public string GetCurrencyCode()
		{
			return "USD";
		}

		public string GetCurrencySymbol()
		{
			return "$";
		}
	}

	private class PreciseLocaleWindows : PlatformBridge
	{
		[DllImport("PreciseLocale")]
		private static extern IntPtr _getLanguage();

		[DllImport("PreciseLocale")]
		private static extern IntPtr _getRegion();

		[DllImport("PreciseLocale")]
		private static extern IntPtr _getCurrencyCode();

		[DllImport("PreciseLocale")]
		private static extern IntPtr _getCurrencySymbol();

		public string GetLanguage()
		{
			return Marshal.PtrToStringUni(_getLanguage());
		}

		public string GetLanguageID()
		{
			return GetLanguage() + "_" + GetRegion();
		}

		public string GetRegion()
		{
			return Marshal.PtrToStringUni(_getRegion());
		}

		public string GetCurrencyCode()
		{
			return Marshal.PtrToStringUni(_getCurrencyCode());
		}

		public string GetCurrencySymbol()
		{
			return Marshal.PtrToStringUni(_getCurrencySymbol());
		}
	}

	private static PlatformBridge _platform;

	private static PlatformBridge platform
	{
		get
		{
			if (_platform == null)
			{
				_platform = new PreciseLocaleWindows();
			}
			return _platform;
		}
	}

	public static string GetRegion()
	{
		return platform.GetRegion();
	}

	public static string GetLanguageID()
	{
		return platform.GetLanguageID();
	}

	public static string GetLanguage()
	{
		return platform.GetLanguage();
	}

	public static string GetCurrencyCode()
	{
		return platform.GetCurrencyCode();
	}

	public static string GetCurrencySymbol()
	{
		return platform.GetCurrencySymbol();
	}
}
