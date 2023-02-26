using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nancy.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebChatBack.Classes;
using WebChatBack.Data;
using WebChatBack.Models;

namespace WebChatBack.Controllers
{
	
	[Route("api/[controller]")]
	[ApiController]
	public class FormController : Controller
	{
		public static ChatContext chat;
		public FormController(ChatContext context)
		{
			chat = context;
		}

		[HttpPost("login")]
		public async Task<ActionResult<string>> LoginUser([FromBody] LoginForm user)
		{
			var userInf = await chat.Users.Where(x => (x.Email == user.login && x.UserPassword.ToString() == user.password)).FirstOrDefaultAsync();
			
			if(userInf != null)
			{
				string jwtToken = await CreateToken(userInf);
				return jwtToken;
			}

			return "Unknow";
		}



		[HttpPost("register")]
		public async Task<ActionResult<string>> RegisterUser([FromBody] RegisterModel reg)

		{    

			Console.WriteLine(123);
			User user = new User(reg);
			await chat.AddAsync(user);
			await chat.SaveChangesAsync();

			string jwtToken = await CreateToken(user);
			return jwtToken;

		
		}





		private async Task<string> CreateToken(User user)
		{
			var identity = GetIdentity(user);
			var now = DateTime.UtcNow;

			var jwt = new JwtSecurityToken(
			   issuer: AuthOptions.ISSUER,
			audience: AuthOptions.AUDIENCE,
			notBefore: now,
			claims: identity.Claims,
			expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
			   signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));

			var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
			return encodedJwt;
		}




		private ClaimsIdentity GetIdentity(User user)
		{
			var claims = new List<Claim>
				{
					new Claim("Id", Convert.ToString(user.Id)),
					new Claim("Name", Convert.ToString(user.Name)),
					new Claim("Login", Convert.ToString(user.Email))
				};
			ClaimsIdentity claimsIdentity =
			new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
				ClaimsIdentity.DefaultRoleClaimType);

			return claimsIdentity;
		}





	}
}
