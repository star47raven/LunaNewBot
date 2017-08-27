using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

/// <summary>
/// Summary description for MemberDataSet
/// </summary>
public class MemberDataSet
{
	public Dictionary<string, object> Header, Status;

	//public static HttpServerUtility ServerInstance = null;
	public static void LoadRanks()
	{
		var path = "ranks.txt";
		var ranksList = File.ReadAllLines(path);
		foreach (var x in ranksList)
		{
			var data = x.Split(';');
			try
			{ Ranks.Add(data[0], data[1]); }
			catch { }
		}
	}

	private static string ReplaceHash(string source, string compared)
	{
		string newlie = (string)source.Clone();
		for (int i = 0; i < source.Length; i++)
			if (source[i] == '#')
			{
				newlie = newlie.Remove(i, 1);
				newlie = newlie.Insert(i, compared[i].ToString());
			}

		return newlie;
	}

	public static Dictionary<string, string> Ranks = new Dictionary<string, string>();

	public class Comparer : IComparer<MemberDataSet>
	{
		public int Compare(MemberDataSet x, MemberDataSet y)
		{
			var s1 = (string)x.Header["Role"];
			var s2 = (string)y.Header["Role"];
			var i1 = Ranks.IndexOfKey(n => ReplaceHash(n, s1) == s1);
			var i2 = Ranks.IndexOfKey(n => ReplaceHash(n, s2) == s2);
			if (i1 - i2 == 0)
			{
				for (int i = 0; i < 4; i++)
					if (s1[i] <= '9' && s1[i] >= '0')
						return s2[i] - s1[i];
				return 0;
			}
			else
				return i1 - i2;
		}
	}

	public static string GetRoleName(string role, string namedPart)
	{
		var np = namedPart;
		if (namedPart == null)
			np = "";
		var item = Ranks.Where(n => ReplaceHash(n.Key, role) == role).First();
		return string.Format(item.Value, namedPart, role[2], role[3]);
	}
}