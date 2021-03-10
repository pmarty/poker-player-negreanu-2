using System;
using Newtonsoft.Json.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Nancy.Simple
{
	public class MainModule : NancyModule
	{
		public MainModule ()
		{
			Get ["/"] = _ => {
				var contentBytes = Encoding.UTF8.GetBytes ("OK");
				var response = new Response {
					ContentType = "text/plain",
					Contents = s => s.Write (contentBytes, 0, contentBytes.Length),
					StatusCode = HttpStatusCode.OK
				};
				return response;
			};

			Post ["/"] = parameters => {
				var form = Request.Form;
				string action = form ["action"];
				switch (action) {
				case "bet_request":
				{
					var json = JObject.Parse (form ["game_state"]);
					try
					{
						var gameState = JsonConvert.DeserializeObject<GameState>(form["game_state"]);
						var bet = PokerPlayer.BetRequest (gameState).ToString ();
						var betBytes = Encoding.UTF8.GetBytes (bet);
						var response = new Response
						{
							ContentType = "text/plain",
							Contents = s => s.Write(betBytes, 0, betBytes.Length),
							StatusCode = HttpStatusCode.OK
						};
						return response;
					}
					catch (Exception e)
					{
						Console.WriteLine(e.ToString());
						return Fallback(json);
					};
				}
				case "showdown":
				{
					try
					{
						var gameState = JsonConvert.DeserializeObject<GameState>(form["game_state"]);
						var bet = PokerPlayer.ShowDown(gameState).ToString ();
						var betBytes = Encoding.UTF8.GetBytes (bet);
						var response = new Response
						{
							ContentType = "text/plain",
							Contents = s => s.Write(betBytes, 0, betBytes.Length),
							StatusCode = HttpStatusCode.OK
						};
						return response;
					}
					catch (Exception e)
					{
						var showDownBytes = Encoding.UTF8.GetBytes ("OK");
						var response = new Response {
							ContentType = "text/plain",
							Contents = s => s.Write (showDownBytes, 0, showDownBytes.Length),
							StatusCode = HttpStatusCode.OK
						};
						return response;
					};
				}
				case "version":
				{
					var versionBytes = Encoding.UTF8.GetBytes (PokerPlayer.VERSION);
					return new Response {
						ContentType = "text/plain",
						Contents = s => s.Write (versionBytes, 0, versionBytes.Length),
						StatusCode = HttpStatusCode.OK
					};
				}
				case "check":
				{
					var contentBytes = Encoding.UTF8.GetBytes ("OK");
					var response = new Response {
						ContentType = "text/plain",
						Contents = s => s.Write (contentBytes, 0, contentBytes.Length),
						StatusCode = HttpStatusCode.OK
					};
					return response;
				}
				default:
					var bytes = Encoding.UTF8.GetBytes ("Not an allowed action or request");
					return new Response {
						ContentType = "text/plain",
						Contents = s => s.Write (bytes, 0, bytes.Length),
						StatusCode = HttpStatusCode.BadRequest
					};
				}
			};
		}

		private static Response Fallback(dynamic json)
		{
			// just go all in
			Console.WriteLine("falling back to all in");
			var bet = 1000.ToString();
			var betBytes = Encoding.UTF8.GetBytes(bet);
			var response = new Response
			{
				ContentType = "text/plain",
				Contents = s => s.Write(betBytes, 0, betBytes.Length),
				StatusCode = HttpStatusCode.OK
			};
			return response;
		}
	}
}