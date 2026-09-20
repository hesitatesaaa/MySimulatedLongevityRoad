using System;
using System.IO;
using System.Security.Cryptography;
using Beebyte.Obfuscator;

[ObfuscateLiterals]
public class RequestHelper
{
	private static string _salt = "";

	public static string salt
	{
		get
		{
			if (_salt == "")
			{
				try
				{
					_salt = SHA256CheckSum(typeof(RequestHelper).Assembly.Location);
				}
				catch (Exception)
				{
					_salt = "err";
				}
			}
			return _salt;
		}
	}

	public static string SHA256CheckSum(string filePath)
	{
		using SHA256 sHA = SHA256.Create();
		using BufferedStream inputStream = new BufferedStream(File.OpenRead(filePath), 1200000);
		return BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", string.Empty).ToLower();
	}
}
