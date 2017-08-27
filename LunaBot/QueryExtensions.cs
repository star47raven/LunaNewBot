using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for QueryExtensions
/// </summary>
public static class QueryExtensions
{
	public static int IndexOfKey<TKey, TVal>(this Dictionary<TKey, TVal> dictionary, Func<TKey, bool> predicate)
	{
		for (int i = 0; i < dictionary.Count; i++)
			if (predicate(dictionary.Keys.ElementAt(i)))
				return i;
		return dictionary.Count;
	}

	public static bool IsNullOrEmpty(this string str)
	{
		if (str == null)
			return true;
		else if (str == string.Empty)
			return true;
		return false;
	}

	public static bool IsEmptyOrWhite(this string str)
	{
		if (str == string.Empty)
			return true;
		else if (str.JustContains(new[] { ' ' }))
			return true;
		return false;
	}

	public static bool ContainsAny(this string source, char[] chars)
	{
		bool res = false;
		foreach (char x in chars)
			res |= source.Contains(x);
		return res;
	}

	public static bool ContainsAll(this string source, char[] chars)
	{
		bool res = true;
		foreach (char x in chars)
			res &= source.Contains(x);
		return res;
	}

	public static bool JustContains(this string source, params (char start, char end)[] ranges)
	{
		bool res = true;
		foreach (char x in source)
		{
			bool cur = false;
			foreach ((char s, char e) r in ranges)
				cur |= (x >= r.s && x <= r.e);
			res &= cur;
		}
		return res;
	}

	public static bool JustContains(this string source, char[] chars)
	{
		bool res = true;
		foreach (char x in source)
		{
			bool cur = false;
			foreach (char c in chars)
				cur |= (c == x);
			res &= cur;
		}
		return res;
	}
}
