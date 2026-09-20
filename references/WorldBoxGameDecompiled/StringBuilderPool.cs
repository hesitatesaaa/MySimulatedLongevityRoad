using System;
using System.Collections.Generic;
using System.Text;

public class StringBuilderPool : IDisposable, IEquatable<StringBuilderPool>, IEquatable<StringBuilder>
{
	[ThreadStatic]
	private static StackPool<StringBuilder> _pool;

	private StringBuilder _string_builder = pool.get();

	private bool _disposed;

	private static StackPool<StringBuilder> pool
	{
		get
		{
			if (_pool == null)
			{
				_pool = new StackPool<StringBuilder>();
			}
			return _pool;
		}
	}

	public StringBuilder string_builder
	{
		get
		{
			if (_disposed)
			{
				throw new ObjectDisposedException("StringBuilderPool");
			}
			return _string_builder;
		}
	}

	public int Capacity
	{
		get
		{
			return _string_builder.Capacity;
		}
		set
		{
			_string_builder.Capacity = value;
		}
	}

	public int MaxCapacity => _string_builder.MaxCapacity;

	public int Length
	{
		get
		{
			return _string_builder.Length;
		}
		set
		{
			_string_builder.Length = value;
		}
	}

	public char this[int index]
	{
		get
		{
			return _string_builder[index];
		}
		set
		{
			_string_builder[index] = value;
		}
	}

