﻿
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


namespace WebChatBack.Controllers
{

	public class UserController : Controller
	{
		private static readonly Dictionary<string,WebSocket> activeUsers = new Dictionary<string, WebSocket>();
		string id;
	


		public static ChatContext chat;

		public UserController(ChatContext context)
		{
			chat = context;
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
					Console.WriteLine(123);
					activeUsers.TryAdd(id, webSocket);


					

				 await ChatsBlock(id);
	            }
			}

			
		}



		private static async Task ChatsBlock(string id)
		{
			var serializer = new JavaScriptSerializer(); 
			WebSocket st = activeUsers[id];
			var buffer = new byte[1024 * 4];



			ReqDb req = new ReqDb();
		    var chats = JsonData(new DataForm("GetChannels", await req.GetChatsList(chat, Convert.ToInt32(id))));

			await st.SendAsync(chats, WebSocketMessageType.Text, true, CancellationToken.None);



			var receiveResult = await st.ReceiveAsync(
				new ArraySegment<byte>(buffer), CancellationToken.None);

	
			while (!receiveResult.CloseStatus.HasValue)
			{
				
				var str = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

				Console.WriteLine(str);
				dynamic csharpPerson = serializer.Deserialize<dynamic>(str);



				switch (csharpPerson["name"])
				{
					case "GetChatById":
						{
						  buffer = JsonData(new DataForm("Messages", await req.GetChatById(chat, Convert.ToInt32(csharpPerson["object"]["Id"]))));
							
						}
							

						break;

				}






				
				await st.SendAsync(

                            new ArraySegment<byte>(buffer, 0, buffer.Length),
                            receiveResult.MessageType,
                            receiveResult.EndOfMessage,
                            CancellationToken.None);



				receiveResult = await st.ReceiveAsync(
					new ArraySegment<byte>(buffer), CancellationToken.None);
			}


			Console.WriteLine("Close Socket");

			activeUsers.Remove(id);

			await st.CloseAsync(
		  receiveResult.CloseStatus.Value,
		  receiveResult.CloseStatusDescription,
		  CancellationToken.None);



		}





		
		public static   byte[] JsonData(DataForm data)
		{

			var LeftBlock = JsonConvert.SerializeObject(data);
		    return Encoding.UTF8.GetBytes(LeftBlock);
		}






		}
}
