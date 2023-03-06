using WebChatBack.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WebChatBack.Data 
{
	public class ChatContext : DbContext
	{
		public ChatContext(DbContextOptions<ChatContext> options) : base(options)
		{
			Database.EnsureCreated();
		}
		public DbSet<User> Users { set; get; }

		public DbSet<Chat> Chats { set; get; }
		public DbSet<Messag> Messags { set; get; }

		public DbSet<ChatsBlock> ChatsBlocks { set; get; }
	}
}
