using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CardStuff;

public class Deck  
{

    private List<Card> allCards;
    private List<Card> deck;
    private int numDecks;
	public Deck(int n, List<Card> allCards)
    {
        numDecks = n;
        this.allCards = allCards;
        reshuffleDeck();
    }
    public void reshuffleDeck()
    {
        deck = new List<Card>();
        for (int aa = 0; aa < numDecks; aa++)
        {
            foreach (Card card in allCards)
                deck.Add(card);
        }
        deck.Shuffle();
    }
    public Card Draw()
    {
        if(deck.Count > 0)
        {
            Card card = deck[0];
            deck.RemoveAt(0);
            return card;
        }
        return null;
    }
    public Card GetCard(int n)
    {
        if (n < 0 || n >= deck.Count)
            return null;
        return deck[deck.Count - (n + 1)];
    }
    public Card RemoveCard(int n)
    {
        if (n < 0 || n >= deck.Count)
            return null;
        Card removed = deck[deck.Count - (n + 1)];
        deck.RemoveAt(deck.Count - (n + 1));
        return removed;
    }
    public int getDeckSize()
    {
        return deck.Count;
    }
    public void AddToDeck(Card c)
    {
        deck.Add(c);
    }
}
