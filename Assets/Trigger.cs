using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardStuff;

namespace TriggerStuff
{
    public abstract class Trigger
    {
        public abstract bool HasTriggered(GameState gameState);
        public abstract string toString();
    }
    public class onRank : Trigger
    {
        private Card.CardRank rank;
        public onRank(Card.CardRank rank)
        {
            this.rank = rank;
        }
        public override bool HasTriggered(GameState gameState)
        {
            return gameState.hasPlayed && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 1).rank == rank;
        }
        public override string toString()
        {
            return "When a " + rank + " is played, ";
        }
    }
    public class onSuit : Trigger
    {
        private Card.CardSuit suit;
        public onSuit(Card.CardSuit suit)
        {
            this.suit = suit;
        }
        public override bool HasTriggered(GameState gameState)
        {
            return gameState.hasPlayed && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 1).suit == suit;
        }
        public override string toString()
        {
            return "When a " + suit + " is played, ";
        }
    }
    public class prevCardIsRank : Trigger
    {
        private Card.CardRank rank;
        public prevCardIsRank(Card.CardRank rank)
        {
            this.rank = rank;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2) == null)
                return false;
            return gameState.hasPlayed && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2).rank == rank;
        }
        public override string toString()
        {
            return "If the previous played card is a " + rank + ", ";
        }
    }
    public class prevCardIsSuit : Trigger
    {
        private Card.CardSuit suit;
        public prevCardIsSuit(Card.CardSuit suit)
        {
            this.suit = suit;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2) == null)
                return false;
            return gameState.hasPlayed && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2).suit == suit;
        }
        public override string toString()
        {
            return "If the previous played card is a " + suit + ", ";
        }
    }
    public class differenceOfTwoCards : Trigger
    {
        private int difference;
        public differenceOfTwoCards(int difference)
        {
            this.difference = difference;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2) == null)
                return false;
            int rank1 = (int)gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2).rank - 1;
            int rank2 = (int)gameState.playPile.getCard(gameState.playPile.getDeckSize() - 1).rank - 1;
            int count = Enum.GetNames(typeof(Card.CardRank)).Length;
            if (((rank2 + difference) % count) == rank1)
                return gameState.hasPlayed;
            if (((rank1 + difference) % count) == rank2)
                return gameState.hasPlayed;
            return false;
        }
        public override string toString()
        {
            return "If the rank of the current card and the previous card have a difference of " + difference + " (wrapping around), ";
        }
    }
    public class sameSuit : Trigger
    {
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2) == null)
                return false;
            return gameState.hasPlayed && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2).suit == gameState.playPile.getCard(gameState.playPile.getDeckSize() - 1).suit;
        }
        public override string toString()
        {
            return "If the suit of the current card and the previous card are the same, ";
        }
    }
    public class differentSuit : Trigger
    {
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2) == null)
                return false;
            return gameState.hasPlayed && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2).suit != gameState.playPile.getCard(gameState.playPile.getDeckSize() - 1).suit;
        }
        public override string toString()
        {
            return "If the suit of the current card and the previous card are different, ";
        }
    }
    public class SuitWithSuit : Trigger
    {
        Card.CardSuit suit1;
        Card.CardSuit suit2;
        public SuitWithSuit(Card.CardSuit suit1, Card.CardSuit suit2)
        {
            this.suit1 = suit1;
            this.suit2 = suit2;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2) == null)
                return false;
            if (!gameState.hasPlayed)
                return false;
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2).suit == suit1 && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 1).suit == suit2)
                return gameState.hasPlayed;
            if (gameState.playPile.getCard(gameState.playPile.getDeckSize() - 2).suit == suit2 && gameState.playPile.getCard(gameState.playPile.getDeckSize() - 1).suit == suit1)
                return gameState.hasPlayed;
            return false;
        }
        public override string toString()
        {
            return "If both the current and previous card are a " + suit1 + " and a " + suit2 + ", ";
        }
    }
    public class CurrentPlayerHasXCards : Trigger
    {
        int numCards;
        public CurrentPlayerHasXCards(int numCards)
        {
            this.numCards = numCards;
        }
        public override bool HasTriggered(GameState gameState)
        {
            return gameState.players[gameState.currentPlayer].getHandCount() == numCards;
        }
        public override string toString()
        {
            return numCards == 1 ?  "If the current player has exactly 1 card left in their hand, " : "If the current player has exactly " + numCards + " cards left in their hand, ";
        }
    }
}

