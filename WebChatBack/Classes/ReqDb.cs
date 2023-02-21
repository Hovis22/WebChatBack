using Microsoft.EntityFrameworkCore;
using WebChatBack.Data;
using WebChatBack.Models;

namespace WebChatBack.Classes
{
	public class ReqDb
	{

	 public async Task<List<dynamic>> GetChatsList(ChatContext chat,int id)
		{


			var chatblock =await (from c in chat.ChatsBlocks
							join u in chat.Users on c.UserId equals u.Id
							where c.UserId == u.Id && c.UserId == id
			select c.ChatId).ToListAsync();

			var chats = await (from c in chat.ChatsBlocks
							   join u in chat.Users on c.UserId equals u.Id
							   join ch in chat.Chats on c.ChatId equals ch.Id
							   join m in chat.Messags on ch.Id equals m.ChatId
							   where c.UserId == u.Id && c.UserId != id && chatblock.Contains(c.ChatId)
							   && c.ChatId == ch.Id && ch.Id == m.ChatId && m.UserId == u.Id
							   group m by new { u.Name, ch.Id } into g
							   select new
							   {
								   Id = g.Key.Id,
								   UserName = g.Key.Name,
								   LastMessage = g.OrderBy(x => x.Id).Last().Mess_Text,
								   MessageCount = g.Count()
							   }).ToListAsync();

			return chats.Cast<dynamic>().ToList();
		} 



    public async Task<List<dynamic>> GetChatById(ChatContext chat, int id)
	{
			var messages = await (from m in chat.Messags
						   where m.ChatId == id
						   select m).ToListAsync();

		return messages.Cast<dynamic>().ToList(); ;
	}




	}



	

}








