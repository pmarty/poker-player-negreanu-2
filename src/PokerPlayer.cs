using System;
using System.Linq;
using Newtonsoft.Json;

namespace Nancy.Simple
{
    public static class PokerPlayer
    {
        public static readonly string VERSION = "Logging";

        public static int BetRequest(GameState gameState)
        {
            Console.WriteLine("Request Gamestate: " + JsonConvert.SerializeObject(gameState));
            var ownPlayer = GetOwnPlayer(gameState);

            if (HasPair(ownPlayer))
            {
                Console.WriteLine("we have a pair");
                return ownPlayer.stack;
            }

            if (IsSuited(ownPlayer))
            {
                Console.WriteLine("we have suited cards");
                return ownPlayer.stack;
            }

            if (HasAnyGoodCard(ownPlayer))
            {
                Console.WriteLine("we have a good card");
                return ownPlayer.stack;
            }

            return 0;
        }

        public static void ShowDown(GameState gameState)
        {
            Console.WriteLine("Final Gamestate: " + JsonConvert.SerializeObject(gameState));
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

        private static bool IsSuited(Player player)
        {
            var suit = player.hole_cards.First().suit;

            return player.hole_cards.All(c => c.suit == suit);
        }

        private static bool HasAnyGoodCard(Player player)
        {
            return player.hole_cards.Any(c => c.rank == "J" || c.rank == "Q" || c.rank == "K" || c.rank == "A");
        }
    }
}