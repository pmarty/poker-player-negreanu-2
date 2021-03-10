using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nancy.Simple
{
	public static class PokerPlayer
	{
		public static readonly string VERSION = "Default C# folding player";

		public static int BetRequest(JObject gameState)
		{
			//TODO: Use this method to return the value You want to bet
			return 1000;
		}


		public static int BetRequest(GameState gameState)
		{
			//TODO: Use this method to return the value You want to bet
			return gameState.Players.Single(p => p.name == "Negreanu 2").stack;
		}

		public static void ShowDown(JObject gameState)
		{
			//TODO: Use this method to showdown
		}
	}
}