namespace WebChatBack.Models
{
	public class ChatsBlock
	{
		public	int Id { get; set; }
		public int ChatId { get; set; }

		public int UserId { get; set; }

		public User user { get; set; }

		public Chat chat { get; set; }

		public ChatsBlock()
		{
			user = new User();
				chat = new Chat();
		}


	}
}
