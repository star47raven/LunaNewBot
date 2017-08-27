using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anovase
{
	public static class StringExtensions
	{
		public static string MiniString(this Guid guid)
		{
			byte[] N = guid.ToByteArray();
			char[] L = new char[8];
			for (int i = 0; i < 8; i++)
			{
				checked
				{
					L[i] = (char)(N[2 * i] * 256 + N[2 * i + 1] + '\u00c0');
				}
			}
			return new string(L);
		}

		public static string ToGuidString(this string source)
		{
			byte[] N = new byte[16];
			for (int i = 0; i < 8; i++)
			{
				checked
				{
					int c = source[i] - '\u00c0';
					N[2 * i] = (byte)(c / 256);
					N[2 * i + 1] = (byte)(c % 256);
				}
			}
			Guid G = new Guid(N);
			return G.ToString("");
		}

		public static string MiniStringFromD(this string source)
		{
			return new Guid(source).MiniString();
		}
	}
}
