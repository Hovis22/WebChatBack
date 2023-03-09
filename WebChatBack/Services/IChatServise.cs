using WebChatBack.Data;
using WebChatBack.Models;

namespace WebChatBack.Services
{
	public interface IChatServise
	{
		 Task<List<dynamic>> GetChatsList( int id);

		Task<List<dynamic>> GetChatById( int id,int userId);

		Task<List<User>> SearchChannels(string value, int id);

		Task<List<int>> SearchUserWith(int id);

		Task<List<int>> GetUsersInChat( int id);

		Task<dynamic> PostMessage(dynamic data);

		Task<dynamic> ChangeMess(dynamic data);

		void DeleteMess( dynamic data);

		Task<string> SetStatuOnline( int id);

		Task<string> SetStatuOffline( int id);

		Task<dynamic> AddChatBlock( dynamic data);

		Task<Chat> AddChat();


	}
}
