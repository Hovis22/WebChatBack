using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebChatBack.Classes
{

	public class AuthOptions
	{
		public const string ISSUER = "Messanger";
		public const string AUDIENCE = "Messanger";
		const string KEY = "phahwawaeewewedsdkey";
		public const int LIFETIME = 10;
		public static SymmetricSecurityKey GetSymmetricSecurityKey()
		{
			return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(KEY));
		}
	}

}
