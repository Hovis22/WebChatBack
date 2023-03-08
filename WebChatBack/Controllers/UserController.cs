
using WebChatBack.Classes;
using WebChatBack.Data;
using WebChatBack.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nancy;
using Nancy.Json;
using Newtonsoft.Json;
using System;
using System.Data.Common;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using WebChatBack.Services;

namespace WebChatBack.Controllers
{

	public class UserController : Controller
	{
		private static readonly Dictionary<string,WebSocket> activeUsers = new Dictionary<string, WebSocket>();
		string id;



		private readonly  ChatService chat;



		public UserController(ChatService chatService)
		{
			chat = chatService;
		}






		[HttpGet("/hi")]
		public async Task Index()
		{
			if (HttpContext.WebSockets.IsWebSocketRequest)
			{
				using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
				id = HttpContext.Request.Query["id"];
				if (!activeUsers.ContainsKey(id))
				{
					activeUsers.TryAdd(id, webSocket);

				 await ChatsBlock(id);
	            }
			}

			
		}



		private async Task ChatsBlock(string id)
		{	
			
			try
			{
                WebSocket st = activeUsers[id];
				var serializer = new JavaScriptSerializer();
			
				var buffer = new byte[1024 * 4];

				await chat.SetStatuOnline(Convert.ToInt32(id));

				var chats = await JsonData(new DataForm("GetChannels", await chat.GetChatsList(Convert.ToInt32(id))));

				await st.SendAsync(chats, WebSocketMessageType.Text, true, CancellationToken.None);


				var bufStart = new byte[1024 * 4];

				bufStart = await JsonData(new DataForm("SetOnline", id));

              
				foreach (int user in await chat.SearchUserWith(Convert.ToInt32(id)))
				{
					if (activeUsers.ContainsKey(user.ToString()))
					{

						await activeUsers[user.ToString()].SendAsync(bufStart,WebSocketMessageType.Text,true,CancellationToken.None);
					}
				}








				var receiveResult = await st.ReceiveAsync(
					new ArraySegment<byte>(buffer), CancellationToken.None);


				while (!receiveResult.CloseStatus.HasValue)
				{
					var sendbuffer = new byte[1024 * 4];

					var str  = Encoding.ASCII.GetString(buffer);

					Console.WriteLine(str);
					dynamic csharpPerson = serializer.Deserialize<dynamic>(str);



					switch (csharpPerson["name"])
					{
						case "GetChatById":
							{
								sendbuffer = await JsonData(new DataForm("Messages", await chat.GetChatById(Convert.ToInt32(csharpPerson["object"]["Id"]))));


								await st.SendAsync(

											new ArraySegment<byte>(sendbuffer, 0, sendbuffer.Length),
											receiveResult.MessageType,
											receiveResult.EndOfMessage,
											CancellationToken.None);



							}


							break;
						case "PostMess":
							{
								DataForm dataForm = new DataForm("NewMessage", await chat.PostMessage( csharpPerson["object"]));

								sendbuffer = await JsonData(dataForm);
							
						       foreach(int user in await chat.GetUsersInChat( Convert.ToInt32(csharpPerson["object"]["ChatId"])))
								{

									if (activeUsers.ContainsKey(user.ToString()))
									{
									
										await activeUsers[user.ToString()].SendAsync(
										new ArraySegment<byte>(sendbuffer, 0, sendbuffer.Length),
										receiveResult.MessageType,
										receiveResult.EndOfMessage,
										CancellationToken.None);
									}
								}
							}
							break;
						case "ChangeMess":
							{
								DataForm dataForm = new DataForm("ChangeMess", await chat.ChangeMess( csharpPerson["object"]));

								sendbuffer = await JsonData(dataForm);

								foreach (int user in await chat.GetUsersInChat( Convert.ToInt32(csharpPerson["object"]["ChatId"])))
								{

									if (activeUsers.ContainsKey(user.ToString()))
									{

										await activeUsers[user.ToString()].SendAsync(
										new ArraySegment<byte>(sendbuffer, 0, sendbuffer.Length),
										receiveResult.MessageType,
										receiveResult.EndOfMessage,
										CancellationToken.None);
									}
								}
							}
							break;
						case "DeleteMes":
							{
								chat.DeleteMess( csharpPerson["object"]);
								DataForm dataForm = new DataForm("DeleteMes", csharpPerson["object"]["MessId"]);
								Console.WriteLine(csharpPerson["object"]["MessId"]);
								sendbuffer = await JsonData(dataForm);

								foreach (int user in await chat.GetUsersInChat( Convert.ToInt32(csharpPerson["object"]["ChatId"])))
								{

									if (activeUsers.ContainsKey(user.ToString()))
									{

										await activeUsers[user.ToString()].SendAsync(
										new ArraySegment<byte>(sendbuffer, 0, sendbuffer.Length),
										receiveResult.MessageType,
										receiveResult.EndOfMessage,
										CancellationToken.None);
									}
								}
							}
							break;


						case "SearchChannels":
							{
								DataForm dataForm = new DataForm("ChannelsFound", await chat.SearchChannels(csharpPerson["object"]["value"], Convert.ToInt32(csharpPerson["object"]["userId"])));

								sendbuffer = await JsonData(dataForm);

								await st.SendAsync(new ArraySegment<byte>(sendbuffer, 0, sendbuffer.Length),
																		receiveResult.MessageType,
																		receiveResult.EndOfMessage,
																		CancellationToken.None);
							}
							break;
						case "CreateChannel":
							{
								await chat.AddChatBlock(csharpPerson["object"]);
								DataForm dataForm = new DataForm("AddChannel", await chat.GetChatsList(Convert.ToInt32(id)));

								sendbuffer = await JsonData(dataForm);

								await st.SendAsync(new ArraySegment<byte>(sendbuffer, 0, sendbuffer.Length),
																		receiveResult.MessageType,
																		receiveResult.EndOfMessage,
																		CancellationToken.None);
							}
							break;
						case "UpdateImg":
							{
								await chat.AddChatBlock( csharpPerson["object"]);
								DataForm dataForm = new DataForm("AddChannel", await chat.GetChatsList(Convert.ToInt32(id)));

								sendbuffer = await JsonData(dataForm);

								await st.SendAsync(new ArraySegment<byte>(sendbuffer, 0, sendbuffer.Length),
																		receiveResult.MessageType,
																		receiveResult.EndOfMessage,
																		CancellationToken.None);
							}
							break;


					}





					sendbuffer = null;


					receiveResult = await st.ReceiveAsync(
						new ArraySegment<byte>(buffer), CancellationToken.None);
				
				
				}

				var bufEnd = new byte[1024 * 4];

				await chat.SetStatuOffline(Convert.ToInt32(id));

				bufEnd = await JsonData(new DataForm("SetOffline", id));




				await st.CloseAsync(
				  receiveResult.CloseStatus.Value,
				  receiveResult.CloseStatusDescription,
				  CancellationToken.None);



               activeUsers.Remove(id);


				foreach (int user in await chat.SearchUserWith(Convert.ToInt32(id)))
				{
					if (activeUsers.ContainsKey(user.ToString()))
					{

						await activeUsers[user.ToString()].SendAsync(bufEnd, WebSocketMessageType.Text, true, CancellationToken.None);
					}
				}





			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}

			Console.WriteLine("Close Socket");

			

	


		}






		public static async  Task<byte[]> JsonData(DataForm data)
		{

			var LeftBlock =  JsonConvert.SerializeObject(data);
		    return Encoding.UTF8.GetBytes(LeftBlock);
		}






		}
}
