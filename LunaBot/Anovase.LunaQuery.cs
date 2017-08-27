using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anovase
{
	public interface ILunaQueryParamParser
	{
		object Parse(string value);
	}

	public abstract class LunaParameter
	{
		public abstract object Value { get; }
		public override string ToString()
		{
			return Value.ToString();
		}
	}

	public class LunaQueryParamParseException : Exception
	{
		public string SourceQueryParam { get; protected set; }
		public Type ParameterType { get; protected set; }
		public ILunaQueryParamParser ParserObject { get; protected set; }
		
		public LunaQueryParamParseException(string message, string sourceParam, Type param, ILunaQueryParamParser parser) : base(message)
		{
			SourceQueryParam = sourceParam;
			ParameterType = param;
			ParserObject = parser;
		}
	}

	public class LunaQueryDeserialiseException : Exception
	{
		public string SourceQuery { get; protected set; }

		public LunaQueryDeserialiseException(string message, string sourceQuery) : base(message)
			=> SourceQuery = sourceQuery;
	}

	public class LunaQuery
	{
		public static readonly Dictionary<string, string[]> Demands = new Dictionary<string, string[]>()
		{
			{ "GET", new[] { "Up" } },
			{ "SET", new[] { "UE" } },
			{ "LIST", new[] { "UP" } }
		};

		protected static readonly Dictionary<char, ILunaQueryParamParser> InputTypes = new Dictionary<char, ILunaQueryParamParser>()
		{
			{ 'u', new LunaUserParam.Parser() },
			{ 'p', new LunaPropertyParam.Parser() },
			{ 'e', new LunaEqualitySetter.Parser() }
		};

		public string Command { get; set; } = "";
		public string Demand { get; set; } = "";
		public List<object> Params { get; set; } = new List<object>();

		int ParamCount { get => Params.Count; }

		public LunaQuery(string query)
		{
			string[] cmdSplitX = query.Split(new[] { '\n' });
			string[] cmdSplit = cmdSplitX.Where(x => !x.IsEmptyOrWhite()).ToArray();

			if (cmdSplit.Length == 0)
				throw new LunaQueryDeserialiseException($"Executing query is empty and cannot be processed.", query);

			if (Demands.ContainsKey(cmdSplit[0].ToUpper()))
				Command = cmdSplit[0];
			else
				throw new LunaQueryDeserialiseException($"LunaQuery command not understood: {cmdSplit[0].ToUpper()}", query);
			
			bool seal = false;
			try
			{
				foreach (string method in Demands[Command])
				{
					try
					{
						int skip = 1;
						string dem = "";
						for (int i = 0; i < method.Length; i++)
						{
							if ('a' <= method[i] && method[i] <= 'z')
							{
								try
								{
									Params.Add(InputTypes[method[i]].Parse(cmdSplit[i + skip]));
								}
								catch { skip--; continue; }
							}
							else
							{
								Params.Add(InputTypes[(char)(method[i] - 'A' + 'a')].Parse(cmdSplit[i + skip]));
							}
							dem += method[i];
						}
						Demand = dem.ToLower();
						seal = true;
					}
					catch { continue; }
				}
				if (!seal)
					throw new LunaQueryDeserialiseException("The parameters within this LunaQuery cannot be processed.", query);
			}
			catch (IndexOutOfRangeException)
			{
				throw new LunaQueryDeserialiseException($"The parameters within this LunaQuery cannot be processed.", query);
			}
		}
	}

	public class LunaUserParam : LunaParameter
	{
		public string Username { get; private set; }

		public override object Value => Username;

		public class Parser : ILunaQueryParamParser
		{
			public object Parse(string value)
			{
				if (!value.JustContains(('a', 'z'), ('A', 'Z'), ('0', '9'), ('-', '-'), ('.', '.')))
					throw new LunaQueryParamParseException("The parameter does not fall into the required restrictions.", value, typeof(LunaUserParam), this);
				return new LunaUserParam() { Username = value };
			}
		}
	}

	public class LunaPropertyParam : LunaParameter
	{
		public string PropertyName { get; private set; }

		public override object Value => PropertyName;

		public class Parser : ILunaQueryParamParser
		{
			public object Parse(string value)
			{
				if (!value.StartsWith("[") || !value.EndsWith("]"))
					throw new LunaQueryParamParseException("The parameter does not meet the expected format.", value, typeof(LunaPropertyParam), this);
				if (!value.Substring(1, value.Length - 2).JustContains(('a', 'z'), ('A', 'Z'), ('0', '9'), (' ', ' ')))
					throw new LunaQueryParamParseException("The parameter does not fall into the required restrictions.", value, typeof(LunaPropertyParam), this);
				return new LunaPropertyParam() { PropertyName = value.Substring(1, value.Length - 2) };
			}
		}
	}

	public class LunaPlainParam : LunaParameter
	{
		public object InnerValue { get; private set; }

		public override object Value => InnerValue;

		public class Parser : ILunaQueryParamParser
		{
			public object Parse(string value)
			{
				if (value == "\\NULL")
					return new LunaPlainParam() { InnerValue = DBNull.Value };
				return new LunaPlainParam() { InnerValue = value };
			}
		}
	}

	public class LunaEqualitySetter : LunaParameter
	{
		public LunaPropertyParam Property { get; set; }
		public LunaPlainParam Setter { get; set; }

		public override object Value => Setter.InnerValue;

		public class Parser : ILunaQueryParamParser
		{
			public object Parse(string value)
			{
				var eqPart = value.Split(new[] { '=' }, 2);
				return new LunaEqualitySetter()
				{
					Property = (LunaPropertyParam)new LunaPropertyParam.Parser().Parse(eqPart[0]),
					Setter = (LunaPlainParam)new LunaPlainParam.Parser().Parse(eqPart[1])
				};
			}
		}
	}

	// Ctrl+L => Remove Line

	public class TsqlOutput
	{
		public string Message { get; set; } = null;
		public List<Dictionary<string, object>> Data { get; set; } = new List<Dictionary<string, object>>();

		public TsqlOutput() { }
		public TsqlOutput(string message, List<Dictionary<string, object>> data) => (Message, Data) = (message, data);
	}
}
