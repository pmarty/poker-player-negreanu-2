using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nancy.Simple
{
    public static class PokerPlayer
    {
        public static readonly string VERSION = "longestsequence";

        public static int BetRequest(GameState gameState)
        {
            Console.WriteLine("Request Gamestate: " + JsonConvert.SerializeObject(gameState));
            var ownPlayer = GetOwnPlayer(gameState);
            var communityCards = gameState.community_cards;
            var currentBuyIn = gameState.current_buy_in;

            if (HasPair(ownPlayer))
            {
                return HasAnyVeryLowCard(ownPlayer)
                    ? GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn)
                    : ownPlayer.stack;
            }

            if (IsSuited(ownPlayer))
            {
                if (HasSequence(ownPlayer, gameState.community_cards))
                {
                    Console.WriteLine("we have suited sequence");
                    return ownPlayer.stack;
                }

                if (HasTwoGoodCards(ownPlayer))
                {
                    Console.WriteLine("we have suited high cards");
                    return ownPlayer.stack;
                }

                if (HasAnyGoodCard(ownPlayer))
                {
                    Console.WriteLine("we have suited good card");
                    // return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
                }
            }

            if (HasSequence(ownPlayer, gameState.community_cards))
            {
                return HasAnyGoodCard(ownPlayer)
                    ? ownPlayer.stack
                    : GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
            }
            
            if (HasPair(ownPlayer) && HasPairWithCommunityCards(ownPlayer, communityCards))
            {
                Console.WriteLine("we have a set with community cards");
                return ownPlayer.stack;
            }

            if (HasTopPairWithCommunityCards(ownPlayer, communityCards))
            {
                Console.WriteLine("we have a top pair with community cards");
                return ownPlayer.stack;
            }

            if (HasPairWithCommunityCards(ownPlayer, communityCards))
            {
                Console.WriteLine("we have a pair with community cards");
                return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
            }

            if (HasAnyGoodCard(ownPlayer) && !HasAnyLowCard(ownPlayer))
            {
                Console.WriteLine("we have a good card and now low card");
                return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
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
        
        private static bool HasPairWithCommunityCards(Player player, List<Card> communityCards)
        {
            var firstCardRank = player.hole_cards.First().rank;
            var secondCardRank = player.hole_cards.Last().rank;

            return communityCards.Any(c => c.rank == firstCardRank || c.rank == secondCardRank);
        }
        
        private static bool HasTopPairWithCommunityCards(Player player, List<Card> communityCards)
        {
            var firstCardRank = player.hole_cards.First().Rank;
            var secondCardRank = player.hole_cards.Last().Rank;

            if (communityCards.Any())
            {
                var topCommunityRank = communityCards.Select(c => c.Rank).OrderByDescending(r => r).First();

                return topCommunityRank == firstCardRank || topCommunityRank == secondCardRank;
            }

            return false;
        }

        private static bool IsSuited(Player player)
        {
            var suit = player.hole_cards.First().suit;

            return player.hole_cards.All(c => c.suit == suit);
        }

        private static bool HasAnyGoodCard(Player player)
        {
            return player.hole_cards.Any(c => c.Rank > Rank.Ten);
        }
        
        private static bool HasTwoGoodCards(Player player)
        {
            return player.hole_cards.All(c => c.Rank > Rank.Ten);
        }

        private static bool HasAnyLowCard(Player player)
        {
            return player.hole_cards.Any(c => c.Rank < Rank.Ten);
        }
        
        private static bool HasAnyVeryLowCard(Player player)
        {
            return player.hole_cards.Any(c => c.Rank < Rank.Seven);
        }
        
        private static bool HasAnyReallyLowCard(Player player)
        {
            return player.hole_cards.Any(c => c.Rank < Rank.Five);
        }

        private static bool HasSequence(Player player, List<Card> communityCards)
        {
            return GetLongestSequence(player.hole_cards.Concat(communityCards).ToList()).Count > 0;
        }

        private static List<Card> GetLongestSequence(List<Card> cards)
        {
            var cardsSorted = cards.OrderBy(c => c.Rank).ToArray();
            //start and length are the "current" values and max are the max found
            int start = 0, length = 0, maxstart = 0, maxlength = 0;
            //loop through array (starting from index 1 to avoid out of bounds)
            for (int i = 1; i < cardsSorted.Length; i++)
            {
                //check if current sequence is longer than previously recorded
                if (length > maxlength)
                {
                    maxstart = start;
                    maxlength = length;
                }

                if (cardsSorted[i - 1].Rank + 1 == cardsSorted[i].Rank)
                {
                    //if the current element isn't part of the current sequence, then start a new sequence
                    if (start + length < i)
                    {
                        start = i - 1;
                        length = 2;
                    }
                    else
                    {
                        //count the length
                        length++;
                    }
                }
            }

            if (maxlength == 0)
            {
                return new List<Card>();
            }

            return cardsSorted.Skip(maxstart).Take(maxlength).ToList();
        }

        private static int GetCallAmount(Player player, int currentBuyIn)
        {
            return currentBuyIn - player.bet;
        }
        
        private static int GetCallAmountIfNotTooHigh(Player player, int currentBuyIn)
        {
            var maxShare = 0.25;
            var amountToCall = currentBuyIn - player.bet;
            
            var share = (double)amountToCall / (double)player.stack;
            if (share > maxShare)
            {
                return 0;
            }

            return amountToCall;
        }
    }
}