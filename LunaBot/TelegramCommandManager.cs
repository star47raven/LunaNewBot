using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
	public class TelegramCommandManager
	{
		public string Username { get; set; }
		List<CommandPair> Actions = new List<CommandPair>();

		public Action<string, Telegram.Message> this[string cmd]
		{
			get => Actions.First((CommandPair x) => cmd.StartsWith("/" + x.Command + "\n") || cmd.StartsWith("/" + x.Command + "@" + Username + "\n") || cmd.StartsWith("/" + x.Command + " ") || cmd.StartsWith("/" + x.Command + "@" + Username + " ") || cmd == x.Command || cmd == "/" + x.Command || cmd == "/" + x.Command + "@" + Username).Callback;
			set
			{
				if (value is null) return;
				for (int i = 0; i < Actions.Count; i++)
					if (Actions[i].Command == cmd)
					{
						Actions[i].Callback = value;
					}
				Actions.Add((cmd, value));
			}
		}

		public bool RunCheck(Telegram.Result result)
		{
			MessageEntity cmdEnt = null;
			if (result.message?.text == null) return false;
			if (!result.message.entities.Any(x => (cmdEnt = x).type == "bot_command" && x.offset == 0)) return false;
			Task.Run(delegate 
			{
				try
				{
					this[result.message.text.ToLower()].Invoke(result.message.text.Remove(0, cmdEnt.length), result.message);
				}
				catch (AggregateException ex)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("[WARNING] " + ex.Message + " >> " + ex.InnerException?.Message ?? "");
					Console.ForegroundColor = ConsoleColor.Cyan;
				}
				catch (Exception ex)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.WriteLine("[WARNING] " + ex.Message);
					Console.ForegroundColor = ConsoleColor.Cyan;
				}
			});
			return true;
		}

		public TelegramCommandManager(string username)
		{
			Username = username;
		}
	}

	public class CommandPair
	{
		public string Command { get; set; }
		public Action<string, Telegram.Message> Callback { get; set; }

		public static implicit operator CommandPair((string Command, Action<string, Telegram.Message> Callback) tuple)
		{
			return new CommandPair() { Command = tuple.Command, Callback = tuple.Callback };
		}
	}
}
