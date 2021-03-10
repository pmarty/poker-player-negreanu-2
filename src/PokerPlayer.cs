using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Nancy.Simple
{
	public static class PokerPlayer
	{
		public static readonly string VERSION = "HasPair";

		public static int BetRequest(GameState gameState)
		{
			var ownPlayer = GetOwnPlayer(gameState);

			if (HasPair(ownPlayer))
			{
				Console.WriteLine("we have a pair");
				return ownPlayer.stack;
			}

			return ownPlayer.stack;
		}

		public static void ShowDown(JObject gameState)
		{
			//TODO: Use this method to showdown
		}
		
		private static Player GetOwnPlayer(GameState gameState)
		{
			return gameState.Players[gameState.in_action];
		}

		private static bool HasPair(Player player)
		{
			var cardRank = player.hole_cards.First().rank;

			return player.hole_cards.All(c => c.rank == cardRank);
		}
	}
}