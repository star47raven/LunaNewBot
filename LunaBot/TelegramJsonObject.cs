using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram
{
	public class User
	{
		public long id;
		public string first_name;
		public string last_name;
		public string username;
	}

	public class Chat
	{
		public long id;
		public string type;
		public string title;
		public string first_name;
		public string last_name;
		public string username;
		public bool? all_members_are_administrators;
	}

	public class Message
	{
		public long message_id;
		public User from;
		public int date;
		public Chat chat;
		public User forward_from;
		public Chat forward_from_chat;
		public long? forward_from_message_id;
		public int? forward_date;
		public Message reply_to_message;
		public int? edit_date;
		public string text;
		public MessageEntity[] entities;
		public Audio audio;
		public Document document;
		public PhotoSize[] photo;
		public Sticker sticker;
		public Video video;
		public Voice voice;
		public string caption;
		public Contact contact;
		public Location location;
		public Venue venue;
		public User new_chat_member;
		public User left_chat_member;
		public string new_chat_title;
		public PhotoSize[] new_chat_photo;
		public bool? delete_chat_photo;
		public bool? group_chat_created;
		public bool? supergroup_chat_created;
		public bool? channel_chat_created;
		public long? migrate_to_chat_id;
		public long? migrate_from_chat_id;
		public Message pinned_message;
	}

	public class MessageEntity
	{
		public string type;
		public int offset;
		public int length;
		public string url;
		public User user;
	}

	public class PhotoSize
	{
		public string file_id;
		public int width;
		public int height;
		public int? file_size;
	}

	public class Audio
	{
		public string file_id;
		public long duration;
		public string performer;
		public string title;
		public string mime_type;
		public long? file_size;
	}

	public class Document
	{
		public string file_id;
		public PhotoSize thumb;
		public string file_name;
		public string mime_type;
		public long? file_size;
	}

	public class Sticker
	{
		public string file_id;
		public int width;
		public int height;
		public PhotoSize thumb;
		public string emoji;
		public long? file_size;
	}

	public class Video
	{
		public string file_id;
		public int width;
		public int height;
		public long duration;
		public PhotoSize thumb;
		public string mime_type;
		public long? file_size;
	}

	public class Voice
	{
		public string file_id;
		public int width;
		public int height;
		public long? file_size;
	}

	public class Contact
	{
		public string phone_number;
		public string first_name;
		public string last_name;
		public long? user_id;
	}

	public class Location
	{
		public float longitude;
		public float latitude;
	}

	public class Venue
	{
		public Location location;
		public string title;
		public string address;
		public string foursquare_id;
	}

	public class UserProfilePhotos
	{
		public int total_count;
		public PhotoSize[] photos;
	}

	public class File
	{
		public string file_id;
		public long? file_size;
		public string file_path;
	}

    public class Result
    {
        public long update_id = 0;
		public Message message;
		public Message edited_message;
		public Message channel_post;
		public Message edited_channel_post;
		public CallbackQuery callback_query;
	}

    public class TelegramJsonObject : TelegramJsonObject<List<Result>>
    {

    }

	public class TelegramJsonObject<T> : TelegramJsonObjectBase where T : class
	{
		public T result;
	}

	public abstract class TelegramJsonObjectBase
	{
		public bool ok;
		public int? error_code;
		public string description;
	}

	public class TelegramException : Exception
	{
		public int HttpErrorCode { get; protected set; }
		public TelegramException(TelegramJsonObjectBase obj) : base(obj.description)
		{
			if (obj.ok) throw new ArgumentException("Requested object is not containing any error.");
			HttpErrorCode = obj.error_code ?? 0;
		}
	}

	public class ReplyKeyboardMarkup : IReplyObject
	{
		public KeyboardButton[][] keyboard;
		public bool? resize_keyboard;
		public bool? one_time_keyboard;
		public bool? selective;
	}

	public class KeyboardButton
	{
		public string text;
		public bool? request_contact;
		public bool? request_location;
	}

	public class ReplyKeyboardRemove : IReplyObject
	{
		public readonly bool remove_keyboard = true;
		public bool? selective;
	}

	public class InlineKeyboardMarkup : IReplyObject
	{
		public InlineKeyboardButton[][] inline_keyboard;
	}

	public class InlineKeyboardButton
	{
		public string text;
		public string url;
		public string callback_data;
		public string switch_inline_query;
		public string switch_inline_query_current_chat;
	}

	public class CallbackQuery
	{
		public string id;
		public User from;
		public Message message;
		public string inline_message_id;
		public string chat_instance;
		public string data;
	}

	public class ForceReply : IReplyObject
	{
		public readonly bool force_reply = true;
		public bool selective = true;
	}

	public class ChatMember
	{
		public User user;
		public string status;
	}

	public class ResponseParameters
	{
		public long? migrate_to_chat_id;
		public long? retry_after;
	}

	public interface IReplyObject { }
}