	public StringBuilderPool()
	{
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_string_builder.Clear();
			pool.release(_string_builder);
			_disposed = true;
		}
	}

	public StringBuilderPool ToTitleCase()
	{
		_string_builder.ToTitleCase();
		return this;
	}

	public StringBuilderPool ToUpper()
	{
		_string_builder.ToUpper();
		return this;
	}

	public StringBuilderPool ToUpperInvariant()
	{
		_string_builder.ToUpperInvariant();
		return this;
	}

	public StringBuilderPool ToLower()
	{
		_string_builder.ToLower();
		return this;
	}

	public StringBuilderPool ToLowerInvariant()
	{
		_string_builder.ToLowerInvariant();
		return this;
	}

	public StringBuilderPool TrimEnd(params char[] trimChars)
	{
		_string_builder.TrimEnd(trimChars);
		return this;
	}

	public StringBuilderPool Cut(int startIndex, int end)
	{
		_string_builder.CreateTrimmedString(startIndex, end);
		return this;
	}

	public StringBuilderPool Remove(params char[] chars)
	{
		_string_builder.Remove(chars);
		return this;
	}

	public int IndexOf(char value)
	{
		return _string_builder.IndexOf(value);
	}

	public int IndexOfAny(params char[] anyOf)
	{
		return _string_builder.IndexOfAny(anyOf);
	}

	public int LastIndexOf(char value)
	{
		return _string_builder.LastIndexOf(value);
	}

	public int LastIndexOfAny(char[] anyOf, int startIndex)
	{
		return _string_builder.LastIndexOfAny(anyOf, startIndex);
	}

	public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
	{
		return _string_builder.LastIndexOfAny(anyOf, startIndex, count);
	}

	public int LastIndexOfAny(params char[] anyOf)
	{
		return _string_builder.LastIndexOfAny(anyOf);
	}

	public StringBuilderPool(int capacity)
	{
		_string_builder = new StringBuilder(capacity);
	}

	public StringBuilderPool(string value)
	{
		_string_builder = new StringBuilder(value);
	}

	public StringBuilderPool(int capacity, int maxCapacity)
	{
		_string_builder = new StringBuilder(capacity, maxCapacity);
	}

	public StringBuilderPool(string value, int capacity)
	{
		_string_builder = new StringBuilder(value, capacity);
	}

	public StringBuilderPool(string value, int startIndex, int length, int capacity)
	{
		_string_builder = new StringBuilder(value, startIndex, length, capacity);
	}

	public StringBuilderPool Append(bool value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(byte value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(char value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(char[] value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(char[] value, int startIndex, int charCount)
	{
		_string_builder.Append(value, startIndex, charCount);
		return this;
	}

	public StringBuilderPool Append(decimal value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(double value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(short value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(int value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(long value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(object value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(ReadOnlyMemory<char> value)
	{
		_string_builder.Append((object?)value);
		return this;
	}

	public StringBuilderPool Append(ReadOnlySpan<char> value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(sbyte value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(float value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(string value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(string value, int startIndex, int count)
	{
		_string_builder.Append(value, startIndex, count);
		return this;
	}

	public StringBuilderPool Append(StringBuilder value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(StringBuilder value, int startIndex, int count)
	{
		_string_builder.Append(value, startIndex, count);
		return this;
	}

	public StringBuilderPool Append(StringBuilderPool value)
	{
		_string_builder.Append(value.string_builder);
		return this;
	}

	public StringBuilderPool Append(StringBuilderPool value, int startIndex, int count)
	{
		_string_builder.Append(value.string_builder, startIndex, count);
		return this;
	}

	public StringBuilderPool Append(ushort value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(uint value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool Append(ulong value)
	{
		_string_builder.Append(value);
		return this;
	}

	public StringBuilderPool AppendFormat(IFormatProvider provider, string format, params object[] args)
	{
		_string_builder.AppendFormat(provider, format, args);
		return this;
	}

	public StringBuilderPool AppendFormat(string format, object arg0)
	{
		_string_builder.AppendFormat(format, arg0);
		return this;
	}

	public StringBuilderPool AppendFormat(string format, object arg0, object arg1)
	{
		_string_builder.AppendFormat(format, arg0, arg1);
		return this;
	}

	public StringBuilderPool AppendFormat(string format, object arg0, object arg1, object arg2)
	{
		_string_builder.AppendFormat(format, arg0, arg1, arg2);
		return this;
	}

	public StringBuilderPool AppendFormat(string format, params object[] args)
	{
		_string_builder.AppendFormat(format, args);
		return this;
	}

	public StringBuilderPool AppendJoin(char separator, params object[] values)
	{
		_string_builder.AppendJoin(separator, values);
		return this;
	}

	public StringBuilderPool AppendJoin(char separator, params string[] values)
	{
		_string_builder.AppendJoin(separator, values);
		return this;
	}

	public StringBuilderPool AppendJoin(string separator, params object[] values)
	{
		_string_builder.AppendJoin(separator, values);
		return this;
	}

	public StringBuilderPool AppendJoin(string separator, params string[] values)
	{
		_string_builder.AppendJoin(separator, values);
		return this;
	}

	public StringBuilderPool AppendJoin<T>(char separator, IEnumerable<T> values)
	{
		_string_builder.AppendJoin(separator, values);
		return this;
	}

	public StringBuilderPool AppendJoin<T>(string separator, IEnumerable<T> values)
	{
		_string_builder.AppendJoin(separator, values);
		return this;
	}

	public StringBuilderPool AppendLine()
	{
		_string_builder.AppendLine();
		return this;
	}

	public StringBuilderPool AppendLine(string value)
	{
		_string_builder.AppendLine(value);
		return this;
	}

	public StringBuilderPool Clear()
	{
		_string_builder.Clear();
		return this;
	}

	public void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
	{
		_string_builder.CopyTo(sourceIndex, destination, destinationIndex, count);
	}

	public void CopyTo(int sourceIndex, Span<char> destination, int count)
	{
		_string_builder.CopyTo(sourceIndex, destination, count);
	}

	public int EnsureCapacity(int capacity)
	{
		return _string_builder.EnsureCapacity(capacity);
	}

	public bool Equals(ReadOnlySpan<char> span)
	{
		return _string_builder.Equals(span);
	}

	public bool Equals(StringBuilder sb)
	{
		return _string_builder.Equals(sb);
	}

	public bool Equals(StringBuilderPool sb)
	{
		return _string_builder.Equals(sb.string_builder);
	}

	public StringBuilderPool Insert(int index, bool value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, byte value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, char value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, char[] value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, char[] value, int startIndex, int charCount)
	{
		_string_builder.Insert(index, value, startIndex, charCount);
		return this;
	}

	public StringBuilderPool Insert(int index, decimal value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, double value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, short value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, int value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, long value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, object value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, ReadOnlySpan<char> value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, sbyte value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, float value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, string value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, string value, int count)
	{
		_string_builder.Insert(index, value, count);
		return this;
	}

	public StringBuilderPool Insert(int index, ushort value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, uint value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Insert(int index, ulong value)
	{
		_string_builder.Insert(index, value);
		return this;
	}

	public StringBuilderPool Remove(int startIndex, int length)
	{
		_string_builder.Remove(startIndex, length);
		return this;
	}

	public StringBuilderPool Replace(char oldChar, char newChar)
	{
		_string_builder.Replace(oldChar, newChar);
		return this;
	}

	public StringBuilderPool Replace(char oldChar, char newChar, int startIndex, int count)
	{
		_string_builder.Replace(oldChar, newChar, startIndex, count);
		return this;
	}

	public StringBuilderPool Replace(string oldValue, string newValue)
	{
		_string_builder.Replace(oldValue, newValue);
		return this;
	}

	public StringBuilderPool Replace(string oldValue, string newValue, int startIndex, int count)
	{
		_string_builder.Replace(oldValue, newValue, startIndex, count);
		return this;
	}

	public string ToString(int startIndex, int length)
	{
		return _string_builder.ToString(startIndex, length);
	}

	public override string ToString()
	{
		return _string_builder.ToString();
	}

	public Span<char> AsSpan()
	{
		Span<char> span = new char[_string_builder.Length];
		_string_builder.CopyTo(0, span, _string_builder.Length);
		return span;
	}
}
