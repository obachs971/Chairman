using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public Deck playPile;
    public Bot[] players;
    public int[] initialHandCounts;
    public int turnDirection;
    public int currentPlayer;
    public int playerTakingAction;
    public bool currPlayerPlayed;
    public bool prevPlayerPlayed;
    public int highestCardValue;
    public GameObjectMain whiteCircle;
    public List<Rule> triggeredRules;
    public List<Rule> previousTriggeredRules;
}
