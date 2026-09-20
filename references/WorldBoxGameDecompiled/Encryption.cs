using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Encryption
{
	private const string INIT_VECTOR = "sayHiIfUReadThis";

	private const int KEY_SIZE = 256;

	public static string EncryptString(string pPlainText, string pPassPhrase)
	{
		byte[] bytes = Encoding.UTF8.GetBytes("sayHiIfUReadThis");
		byte[] bytes2 = Encoding.UTF8.GetBytes(pPlainText);
		using PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(pPassPhrase, null);
		byte[] bytes3 = passwordDeriveBytes.GetBytes(32);
		using RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			Mode = CipherMode.CBC
		};
		using ICryptoTransform transform = rijndaelManaged.CreateEncryptor(bytes3, bytes);
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(bytes2, 0, bytes2.Length);
		cryptoStream.FlushFinalBlock();
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	public static string DecryptString(string pCipherText, string pPassPhrase)
	{
		byte[] bytes = Encoding.UTF8.GetBytes("sayHiIfUReadThis");
		byte[] array = Convert.FromBase64String(pCipherText);
		using PasswordDeriveBytes passwordDeriveBytes = new PasswordDeriveBytes(pPassPhrase, null);
		byte[] bytes2 = passwordDeriveBytes.GetBytes(32);
		using RijndaelManaged rijndaelManaged = new RijndaelManaged
		{
			Mode = CipherMode.CBC
		};
		using ICryptoTransform transform = rijndaelManaged.CreateDecryptor(bytes2, bytes);
		using MemoryStream stream = new MemoryStream(array);
		using CryptoStream cryptoStream = new CryptoStream(stream, transform, CryptoStreamMode.Read);
		byte[] array2 = new byte[array.Length];
		int count = cryptoStream.Read(array2, 0, array2.Length);
		return Encoding.UTF8.GetString(array2, 0, count);
	}
}
