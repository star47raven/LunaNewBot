using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anovase.MembersData
{
	public class BoardPermissions
	{
		public PermissionAttribute Get { get; set; } = null;
		public PermissionAttribute Set { get; set; } = null;
		public PermissionAttribute Add { get; set; } = null;

		public BoardPermissions(string perm)
		{
			
		}
	}

	public class PermissionAttribute
	{
		public (string Column, bool Allow)[] Columns { get; set; }
		public (string Role, bool Allow)[] RoleFilter { get; set; }
		public (string Name, bool Allow)[] NamedPart { get; set; }
		public string OriginalString { get; protected set; }

		public PermissionAttribute(string attr)
		{
			string[] parts = attr.Split(';');
			if (parts.Length != 3)
				throw new ArgumentException("The attributes are not in the correct format.");

			(string, bool)[] deserialiseAttr(string a)
			{
				string itr = a.Substring(0);
				List<(string, bool)> data = new List<(string, bool)>();
				if (itr[0] == '+' || itr[0] == '-')
				{
					while (itr.Length > 0)
					{
						string current = "";
						int i = Math.Min(itr.IndexOf('+'), itr.IndexOf('-'));
						if (i != -1)
						{
							current = itr.Substring(0, i);
							itr = itr.Substring(i);
						}
						else
						{
							current = itr;
							itr = "";
						}
						data.Add((current.Substring(1), current[0] == '+'));
					}
					return data.ToArray();
				}
				else
					throw new ArgumentException("The attributes are not in the correct format.");
			}

			Columns = deserialiseAttr(parts[0]);
			RoleFilter = deserialiseAttr(parts[1]);
			NamedPart = deserialiseAttr(parts[2]);
			OriginalString = attr;
		}

		public override string ToString()
		{
			string str = "";
			foreach (var x in Columns ?? new (string, bool)[0])
				str += $"{(x.Allow ? '+' : '-')}{x.Column}";
			str += ";";
			foreach (var x in RoleFilter ?? new(string, bool)[0])
				str += $"{(x.Allow ? '+' : '-')}{x.Role}";
			str += ";";
			foreach (var x in NamedPart ?? new(string, bool)[0])
				str += $"{(x.Allow ? '+' : '-')}{x.Name}";
			return str;
		}
	}


}
