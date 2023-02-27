namespace WebChatBack.Models
{
	public class User
	{
		public int Id { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public string Password { get; set; }

		public byte[]? Image { get; set; }

		public List<ChatsBlock>? chatsBlocks { get; set; }

		public User()
		{
			chatsBlocks = new List<ChatsBlock>();
		}


		public User(RegisterModel reg)
		{
			Name = reg.login;
			Email = reg.email;
			Password = reg.password;
		}


	}
}
