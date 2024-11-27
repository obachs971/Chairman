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
        public onRank(Card.CardRank rank) : base()
        {
            this.rank = rank;
        }
        public override bool HasTriggered(GameState gameState)
        {
            return gameState.currPlayerPlayed && gameState.playPile.GetCard(0).getRank() == rank;
        }
        public override string toString()
        {
            return "If a " + rank + " is played, ";
        }
    }
    public class onSuit : Trigger
    {
        private Card.CardSuit suit;
        public onSuit(Card.CardSuit suit) : base()
        {
            this.suit = suit;
        }
        public override bool HasTriggered(GameState gameState)
        {
            return gameState.currPlayerPlayed && gameState.playPile.GetCard(0).getSuit() == suit;
        }
        public override string toString()
        {
            return "If a " + suit + " is played, ";
        }
    }
    public class prevCardIsRank : Trigger
    {
        private Card.CardRank rank;
        public prevCardIsRank(Card.CardRank rank) : base()
        {
            this.rank = rank;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.GetCard(1) == null)
                return false;
            return gameState.currPlayerPlayed && gameState.playPile.GetCard(1).getRank() == rank;
        }
        public override string toString()
        {
            return "If a card is played on a " + rank + ", ";
        }
    }
    public class prevCardIsSuit : Trigger
    {
        private Card.CardSuit suit;
        public prevCardIsSuit(Card.CardSuit suit) : base()
        {
            this.suit = suit;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.GetCard(1) == null)
                return false;
            return gameState.currPlayerPlayed && gameState.playPile.GetCard(1).getSuit() == suit;
        }
        public override string toString()
        {
            return "If a card is played on a " + suit + ", ";
        }
    }
    public class differenceOfTwoCards : Trigger
    {
        private int difference;
        public differenceOfTwoCards(int difference) : base()
        {
            this.difference = difference;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.GetCard(1) == null)
                return false;
            int rank1 = (int)gameState.playPile.GetCard(1).getRank() - 1;
            int rank2 = (int)gameState.playPile.GetCard(0).getRank() - 1;
            int count = gameState.highestCardValue;
            if (((rank2 + difference) % count) == rank1)
                return gameState.currPlayerPlayed;
            if (((rank1 + difference) % count) == rank2)
                return gameState.currPlayerPlayed;
            return false;
        }
        public override string toString()
        {
            return "If the rank of the current card and the previous card have a difference of " + difference + " (wrapping around), ";
        }
    }
    public class sameSuit : Trigger
    {
        public sameSuit() : base()
        {

        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.GetCard(1) == null)
                return false;
            return gameState.currPlayerPlayed && gameState.playPile.GetCard(1).getSuit() == gameState.playPile.GetCard(0).getSuit();
        }
        public override string toString()
        {
            return "If the suit of the current card and the previous card are the same, ";
        }
    }
    public class differentSuit : Trigger
    {
        public differentSuit() : base()
        {

        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.GetCard(1) == null)
                return false;
            return gameState.currPlayerPlayed && gameState.playPile.GetCard(1).getSuit() != gameState.playPile.GetCard(0).getSuit();
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
        public SuitWithSuit(Card.CardSuit suit1, Card.CardSuit suit2) : base()
        {
            this.suit1 = suit1;
            this.suit2 = suit2;
        }
        public override bool HasTriggered(GameState gameState)
        {
            if (gameState.playPile.GetCard(1) == null)
                return false;
            if (gameState.playPile.GetCard(1).getSuit() == suit1 && gameState.playPile.GetCard(0).getSuit() == suit2)
                return gameState.currPlayerPlayed;
            if (gameState.playPile.GetCard(1).getSuit() == suit2 && gameState.playPile.GetCard(0).getSuit() == suit1)
                return gameState.currPlayerPlayed;
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
        public CurrentPlayerHasXCards(int numCards) : base()
        {
            this.numCards = numCards;
        }
        public override bool HasTriggered(GameState gameState)
        {
            return gameState.initialHandCounts[gameState.currentPlayer] == numCards;
        }
        public override string toString()
        {
            return numCards == 1 ?  "If the current player has exactly 1 card left in their hand before drawing/playing, " : "If the current player has exactly " + numCards + " cards left in their hand before drawing/playing, ";
        }
    }
    public class XPlayerIndex
    {
        public int getPlayerIndex(XPlayerAction playerAction, GameState gameState, int turnCounter, int turnDirection, int nextTurnCounter, int numPlayers, int prevPlayer, int lastPlayerPassed, int lastPlayerPlayed)
        {
            switch (playerAction)
            {
                case XPlayerAction.NULL: return 0;
                case XPlayerAction.CURRENT_PLAYER: return turnCounter;
                case XPlayerAction.CW_OF_CURRENT: return mod(turnCounter + 1, numPlayers);
                case XPlayerAction.CCW_OF_CURRENT: return mod(turnCounter - 1, numPlayers);
                case XPlayerAction.OPPOSITE_PLAYER: return mod(turnCounter + (numPlayers / 2), numPlayers);
                case XPlayerAction.NEXT_PLAYER: return mod(nextTurnCounter + turnDirection, numPlayers);
                case XPlayerAction.PREVIOUS_PLAYER: return prevPlayer;
                case XPlayerAction.LAST_PLAYER_PASSED: return lastPlayerPassed;
                case XPlayerAction.LAST_PLAYER_PLAYED: return lastPlayerPlayed;
                case XPlayerAction.PLAYER_WITH_WHITE_CIRCLE:
                    if(gameState.whiteCircle != null)
                    {
                        for (int i = 0; i < numPlayers; i++)
                        {
                            if (gameState.players[i].hasObject(gameState.whiteCircle))
                                return i;
                        }
                    }
                    return -1;
            }
            return -1;
        }
        int mod(int n, int m)
        {
            return (n % m + m) % m;
        }
    }
}

