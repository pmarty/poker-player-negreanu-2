using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Nancy.Simple
{
    public static class PokerPlayer
    {
        public static readonly string VERSION = "fix showdown";

        public static int BetRequest(GameState gameState)
        {
            var ownPlayer = GetOwnPlayer(gameState);
            var communityCards = gameState.community_cards;
            var currentBuyIn = gameState.current_buy_in;
            var allCards = ownPlayer.hole_cards.Concat(gameState.community_cards).ToList();

            if (communityCards.Count != 0)
            {
                var betAmountForStraights = GetBetAmountForStraights(allCards, ownPlayer, communityCards, currentBuyIn);

                if (betAmountForStraights.HasValue)
                {
                    Console.WriteLine("we have some straight to bet on");
                    return betAmountForStraights.Value;
                }
            }

            if (GetMaxSameOfAKindCount(allCards) > 2)
            {
                Console.WriteLine("we have" + GetMaxSameOfAKindCount(allCards) + " same of a kind");
                return ownPlayer.stack;
            }

            if (HasTwoPairs(ownPlayer, communityCards))
            {
                Console.WriteLine("we have two pairs");
                return ownPlayer.stack;
            }

            var betAmountForFlushes = GetBetAmountForFlushes(ownPlayer, allCards, communityCards, currentBuyIn);
            if (betAmountForFlushes.HasValue)
            {
                Console.WriteLine("we're betting on a flush");
                return betAmountForFlushes.Value;
            }

            if (HasPair(ownPlayer) && !HasCommunityCards(communityCards))
            {
                Console.WriteLine("we have a pair on our hands");
                return HasAnyVeryLowCard(ownPlayer)
                    ? GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn)
                    : GetCallOrAllIn(ownPlayer, currentBuyIn, communityCards);
            }

            if (IsSuited(ownPlayer) && !communityCards.Any())
            {
                if (communityCards.Count == 0 && HasSequenceWithoutCommunityCards(ownPlayer))
                {
                    Console.WriteLine("we have suited sequence");
                    GetCallOrAllIn(ownPlayer, currentBuyIn, communityCards);
                }

                if (HasTwoGoodCards(ownPlayer))
                {
                    Console.WriteLine("we have suited high cards");
                    GetCallOrAllIn(ownPlayer, currentBuyIn, communityCards);
                }

                if (HasAnyGoodCard(ownPlayer))
                {
                    Console.WriteLine("we have suited good card");
                    return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
                }
            }

            if (communityCards.Count == 0 && HasSequenceWithoutCommunityCards(ownPlayer))
            {
                Console.WriteLine("we a sequence on our hands");
                return HasAnyGoodCard(ownPlayer)
                    ? GetCallOrAllIn(ownPlayer, currentBuyIn, communityCards)
                    : GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
            }

            if (HasTopPairWithCommunityCards(ownPlayer, communityCards))
            {
                Console.WriteLine("we have a top pair with community cards");
                return GetCallOrAllIn(ownPlayer, currentBuyIn, communityCards);
            }

            if (HasTwoPairWithCommunityCards(ownPlayer, communityCards))
            {
                Console.WriteLine("we have two pair with community cards");
                return GetCallOrAllIn(ownPlayer, currentBuyIn, communityCards);
            }

            if (HasSetWithCommunityCards(ownPlayer, communityCards))
            {
                Console.WriteLine("we have a set with community cards");
                return GetCallOrAllIn(ownPlayer, currentBuyIn, communityCards);
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

            return GetCallAmountIfVeryLow(ownPlayer, currentBuyIn);
        }

        private static Player GetOwnPlayer(GameState gameState)
        {
            return gameState.Players[gameState.in_action];
        }

        private static bool HasTwoPairs(Player player, List<Card> communityCards)
        {
            var firstCardRank = player.hole_cards.First().Rank;
            var secondCardRank = player.hole_cards.Last().Rank;

            if (communityCards.Any())
            {
                var hasCommunityRank1 = communityCards.Any(c => c.Rank == firstCardRank);
                var hasCommunityRank2 = communityCards.Any(c => c.Rank == secondCardRank);

                return hasCommunityRank1 && hasCommunityRank2;
            }

            return false;
        }

        private static int GetMaxSameOfAKindCount(List<Card> cards)
        {
            return cards.GroupBy(c => c.Rank).OrderByDescending(g => g.Count()).First().Count();
        }

        private static int? GetBetAmountForFlushes(Player ownPlayer, List<Card> allCards, List<Card> communityCards, int currentBuyIn)
        {
            var maxSameSuit = GetMaxSameSuit(ownPlayer, allCards);

            if (maxSameSuit > 2)
            {
                if (maxSameSuit >= 5)
                {
                    return ownPlayer.stack;
                }

                if (maxSameSuit == 4 && communityCards.Count < 4)
                {
                    return ownPlayer.stack;
                }

                if (maxSameSuit == 4 && communityCards.Count < 5)
                {
                    return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
                }

                if (communityCards.Count < 4)
                {
                    return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
                }
            }

            return null;
        }

        private static int? GetBetAmountForStraights(List<Card> allCards, Player ownPlayer, List<Card> communityCards, int currentBuyIn)
        {
            var maxSequence = GetLongestSequence(allCards).Count;

            if (maxSequence >= 5)
            {
                return ownPlayer.stack;
            }

            if (maxSequence == 4 && communityCards.Count < 4)
            {
                return ownPlayer.stack;
            }
                
            if (maxSequence == 4 && communityCards.Count < 5)
            {
                return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
            }

            if (maxSequence == 3 && communityCards.Count < 4)
            {
                return GetCallAmountIfNotTooHigh(ownPlayer, currentBuyIn);
            }

            return null;
        }

        private static int GetMaxSameSuit(Player player, List<Card> cards)
        {
            var suitOne = player.hole_cards.First().Suit;
            var suitTwo = player.hole_cards.Last().Suit;

            return Math.Max(cards.Count(c => c.Suit == suitOne), cards.Count(c => c.Suit == suitTwo));
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

        private static bool HasTwoPairWithCommunityCards(Player player, List<Card> communityCards)
        {
            var firstCardRank = player.hole_cards.First().Rank;
            var secondCardRank = player.hole_cards.Last().Rank;

            return communityCards.Any(c => c.Rank == firstCardRank) &&
                   communityCards.Any(c => c.Rank == secondCardRank);
        }

        private static bool HasSetWithCommunityCards(Player player, List<Card> communityCards)
        {
            var firstCardRank = player.hole_cards.First().Rank;
            var secondCardRank = player.hole_cards.Last().Rank;

            if (communityCards.Any())
            {
                var firstCardRankCommunityCardCount = communityCards.Count(c => c.Rank == firstCardRank);
                var secondCardRankCommunityCardCount = communityCards.Count(c => c.Rank == secondCardRank);

                return firstCardRankCommunityCardCount > 1 || secondCardRankCommunityCardCount > 1;
            }

            return false;
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

        private static bool IsStraightFlush(List<Card> cards)
        {
            var longestSequence = GetLongestSequence(cards);
            return longestSequence.Count == 5 && longestSequence.Select(c => c.Suit).Distinct().Count() == 1;
        }

        private static bool IsStraight(List<Card> cards)
        {
            return GetLongestSequence(cards).Count == 5;
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

        private static bool HasSequenceWithoutCommunityCards(Player player)
        {
            return GetLongestSequence(player.hole_cards).Count > 1;
        }

        private static bool HasCommunityCards(IList<Card> communityCards)
        {
            return communityCards.Any();
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
            var maxShare = 0.35;
            var amountToCall = currentBuyIn - player.bet;

            var share = (double) amountToCall / (double) player.stack;
            if (share > maxShare)
            {
                return 0;
            }

            return amountToCall;
        }

        private static int GetCallAmountIfVeryLow(Player player, int currentBuyIn)
        {
            var amountToCall = currentBuyIn - player.bet;
            if (amountToCall < 20)
            {
                return amountToCall;
            }

            return 0;
        }

        private static bool IsLastRound(List<Card> communityCards)
        {
            return communityCards.Count == 5;
        }

        private static int GetCallOrAllIn(Player ownPlayer, int currentBuyIn, List<Card> communityCards)
        {
            if (IsLastRound(communityCards))
            {
                return ownPlayer.stack;
            }

            return GetCallAmount(ownPlayer, currentBuyIn);
        }
    }
}