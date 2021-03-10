using System;

namespace Nancy.Simple
{
    public class Card
    {
        public string rank { get; set; }
        public string suit { get; set; }
        public Suit Suit => (Suit) Enum.Parse(typeof(Suit), suit, ignoreCase: true);
        public Rank Rank
        {
            get
            {
                var isNumber = int.TryParse(rank, out var parsedRank);
                if (isNumber)
                {
                    return (Rank) parsedRank;
                }

                switch (rank)
                {
                    case "J": return Rank.Jack;
                    case "Q": return Rank.Queen;
                    case "K": return Rank.King;
                    case "A": return Rank.Ace;
                    default:
                        throw new ArgumentOutOfRangeException("unexpected rank:" + rank);
                }
            }
        }
    }
}