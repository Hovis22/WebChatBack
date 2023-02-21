﻿namespace WebChatBack.Models
{
	public class User
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public int UserPassword { get; set; }

		public List<ChatsBlock> chatsBlocks { get; set; }

		public User()
		{
			chatsBlocks = new List<ChatsBlock>();
		}



	}
}