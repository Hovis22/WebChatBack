namespace WebChatBack.Classes
{
	public class DataForm
	{
		public string Name;

		public List<dynamic> Data;
	
	  public DataForm(string Name, List<dynamic> Data)
		{
			this.Name = Name;
			this.Data = Data; 
		}

	}
}
