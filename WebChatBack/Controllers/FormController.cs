using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Nancy.Json;
using Newtonsoft.Json;
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

			Console.WriteLine(123);
			var userInf = await chat.Users.Where(x => (x.Name == user.login && x.Password == user.password)).FirstOrDefaultAsync();
			
			if(userInf != null)
			{
				string jwtToken = await CreateToken(userInf);
				return Ok(jwtToken);
			}
			else  return BadRequest("Incorrect UserName or Login");
		}



		[HttpPost("register")]
		public async Task<ActionResult<string>> RegisterUser([FromBody] RegisterModel reg)

		{

			User existingUser = await chat.Users.FirstOrDefaultAsync(u => u.Name == reg.login || u.Email == reg.email);

			if (existingUser != null)
			{
				return BadRequest("User with this login or email already exists.");
			}



			User user = new User(reg);
			await chat.AddAsync(user);
			await chat.SaveChangesAsync();

			string jwtToken = await CreateToken(user);
			return jwtToken;
		}




		[HttpPost]
		[Route("UserEdit")]
		public async Task<ActionResult<string>> UserEdit([FromForm] IFormFile file,[FromForm] string data)
		{
			var serializer = new JavaScriptSerializer();



			var userData = JsonConvert.DeserializeObject<ChangeModel>(data);


		

			byte[] imageData = null;
			int userId = Convert.ToInt32(userData.Id);

			User user = await chat.Users.Where(x => x.Id == userId).FirstOrDefaultAsync() ;
			user.Name = userData.Name;
			user.Email = userData.Login;


			if (file.Length != 4)
			{
				using (var binarRead = new BinaryReader(file.OpenReadStream()))
				{
					imageData = binarRead.ReadBytes((int)file.Length);
				}
				user.Image = imageData;
			}

			chat.Update(user);
			await chat.SaveChangesAsync();

			string JwtToken = await CreateToken(user);

			return JwtToken;
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

			if (user.Image != null)
			{
				Claim imageClaim = new Claim("Image", Convert.ToBase64String(user.Image));
				claims.Add(imageClaim);
			}

			ClaimsIdentity claimsIdentity =
				new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);

			return claimsIdentity;
		}






	}
}
