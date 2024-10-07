using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CardStuff
{
    public class Card
    {
        public enum CardSuit
        {
            Spades = 1,
            Hearts = 2,
            Clubs = 3,
            Diamonds = 4
        }
        public enum CardRank
        {
            Ace = 1,
            Two = 2,
            Three = 3,
            Four = 4,
            Five = 5,
            Six = 6,
            Seven = 7,
            Eight = 8,
            Nine = 9,
            Ten = 10,
            Jack = 11,
            Queen = 12,
            King = 13
        }
        public CardRank rank;
        public CardSuit suit;
        public Card(CardRank rank, CardSuit suit)
        {
            this.rank = rank;
            this.suit = suit;
        }
        public string toString()
        {
            return rank + " of " + suit;
        }
    }
}

