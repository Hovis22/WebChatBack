namespace WebChatBack.Models
{
	public class Messag
	{

		public int Id { get; set; }

		public int ChatId { get; set; }

		public int UserId { get; set; }

		public string Mess_Text { get; set; }

		public DateTime Created { get; set; }

		public bool IsCheck { get; set; }



	}
}
