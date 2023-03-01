using Microsoft.EntityFrameworkCore;
using System.Linq;
using WebChatBack.Data;
using WebChatBack.Models;

namespace WebChatBack.Classes
{
	public class ReqDb
	{

	 public async Task<List<dynamic>> GetChatsList(ChatContext chat,int id)
		{

			var chatBlocks = await (from cb in chat.ChatsBlocks
									where cb.UserId == id
									select cb).ToListAsync();

		


			var chatBlocksWithSameChatId = await (from cb in chat.ChatsBlocks
												  join u in chat.Users on cb.UserId equals u.Id
												  join c in chat.Chats on cb.ChatId equals c.Id into cg
												  from x in cg.DefaultIfEmpty()
												  join m in chat.Messags on x.Id equals m.ChatId into mg
												  from y in mg.OrderByDescending(m => m.Created).Take(1).DefaultIfEmpty()
												  where chatBlocks.Select(c => c.ChatId).Contains(cb.ChatId) && cb.UserId != id
												  group y by new { ChatId = x.Id, UserName = u.Name } into g
												  orderby g.Max(x => x.Created) descending
												  select new
												  {
													  Id = g.Key.ChatId,
													  UserName = g.Key.UserName,
													  LastMessage = g.FirstOrDefault().Mess_Text ?? "",
													  MessageCount = g.Where(x => (x.IsCheck == false && x.UserId != id)).Count(),
													  LastMessageCreated = g.Max(x => x.Created) == default ? DateTime.Now : g.Max(x => x.Created)
												  }).ToListAsync();


			return chatBlocksWithSameChatId.Cast<dynamic>().ToList();
		} 



    public async Task<List<dynamic>> GetChatById(ChatContext chat, int id)
	{
			var messages = await (from m in chat.Messags
						   where m.ChatId == id
						   select m).ToListAsync();

		return messages.Cast<dynamic>().ToList(); 
	}


		public async Task<List<User>> SearchChannels(ChatContext chat, string value, int id)
		{
			var userName = await (from u in chat.Users
								  where u.Name.StartsWith(value)
								  select u
								  ).ToListAsync();

			var usersWithoutCommonChats = await (from u in chat.Users
												 where !chat.ChatsBlocks.Any(cb => cb.UserId == u.Id && chat.Chats.Any(c => c.AllChatBlock.Contains(cb) && c.AllChatBlock.Any(cb2 => cb2.UserId == id)))
												 select u).ToListAsync();
			
			return usersWithoutCommonChats;
		}




		public async Task<List<int>> GetUsersInChat(ChatContext chat, int id)
		{
			var users = await (from u in chat.Users
								  join c in chat.ChatsBlocks on u.Id equals c.UserId
								  where c.UserId == u.Id && c.ChatId == id
								  select u.Id).ToListAsync();

			return users;
		}




		public async Task<dynamic> PostMessage(ChatContext chat, dynamic data)
		{
			Messag messag = new Messag();

			messag.ChatId = Convert.ToInt32(data["ChatId"]);
			messag.UserId = Convert.ToInt32(data["UserId"]);
			messag.Mess_Text = data["MessageText"];
			messag.Created = DateTime.Now;

	      await chat.AddAsync(messag);
			
		   await chat.SaveChangesAsync();

			return messag;
		}





		public async Task<dynamic> AddChatBlock(ChatContext chat, dynamic data)
		{
			var st = await AddChat(chat);
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




		public async Task<Chat> AddChat(ChatContext chat)
		{
			Console.WriteLine("\nAddChannel");
			Chat newChat = new Chat();

			newChat.Type =  "Chat";

			await chat.AddAsync(newChat);

			await chat.SaveChangesAsync();

			

			return newChat;
		}





	}



	

}








