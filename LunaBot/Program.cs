using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram;
using System.Threading;
using Anovase;

namespace VsoIntelBot
{
	class Program
	{
		static (long room, long user)? sevanithrah = null;
		//static bool curfew = false;
		//static string[] tracks = new string[5];
		//static long?[] tracksId = new long?[5];

		static void Main(string[] args)
		{
			/*System.AppDomain.CurrentDomain.UnhandledException += delegate (object sender, UnhandledExceptionEventArgs e)
			{
				if (e.ExceptionObject is AggregateException ex)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.BackgroundColor = ConsoleColor.Black;
					Console.WriteLine("[UNCAUGHT] " + ex.Message + " >> " + ex.InnerException?.Message ?? "");
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.BackgroundColor = ConsoleColor.DarkBlue;
					return;
				}
				if (e.ExceptionObject is Exception aex)
				{
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.BackgroundColor = ConsoleColor.Black;
					Console.WriteLine("[UNCAUGHT] " + aex.Message + " >> " + aex.InnerException?.Message ?? "");
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.BackgroundColor = ConsoleColor.DarkBlue;
					return;
				}
			};*/
			try
			{
				Console.BackgroundColor = ConsoleColor.DarkBlue;
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.Title = "Anovase Luna Bot for Telegram";
				Console.Clear();
				Console.WriteLine("[ Luna Bot for Telegram .:.:.:. Anovase Team 2017 ]\n");

				Console.WriteLine("> Enabling connectors...");
				string[] authXFX = System.IO.File.ReadAllLines("authXFX");
				TelegramConnection OpCon = new TelegramConnection(authXFX[0], false);
				TelegramSessionManager SsmCore = new TelegramSessionManager();
				TelegramCommandManager CmdCore = new TelegramCommandManager(authXFX[1]);
				try
				{
					var lines = System.IO.File.ReadAllLines("roomtarget");
					sevanithrah = (long.Parse(lines[0]), long.Parse(lines[1]));
				}
				catch { }

				CmdCore["memq"] = (s, m) =>
				{
					if (m.from.id != sevanithrah?.user)
					{
						OpCon.SendReply(m.chat.id, m.message_id, "<pre>Access Denied.</pre>");
						return;
					}

					string cmdS = s;

					while (cmdS.StartsWith(" "))
						cmdS = cmdS.Remove(0, 1);

					try
					{
						var lunaQuery = new LunaQuery(cmdS);
						var db = new MembersDatabase();
						var result = db.ProcessTsqlQuery(MembersDatabase.BuildTransactFromLunaQuery(lunaQuery));

						if (result.Data == null)
						{
							OpCon.SendReply(m.chat.id, m.message_id, $"<pre>{TelegramConnection.EscapeHtml(result.Message)}</pre>");
							return;
						}

						string output = "<b>Data returned:</b>";
						foreach (var x in result.Data)
						{
							output += "\n\n";
							foreach (var d in x)
								output += $"▫️ {TelegramConnection.EscapeHtml(d.Key)}: <code>{TelegramConnection.EscapeHtml(d.Value.ToString())}</code>\n";
						}

						OpCon.SendReply(m.chat.id, m.message_id, output);
					}
					catch (Exception ex)
					{
						OpCon.SendReply(m.chat.id, m.message_id, $"<pre>{TelegramConnection.EscapeHtml(ex.Message)}</pre>");
					}
				};

				CmdCore["assoc"] = (s, m) =>
				{
					if (m.from.id != sevanithrah?.user)
					{
						OpCon.SendReply(m.chat.id, m.message_id, "<pre>Access Denied.</pre>");
						return;
					}

					string[] cmdPart = s.Split(new[] { '\n', ' ' });
					var clearCmd = cmdPart.Where(x => !x.IsEmptyOrWhite()).ToArray();
					if (clearCmd.Length != 2)
					{
						OpCon.SendReply(m.chat.id, m.message_id, "<pre>Command Inavlid.</pre>");
						return;
					}
					var userId = m.entities.FirstOrDefault(x => x.user?.username == clearCmd[0])?.user.id;
					if (userId is null)
					{
						OpCon.SendReply(m.chat.id, m.message_id, "<pre>Telegram User Not Found.</pre>");
						return;
					}
					MembersDatabase db = new MembersDatabase();
					if (!db.UserExists(clearCmd[1], true))
					{
						OpCon.SendReply(m.chat.id, m.message_id, "<pre>Original Member Entry Not Found.</pre>");
						return;
					}
										

				};
				
				Console.WriteLine("  Connectors OPCON.");

				Console.WriteLine("> Syncing with Telegram servers...");
				long offset = 0;
				{
					@_sync_tele_begin:
					try
					{	
						offset = OpCon.GetUpdates(-1).Result.result?.FirstOrDefault()?.update_id + 1 ?? 0;
						Console.WriteLine("  Synchronisation succeeded.");
					}
					catch (AggregateException ex)
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine("[ROUND FAIL] " + ex.Message + " >> " + ex.InnerException?.Message ?? "");
						Console.ForegroundColor = ConsoleColor.Cyan;
						Thread.Sleep(1000);
						goto _sync_tele_begin;
					}
					catch (Exception ex)
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine("[ROUND FAIL] " + ex.Message);
						Console.ForegroundColor = ConsoleColor.Cyan;
						Thread.Sleep(1000);
						goto _sync_tele_begin;
					}
				}

				Console.WriteLine(">> Listening for updates");
				Console.WriteLine();
				while (true)
				{
					try
					{
						TelegramJsonObject obj = OpCon.GetUpdates(offset).Result;
						if (!obj.ok) continue;
						if (obj.result.Count > 0) offset = obj.result.Last().update_id + 1;
						foreach (var x in obj.result)
						{
							if (sevanithrah == null)
							{
								if (x.message.text.ToLower() == authXFX[2])
								{
									sevanithrah = (x.message.chat.id, x.message.from.id);
									System.IO.File.WriteAllLines("roomtarget", new[] { sevanithrah?.room.ToString(), sevanithrah?.user.ToString() });
									OpCon.SendReply(sevanithrah?.room ?? 0, x.message.message_id, "`Update channel activated on this chat.`\n`I'm OPCON to you Sevanithrah.`\n`Set me as administator to unleash higher capabilities.`");
									continue;
								}
							} 
							else if (!CmdCore.RunCheck(x))
								SsmCore.SeekAction(x);
						}
						Thread.Sleep(1000);
					}
					catch (AggregateException ex)
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine("[ROUND FAIL] " + ex.Message + " >> " + ex.InnerException?.Message ?? "");
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
					catch (Exception ex)
					{
						Console.ForegroundColor = ConsoleColor.Magenta;
						Console.WriteLine("[ROUND FAIL] " + ex.Message);
						Console.ForegroundColor = ConsoleColor.Cyan;
					}
				}
			}
			catch (AggregateException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.BackgroundColor = ConsoleColor.Black;
				Console.WriteLine("[FATAL ERROR] " + ex.Message + " >> " + ex.InnerException?.Message ?? "");
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.BackgroundColor = ConsoleColor.DarkBlue;
			}
			catch (Exception ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.BackgroundColor = ConsoleColor.Black;
				Console.WriteLine("[FATAL ERROR] " + ex.Message);
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.BackgroundColor = ConsoleColor.DarkBlue;
			}
			Console.ReadKey();
		}
	}
}
