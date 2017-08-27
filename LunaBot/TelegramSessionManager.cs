using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
	public interface ITelegramSession
	{
		(bool, bool) TryFire(Result update);
	}

	public class TelegramInteractiveSession : ITelegramSession
	{
		public long ChatPipe { get; set; } 
		public long? UserID { get; set; }
		public long MessageID { get; set; }
		public Func<Message, bool> Callback { get; set; }

		public (bool, bool) TryFire(Result update)
		{
			if (update.message is null) return (false, false);
			if (update.message.chat.id == ChatPipe
				&& update.message.reply_to_message?.message_id == MessageID
				&& (UserID is null || update.message.from?.id == UserID))
			{
				bool ret = Callback.Invoke(update.message);
				return (true, ret);
			}
			else return (false, false);
		}

		public TelegramInteractiveSession(long chat, long? user, long message, Func<Message, bool> callback)
		{
			ChatPipe = chat;
			UserID = user;
			MessageID = message;
			Callback = callback;
		}
	}

	public class TelegramBloackadeSession : ITelegramSession
	{
		public long ChatPipe { get; set; }
		public Func<Message, bool> Callback { get; set; }

		public (bool, bool) TryFire(Result update)
		{
			if (update.message is null)
				return (false, false);
			if (update.message.chat.id == ChatPipe)
			{
				bool ret = Callback.Invoke(update.message);
				return (true, ret);
			}
			else
				return (false, false);
		}

		public TelegramBloackadeSession(long chat, Func<Message, bool> callback)
		{
			ChatPipe = chat;
			Callback = callback;
		}
	}

	public class TelegramUserSession : ITelegramSession
	{
		public long ChatPipe { get; set; }
		public Func<Message, bool> Callback { get; set; }

		public (bool, bool) TryFire(Result update)
		{
			if (update.message is null) return (false, false);
			if (update.message.chat.id == ChatPipe
				/*&& update.message.reply_to_message == null*/)
			{
				bool ret = Callback.Invoke(update.message);
				return (true, ret);
			}
			else return (false, false);
		}

		public TelegramUserSession(long chat, Func<Message, bool> callback)
		{
			ChatPipe = chat;
			Callback = callback;
		}
	}

	public class TelegramInlineSession : ITelegramSession
	{
		public long ChatPipe { get; set; }
		public long? UserID { get; set; }
		public long MessageID { get; set; }
		public object State { get; set; } = "";
		public Func<CallbackQuery, TelegramInlineSession, bool> Callback { get; set; }

		public (bool, bool) TryFire(Result update)
		{
			if (update.callback_query is null) return (false, false);
			if (update.callback_query.message.chat.id == ChatPipe
				&& update.callback_query.message.message_id == MessageID
				&& (UserID == null || update.callback_query.from?.id == UserID))
			{
				bool ret = Callback.Invoke(update.callback_query, this);
				return (true, ret);
			}
			else return (false, false);
		}

		public TelegramInlineSession(long chat, long? user, long message, Func<CallbackQuery, TelegramInlineSession, bool> callback)
		{
			ChatPipe = chat;
			UserID = user;
			MessageID = message;
			Callback = callback;
		}
	}

	public class TelegramStatusMessageProxy
	{
		public long ChatPipe { get; protected set; }
		public long MessageID { get; protected set; }
		public TelegramConnection Connection { get; protected set; }
		protected string _Text = "";
		public string Text
		{
			get => _Text;
			set
			{
				_Text = value;
				Connection.EditMessage(ChatPipe, MessageID, value, null);
			}
		}

		public TelegramStatusMessageProxy(TelegramConnection connection, long chatid, string text)
		{
			Connection = connection;
			ChatPipe = chatid;
			_Text = text;
			Message m = Connection.SendMessage(ChatPipe, Text);
			MessageID = m.message_id;
		}
	}

	public class TelegramSessionManager
	{
		public List<ITelegramSession> Sessions { get; protected set; } = new List<ITelegramSession>();

		public void PendInteractive(Message message, long userid, bool selective, Func<Message, bool> callback)
		{
			Sessions.Add(new TelegramInteractiveSession(message.chat.id, selective ? (int?)userid : null, message.message_id, callback));
		}

		public void PendUser(Message message, Func<Message, bool> callback)
		{
			Unpenduser(message.chat.id);
			Sessions.Add(new TelegramUserSession(message.chat.id, callback));
		}

		public void PendInline(Message message, long userid, bool selective, Func<CallbackQuery, TelegramInlineSession, bool> callback)
		{
			Sessions.Add(new TelegramInlineSession(message.chat.id, selective ? (int?)userid : null, message.message_id, callback));
		}

		public void PendBlockade(Message message, Func<Message, bool> callback)
		{
			Sessions.Add(new TelegramBloackadeSession(message.chat.id, callback));
		}

		public void Unpenduser(long chatid)
		{
			Sessions.RemoveAll((ITelegramSession s) => _userCast(s)?.ChatPipe == chatid);

			TelegramUserSession _userCast(ITelegramSession session)
			{
				if (session is TelegramUserSession) return (TelegramUserSession)session;
				else return null;
			}
		}

		public void SeekAction(Result update)
		{
			if (update.message is null && update.callback_query is null) return;
			Task.Run(delegate {
				try
				{
					for (int i = 0; i < Sessions.Count; i++)
					{
						(bool act, bool remove) = Sessions[i].TryFire(update);
						if (act)
						{
							if (remove) Sessions.RemoveAt(i);
							break;
						}
					}
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
		}
	}
}
