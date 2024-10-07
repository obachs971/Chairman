using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public Deck playPile;
    public Bot[] players;
    public int direction;
    public int currentPlayer;
    public bool hasPlayed;

    public GameState(Deck playPile, Bot[] players, int direction, int currentPlayer, bool hasPlayed)
    {
        this.playPile = playPile;
        this.players = players;
        this.direction = direction;
        this.currentPlayer = currentPlayer;
        this.hasPlayed = hasPlayed;
    }
}
