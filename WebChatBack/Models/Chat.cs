namespace WebChatBack.Models
{
	public class Chat
	{
		public int Id { get; set; }

		public string Type { get; set; }

		public List<Messag>? AllMessages { get; set; }


		public Chat()
		{
			AllMessages = new List<Messag>();
		}

	}
}
