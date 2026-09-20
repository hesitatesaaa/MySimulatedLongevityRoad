using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;

public static class BlacklistTools
{
	private static string[] _profanities;

	public static string[] getProfanities()
	{
		if (_profanities != null)
		{
			return _profanities;
		}
		Object obj = Resources.Load("blacklisted_names");
		Object obj2 = ((obj is TextAsset) ? obj : null);
		string text = ((TextAsset)obj2).text;
		Resources.UnloadAsset(obj2);
		string[] array = Regex.Split(text, "\r\n?|\n", RegexOptions.Singleline);
		using ListPool<string> listPool = new ListPool<string>(array.Length);
		for (int i = 0; i < array.Length; i++)
		{
			string text2 = array[i].Trim().ToLower();
			if (text2.Length != 0)
			{
				listPool.Add(text2);
			}
		}
		_profanities = listPool.ToArray();
		return _profanities;
	}

	public static void loadProfanityFilter(Dictionary<char, string[]> pProfanity, HashSet<char> pUnique)
	{
		if (pProfanity != null && pProfanity.Count > 0)
		{
			return;
		}
		try
		{
			Dictionary<char, List<string>> dictionary = new Dictionary<char, List<string>>();
			string[] profanities = getProfanities();
			foreach (string text in profanities)
			{
				pUnique.Clear();
				pUnique.UnionWith(text);
				pUnique.RemoveWhere((char pChar) => !char.IsLetter(pChar));
				foreach (char item in pUnique)
				{
					if (!dictionary.ContainsKey(item))
					{
						dictionary[item] = new List<string>();
					}
					dictionary[item].Add(text);
				}
			}
			foreach (char key in dictionary.Keys)
			{
				pProfanity[key] = dictionary[key].ToArray();
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Error when loading blacklist");
			Debug.LogError((object)ex);
		}
	}

	public static void loadProfanityFilter(Dictionary<char, char[][]> pProfanity, HashSet<char> pUnique)
	{
		if (pProfanity != null && pProfanity.Count > 0)
		{
			return;
		}
		try
		{
			Dictionary<char, List<char[]>> dictionary = new Dictionary<char, List<char[]>>();
			string[] profanities = getProfanities();
			foreach (string text in profanities)
			{
				pUnique.Clear();
				pUnique.UnionWith(text);
				pUnique.RemoveWhere((char pChar) => !char.IsLetter(pChar));
				char[] item = text.ToCharArray();
				foreach (char item2 in pUnique)
				{
					if (!dictionary.ContainsKey(item2))
					{
						dictionary[item2] = new List<char[]>();
					}
					dictionary[item2].Add(item);
				}
			}
			foreach (char key in dictionary.Keys)
			{
				pProfanity[key] = dictionary[key].ToArray();
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Error when loading blacklist");
			Debug.LogError((object)ex);
		}
	}

	public static void loadProfanityFilter(Dictionary<int, HashSet<int>> pProfanity, ref int pMinLength, ref int pMaxLength)
	{
		if (pProfanity != null && pProfanity.Count > 0)
		{
			return;
		}
		try
		{
			string[] profanities = getProfanities();
			foreach (string text in profanities)
			{
				if (text.Length < pMinLength)
				{
					pMinLength = text.Length;
				}
				if (text.Length > pMaxLength)
				{
					pMaxLength = text.Length;
				}
				if (!pProfanity.ContainsKey(text.Length))
				{
					pProfanity.Add(text.Length, new HashSet<int>());
				}
				if (!pProfanity[text.Length].Add(getCharHashCode(text.ToCharArray())))
				{
					Debug.Log((object)("Duplicate profanity: " + text));
				}
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Error when loading blacklist");
			Debug.LogError((object)ex);
		}
	}

	public static void loadProfanityFilter(Dictionary<string, string[]> pProfanity, int pIndexLength = 3)
	{
		if (pProfanity != null && pProfanity.Count > 0)
		{
			return;
		}
		try
		{
			Dictionary<string, HashSet<string>> dictionary = new Dictionary<string, HashSet<string>>();
			string[] profanities = getProfanities();
			foreach (string text in profanities)
			{
				string key = text.Substring(0, pIndexLength);
				if (!dictionary.ContainsKey(key))
				{
					dictionary.Add(key, new HashSet<string>());
				}
				if (!dictionary[key].Add(text))
				{
					Debug.Log((object)("Duplicate profanity: " + text));
				}
			}
			foreach (KeyValuePair<string, HashSet<string>> item in dictionary)
			{
				pProfanity.Add(item.Key, item.Value.ToArray());
			}
		}
		catch (Exception ex)
		{
			Debug.Log((object)"Error when loading blacklist");
			Debug.LogError((object)ex);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int getCharHashCode(char[] pChar)
	{
		return ((IStructuralEquatable)pChar).GetHashCode((IEqualityComparer)EqualityComparer<char>.Default);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string cleanString(string pString)
	{
		if (string.IsNullOrEmpty(pString))
		{
			return pString;
		}
		string text = pString[0].ToString();
		for (int i = 0; i < pString.Length - 1; i++)
		{
			if (!pString[i].Equals(pString[i + 1]))
			{
				text += pString[i + 1];
			}
		}
		return text;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string cleanStringAsSpan(string pString)
	{
		if (string.IsNullOrEmpty(pString))
		{
			return pString;
		}
		ReadOnlySpan<char> readOnlySpan = pString.AsSpan();
		Span<char> span = stackalloc char[readOnlySpan.Length];
		int length = 0;
		span[length++] = readOnlySpan[0];
		for (int i = 1; i < readOnlySpan.Length; i++)
		{
			if (readOnlySpan[i] != readOnlySpan[i - 1])
			{
				span[length++] = readOnlySpan[i];
			}
		}
		return new string(span.Slice(0, length));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ReadOnlySpan<char> cleanSpan(ReadOnlySpan<char> pSpan)
	{
		if (pSpan.Length == 0)
		{
			return pSpan;
		}
		Span<char> span = new char[pSpan.Length];
		int length = 0;
		span[length++] = pSpan[0];
		for (int i = 1; i < pSpan.Length; i++)
		{
			if (pSpan[i] != pSpan[i - 1])
			{
				span[length++] = pSpan[i];
			}
		}
		return span.Slice(0, length);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool contains(ReadOnlySpan<char> pText, ReadOnlySpan<char> pSearchPattern)
	{
		if (pSearchPattern.Length == 0)
		{
			return true;
		}
		if (pSearchPattern.Length > pText.Length)
		{
			return false;
		}
		char c = pSearchPattern[0];
		for (int i = 0; i <= pText.Length - pSearchPattern.Length; i++)
		{
			if (pText[i] != c)
			{
				continue;
			}
			bool flag = true;
			for (int j = 1; j < pSearchPattern.Length; j++)
			{
				if (pText[i + j] != pSearchPattern[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool contains(ReadOnlySpan<char> pText, ReadOnlySpan<char> pSearchPattern, int pStartIndex)
	{
		if (pSearchPattern.Length == 0)
		{
			return true;
		}
		if (pSearchPattern.Length > pText.Length)
		{
			return false;
		}
		char c = pSearchPattern[0];
		for (int i = pStartIndex; i <= pText.Length - pSearchPattern.Length; i++)
		{
			if (pText[i] != c)
			{
				continue;
			}
			bool flag = true;
			for (int j = 1; j < pSearchPattern.Length; j++)
			{
				if (pText[i + j] != pSearchPattern[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool contains(ref ReadOnlySpan<char> pText, ref ReadOnlySpan<char> pSearchPattern)
	{
		if (pSearchPattern.Length == 0)
		{
			return true;
		}
		if (pSearchPattern.Length > pText.Length)
		{
			return false;
		}
		char c = pSearchPattern[0];
		for (int i = 0; i <= pText.Length - pSearchPattern.Length; i++)
		{
			if (pText[i] != c)
			{
				continue;
			}
			bool flag = true;
			for (int j = 1; j < pSearchPattern.Length; j++)
			{
				if (pText[i + j] != pSearchPattern[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool contains2(ref ReadOnlySpan<char> pText, ref ReadOnlySpan<char> pSearchPattern)
	{
		int length = pSearchPattern.Length;
		if (length == 0)
		{
			return true;
		}
		int length2 = pText.Length;
		if (length > length2)
		{
			return false;
		}
		int i = 0;
		for (int num = length2 - length; i <= num; i++)
		{
			if (pText.Slice(i, length).SequenceEqual(pSearchPattern))
			{
				return true;
			}
		}
		return false;
	}
}
