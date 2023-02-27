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

			//var chatblock =await (from c in chat.ChatsBlocks
			//				join u in chat.Users on c.UserId equals u.Id
			//				where c.UserId == u.Id && c.UserId == id
			//select c).ToListAsync();

			//var chats = await (from c in chat.ChatsBlocks
			//				   join u in chat.Users on c.UserId equals u.Id
			//				   join ch in chat.Chats on c.ChatId equals ch.Id
			//				   join m in chat.Messags on ch.Id equals m.ChatId
			//				   where c.UserId == u.Id && c.UserId != id && chatblock.Contains(c.ChatId)
			//				   && c.ChatId == ch.Id && ch.Id == m.ChatId && m.UserId == u.Id
			//				   group m by new { u.Name, ch.Id } into g
			//				   select new
			//				   {
			//					   Id = g.Key.Id,
			//					   UserName = g.Key.Name,
			//					   LastMessage = (from m in chat.Messags
			//									  where m.ChatId == g.Key.Id
			//									  orderby m.Created descending
			//									  select m.Mess_Text).FirstOrDefault(),
			//					   MessageCount = g.Count()
			//				   }).ToListAsync();



			//var realch = await (from c in chat.Chats
			//					join cb in chat.ChatsBlocks on c.Id equals cb.ChatId
			//					join u in chat.Users on cb.UserId equals u.Id
			//					join m in chat.Messags on c.Id equals m.ChatId
			//					where c.Id == cb.ChatId && cb.UserId == u.Id && c.Id == m.ChatId && m.UserId == u.Id && c.AllChatBlock.Contains(chatblock) 
			//					select cb

			//					 ).ToListAsync();

			var chatBlocksWithSameChatId = await (from cb in chat.ChatsBlocks
												  join u in chat.Users on cb.UserId equals u.Id
												  join c in chat.Chats on cb.ChatId equals c.Id
												  join m in chat.Messags on c.Id equals m.ChatId
												  where chatBlocks.Select(c => c.ChatId).Contains(cb.ChatId) && cb.UserId != id
												  group m by new { u.Name, c.Id } into g
												  orderby g.Max(x => x.Created) descending
												  select new
												  {
													  Id = g.Key.Id,
													  UserName = g.Key.Name,
													  LastMessage = (from m in chat.Messags
																	 where m.ChatId == g.Key.Id
																	 orderby m.Created descending
																	 select m.Mess_Text).FirstOrDefault(),
													  MessageCount = g.Where(x => (x.IsCheck == false && x.UserId != id)).Count(),
													  LastMessageCreated = g.Max(x => x.Created)
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


	      await chat.AddAsync(messag);
			
		   await chat.SaveChangesAsync();

			return messag;
		}



	}



	

}








