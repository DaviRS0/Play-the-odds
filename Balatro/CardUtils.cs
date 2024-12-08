using System;
using System.Collections.Generic;
using System.Linq;

namespace Balatro
{
    public static class CardUtils
    {
        public static int CalculateScore(List<Card> cards)
        {
            var selectedCards = cards.Where(card => card.IsSelected).ToList();
            if (selectedCards.Count < 2) return 0; // Ensure at least two cards are selected

            if (IsFlush(selectedCards)) return CalculateFlushScore(selectedCards);
            if (IsFullHouse(selectedCards)) return CalculateFullHouseScore(selectedCards);
            if (IsThreeOfAKind(selectedCards)) return CalculateThreeOfAKindScore(selectedCards);
            if (IsTwoPair(selectedCards)) return CalculateTwoPairScore(selectedCards);
            if (IsPair(selectedCards)) return CalculatePairScore(selectedCards);

            // If no poker hand is matched, return 0
            return 0;
        }

        public static int CalculateHighCardScore(List<Card> cards)
        {
            return cards.Sum(card => CardValues[card.Name.Split('_')[1]]);
        }

        public static List<string> Suits => new List<string> { "Hearts", "Spades" };
        public static List<string> Ranks => new List<string> { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };

        public static Dictionary<string, int> CardValues = new Dictionary<string, int>
        {
            { "2", 2 },
            { "3", 3 },
            { "4", 4 },
            { "5", 5 },
            { "6", 6 },
            { "7", 7 },
            { "8", 8 },
            { "9", 9 },
            { "10", 10 },
            { "J", 11 },
            { "Q", 12 },
            { "K", 13 },
            { "A", 14 }
        };

        public static List<string> CreateDeck()
        {
            var deck = new List<string>();
            foreach (var suit in Suits)
            {
                foreach (var rank in Ranks)
                {
                    deck.Add($"{suit}_{rank}");
                }
            }
            return deck;
        }

        public static List<string> DealHand(List<string> deck, int handSize = 8)
        {
            var random = new Random();
            return deck.OrderBy(x => random.Next()).Take(handSize).ToList();
        }

        private static bool IsFlush(List<Card> cards)
        {
            return cards.Count == 5 && cards.GroupBy(card => card.Name.Split('_')[0]).Any(group => group.Count() == 5);
        }

        private static bool IsFullHouse(List<Card> cards)
        {
            var groups = cards.GroupBy(card => card.Name.Split('_')[1]).ToList();
            return groups.Count == 2 && (groups.Any(g => g.Count() == 3) && groups.Any(g => g.Count() == 2));
        }

        private static bool IsThreeOfAKind(List<Card> cards)
        {
            return cards.GroupBy(card => card.Name.Split('_')[1]).Any(group => group.Count() == 3);
        }

        private static bool IsTwoPair(List<Card> cards)
        {
            return cards.GroupBy(card => card.Name.Split('_')[1]).Count(group => group.Count() == 2) == 2;
        }

        private static bool IsPair(List<Card> cards)
        {
            return cards.GroupBy(card => card.Name.Split('_')[1]).Any(group => group.Count() == 2);
        }

        private static int CalculateFlushScore(List<Card> cards)
        {
            int baseScore = 15;
            int cardValueSum = cards.Sum(card => CardValues[card.Name.Split('_')[1]]);
            return (baseScore + cardValueSum) * 4; // Apply multiplier of 4
        }

        private static int CalculateFullHouseScore(List<Card> cards)
        {
            int baseScore = 15;
            int cardValueSum = cards.Sum(card => CardValues[card.Name.Split('_')[1]]);
            return (baseScore + cardValueSum) * 3; // Apply multiplier of 3
        }

        private static int CalculateThreeOfAKindScore(List<Card> cards)
        {
            int baseScore = 10;
            int cardValueSum = cards.Sum(card => CardValues[card.Name.Split('_')[1]]);
            return (baseScore + cardValueSum) * 3; // Apply multiplier of 3
        }

        private static int CalculateTwoPairScore(List<Card> cards)
        {
            int baseScore = 5;
            int cardValueSum = cards.Sum(card => CardValues[card.Name.Split('_')[1]]);
            return (baseScore + cardValueSum) * 2; // Apply multiplier of 2
        }

        private static int CalculatePairScore(List<Card> cards)
        {
            int baseScore = 3;
            var pairGroup = cards.GroupBy(card => card.Name.Split('_')[1]).First(group => group.Count() == 2);
            int pairValueSum = pairGroup.Sum(card => CardValues[card.Name.Split('_')[1]]);
            return (baseScore + pairValueSum) * 2; // Apply multiplier of 2
        }
    }
}