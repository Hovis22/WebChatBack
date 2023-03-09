using Microsoft.EntityFrameworkCore;
using WebChatBack.Data;
using WebChatBack.Models;

namespace WebChatBack.Services
{
	public class ChatService : IChatServise
	{
		private readonly ChatContext chat;

		public ChatService(ChatContext chatContext)
		{
			chat = chatContext;
		}


	public async Task<List<dynamic>> GetChatsList(int id)
		{

			var chatBlocks = await (from cb in chat.ChatsBlocks
									where cb.UserId == id
									select cb).ToListAsync();



			var chatBlocksWithSameChatId = await (from cb in chat.ChatsBlocks
												  join u in chat.Users on cb.UserId equals u.Id
												  join c in chat.Chats on cb.ChatId equals c.Id into cg
												  from x in cg.DefaultIfEmpty()
												  join m in chat.Messags on x.Id equals m.ChatId into mg
												  from y in mg.OrderByDescending(m => m.Created).DefaultIfEmpty()
												  where chatBlocks.Select(c => c.ChatId).Contains(cb.ChatId) && cb.UserId != id
												  group y by new { ChatId = x.Id, UserName = u.Name, UserImage = u.Image, UserStatus = u.Status, UserLastTimeOnline = u.LastTimeOnline, UserId = u.Id } into g
												  orderby g.Max(x => x.Created) descending
												  select new
												  {
													  Id = g.Key.ChatId,
													  UserName = g.Key.UserName,
													  UserId = g.Key.UserId,
													  UserImage = g.Key.UserImage,
													  UserStatus = g.Key.UserStatus,
													  UserLastTimeOnline = g.Key.UserLastTimeOnline,
													  LastMessage = g.OrderBy(x => x.Created).Last().Mess_Text ?? "",
													  MessageCount = g.Where(x => (x.IsCheck == false && x.UserId != id)).Count(),
													  LastMessageCreated = g.Max(x => x.Created) == default ? null : g.Max(x => x.Created),
												  }).ToListAsync();



			return chatBlocksWithSameChatId.Cast<dynamic>().ToList();
		}



		public async Task<List<dynamic>> GetChatById(int id,int userId)
		{
			var ChangeStatusmessages = await (from m in chat.Messags
								  where m.ChatId == id && m.UserId == userId && m.IsCheck == false
								  select m).ToListAsync();

			foreach(var change in ChangeStatusmessages)
			{
				change.IsCheck = true;

			}

		 await chat.SaveChangesAsync();


			var messages = await(from m in chat.Messags
								 where m.ChatId == id
								 select m).ToListAsync();

			return messages.Cast<dynamic>().ToList();
		}





		public async Task<Chat> AddChat()
		{
			Chat newChat = new Chat();

			newChat.Type = "Chat";

			await chat.AddAsync(newChat);

			await chat.SaveChangesAsync();



			return newChat;
		}

		public async Task<dynamic> AddChatBlock(dynamic data)
		{
			var st = await AddChat();
			ChatsBlock chatsBlock1 = new ChatsBlock();
			chatsBlock1.UserId = Convert.ToInt32(data["OwnUserId"]);
			chatsBlock1.ChatId = st.Id;

			ChatsBlock chatsBlock2 = new ChatsBlock();

			chatsBlock2.UserId = Convert.ToInt32(data["UserToAdd"]);
			chatsBlock2.ChatId = st.Id;

			await chat.AddAsync(chatsBlock1);
			await chat.AddRangeAsync(new List<ChatsBlock> { chatsBlock1, chatsBlock2 });



			await chat.SaveChangesAsync();

			return null;
		}

		public async Task<dynamic> ChangeMess(dynamic data)
		{
			int mesid = Convert.ToInt32(data["MessId"]);
			var messag = chat.Messags.Where(x => x.Id == mesid).FirstOrDefault();


			messag.Mess_Text = data["MessageText"];


			await chat.SaveChangesAsync();

			return messag;
		}

		public void DeleteMess(dynamic data)
		{
			int mesid = Convert.ToInt32(data["MessId"]);
			var messag = chat.Messags.Where(x => x.Id == mesid).FirstOrDefault();


			chat.Messags.Remove(messag);


			chat.SaveChanges();
		}

	
		public async Task<List<int>> GetUsersInChat(int id)
		{
			var users = await(from u in chat.Users
							  join c in chat.ChatsBlocks on u.Id equals c.UserId
							  where c.UserId == u.Id && c.ChatId == id
							  select u.Id).ToListAsync();

			return users;
		}

		public async Task<dynamic> PostMessage(dynamic data)
		{
			Messag messag = new Messag();

			messag.ChatId = Convert.ToInt32(data["ChatId"]);
			messag.UserId = Convert.ToInt32(data["UserId"]);
			messag.Mess_Text = data["MessageText"];
			messag.Created = DateTime.Now;
			messag.IsCheck = false;

			await chat.AddAsync(messag);

			await chat.SaveChangesAsync();

			return messag;
		}

		public async Task<List<User>> SearchChannels(string value, int id)
		{
			var usersWithoutCommonChats = await(from u in chat.Users
												where u.Name.StartsWith(value) && u.Id != id && !chat.ChatsBlocks.Any(cb => cb.UserId == u.Id && chat.Chats.Any(c => c.AllChatBlock.Contains(cb) && c.AllChatBlock.Any(cb2 => cb2.UserId == id)))
												select u).ToListAsync();

			return usersWithoutCommonChats;
		}

		public async Task<List<int>> SearchUserWith(int id)
		{

			var usersWithCommonChats = await(from u in chat.Users
											 where u.Id != id && chat.Chats.Any(c => c.AllChatBlock.Any(cb => cb.UserId == u.Id && cb.UserId != id))
											 select u.Id).ToListAsync();

			return usersWithCommonChats;
		}

		public async Task<string> SetStatuOffline(int id)
		{
			var user = chat.Users.Where(x => x.Id == id).FirstOrDefault();
			user.Status = false;

			user.LastTimeOnline = DateTime.Now;

			chat.Users.Update(user);
			await chat.SaveChangesAsync();
			return null;
		}

		public async Task<string> SetStatuOnline(int id)
		{
			var user = chat.Users.Where(x => x.Id == id).FirstOrDefault();
			user.Status = true;

			chat.Users.Update(user);

			await chat.SaveChangesAsync();

			return null;
		}


		public async Task<string> SetMessageIsChecked(int id)
		{
			var messag = chat.Messags.Where(x => x.Id == id).FirstOrDefault();
			messag.IsCheck = true;

			chat.Messags.Update(messag);

			await chat.SaveChangesAsync();

			return null;
		}



		public async Task<string> DeleteChat(int id)
		{
			var chatToDel = chat.Chats.Where(x => x.Id == id).FirstOrDefault();

			chat.Remove(chatToDel);


			await chat.SaveChangesAsync();

			return null;
		}


	}
}
