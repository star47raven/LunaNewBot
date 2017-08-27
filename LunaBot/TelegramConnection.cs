using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Telegram
{
    public class TelegramConnection
    {
        public string Auth;
		public User Me;
		public string ParseMode = "html";

		public static string EscapeHtml(string text)
		{
			return text.Replace("<", "&lt;").Replace(">", "&gt;").Replace("&", "&amp;")/*.Replace("\"", "&quot;")*/;
		}

        public TelegramConnection(string AuthKey, bool Markdown)
        {
            Auth = AuthKey;
			_try_lapse:
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + "/getMe");
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				Me = JsonConvert.DeserializeObject<TelegramJsonObject<User>>(Encoding.UTF8.GetString(buff)).result;
				if (Markdown)
					ParseMode = "{ParseMode}";
			}
			catch (WebException)
			{
				goto _try_lapse;
			}
		}

        public Message SendMessage(long ChatID, string Text)
        {
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + "/sendMessage?parse_mode=" + ParseMode + "&chat_id=" + ChatID + "&text=" + Text);
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
        }

		public Message SendPhoto(long ChatID, string Url, string Caption = null)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + "/sendPhoto?" + "chat_id=" + ChatID + "&photo=" + Url + (Caption is null ? "" : "&caption=" + Caption));
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public async Task<Message> SendPhoto(long ChatID, byte[] data, string Caption = null)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				/*HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + "/sendPhoto?" + "chat_id=" + ChatID + (Caption is null ? "" : "&caption=" + Caption));
				postx.Method = "POST";
				postx.ContentType = "multipart/form-data";
				//postx.Timeout = 5000;
				MultipartFormDataContent cont = new MultipartFormDataContent();
				cont.Add(new ByteArrayContent(data), "photo");
				await cont.CopyToAsync(postx.GetRequestStream());
				WebResponse res = await postx.GetResponseAsync();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;*/
				var client = new HttpClient();
				var cont = new MultipartFormDataContent();
				var bac = new ByteArrayContent(data);
				bac.Headers.Add("Content-Type", "image/jpg");
				cont.Add(bac, "photo");
				var rq = new HttpRequestMessage(HttpMethod.Post, "https://api.telegram.org/bot" + Auth + "/sendPhoto?" + "chat_id=" + ChatID + (Caption is null ? "" : "&caption=" + Caption));
				rq.Content = cont;
				var res = await client.SendAsync(rq);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(await res.Content.ReadAsStringAsync()).result;
			}
			catch (WebException ex)
			{
				if (__try < 1)
					goto _try_lapse;
				else
				{
					string errJson = await new StreamReader(ex.Response.GetResponseStream()).ReadToEndAsync();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public Message SendAudio(long ChatID, string Url, string Caption = null)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + "/sendAudio?" + "chat_id=" + ChatID + "&audio=" + Url + (Caption is null ? "" : "&caption=" + Caption));
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public Message SendReply(long ChatID, long ReplyTo, string Text)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + $"/sendMessage?parse_mode={ParseMode}&chat_id={ChatID}&reply_to_message_id={ReplyTo}&text={Text}&reply_markup={{\"hide_keyboard\":true,\"selective\":true}}");
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public Message SendReply(long ChatID, long ReplyTo, string Text, IReplyObject Reply)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				string DI = "";
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create(DI = "https://api.telegram.org/bot" + Auth + $"/sendMessage?parse_mode={ParseMode}&chat_id={ChatID}&reply_to_message_id={ReplyTo}&text={Text}&reply_markup={JsonConvert.SerializeObject(Reply, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })}");
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public bool IsChatAdmin(Chat Pipe, User Client)
		{
			if (Pipe.type == "private") return true;
			else
			{
				int __try = 0;
				_try_lapse:
				__try++;
				try
				{
					HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + $"/getChatAdministrators?chat_id={Pipe.id}");
					rq.Timeout = 5000;
					HttpWebResponse rs = (HttpWebResponse)rq.GetResponse();
					byte[] buff = new byte[rs.ContentLength];
					rs.GetResponseStream().Read(buff, 0, (int)rs.ContentLength);
					rs.Close();
					string STR = Encoding.UTF8.GetString(buff);
					ChatMember[] tgix = JsonConvert.DeserializeObject<TelegramJsonObject<ChatMember[]>>(STR).result;
					if (tgix.Count((ChatMember x) => x.user.id == Client.id) == 0) return false;
					else return true;
				}
				catch (WebException ex)
				{
					if (__try < 5)
						goto _try_lapse;
					else
					{
						string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
						var exp = JsonConvert.DeserializeObject<TelegramJsonObject<ChatMember[]>>(errJson);
						throw new TelegramException(exp);
					}
				}
			}
		}

		public void AnswerCallback(string UID, string Text, bool Alert)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + $"/answerCallbackQuery?callback_query_id={UID}&text={Text}&show_alert={Alert}");
				postx.Timeout = 5000;
				postx.GetResponse().Close();
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<object>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public Message EditMessage(long ChatID, long Message, string Text, IReplyObject Reply = null)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				// $DI
				string ix = JsonConvert.SerializeObject(Reply, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + $"/editMessageText?parse_mode={ParseMode}&chat_id={ChatID}&message_id={Message}&text={Text}" + (Reply is null ? "" : $"&reply_markup={JsonConvert.SerializeObject(Reply, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })}"));
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				// $DI
				string ixx = Encoding.UTF8.GetString(buff);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public Message EditInline(long ChatID, long Message, IReplyObject Reply)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + $"/editMessageReplyMarkup?chat_id={ChatID}&message_id={Message}&reply_markup={JsonConvert.SerializeObject(Reply, Formatting.None, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })}");
				postx.Timeout = 5000;
				WebResponse res = postx.GetResponse();
				byte[] buff = new byte[res.ContentLength];
				res.GetResponseStream().Read(buff, 0, (int)res.ContentLength);
				return JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(Encoding.UTF8.GetString(buff)).result;
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public void RemoveMessage(long ChatID, long Message)
		{
			int __try = 0;
			_try_lapse:
			__try++;
			try
			{
				HttpWebRequest postx = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + $"/deleteMessage?chat_id={ChatID}&message_id={Message}");
				postx.Timeout = 5000;
				postx.GetResponse();
			}
			catch (WebException ex)
			{
				if (__try < 5)
					goto _try_lapse;
				else
				{
					string errJson = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
					var exp = JsonConvert.DeserializeObject<TelegramJsonObject<Message>>(errJson);
					throw new TelegramException(exp);
				}
			}
		}

		public async Task<TelegramJsonObject> GetUpdates(long Offset)
        {
			/*HttpWebRequest rq = (HttpWebRequest)HttpWebRequest.Create("https://api.telegram.org/bot" + Auth + "/getUpdates?offset=" + Offset + "&limit=5");
            rq.Timeout = 10000;
            HttpWebResponse rs = (HttpWebResponse)rq.GetResponse();
            byte[] buff = new byte[rs.ContentLength];
            rs.GetResponseStream().Read(buff, 0, (int)rs.ContentLength);
            rs.Close();
			string STR = Encoding.UTF8.GetString(buff);
			TelegramJsonObject tgix = JsonConvert.DeserializeObject<TelegramJsonObject>(STR);
            return tgix;*/

			return JsonConvert.DeserializeObject<TelegramJsonObject>(await (await new HttpClient() { Timeout = TimeSpan.FromMilliseconds(10000) }
				.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.telegram.org/bot" + Auth + "/getUpdates?offset=" + Offset + "&limit=5")))
				.Content.ReadAsStringAsync());
        }
    }
}
