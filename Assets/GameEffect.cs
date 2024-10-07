using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriggerStuff;
using CardStuff;

public enum Effect
{
	REVERSE_TURN_ORDER,
	SKIP_NEXT_PLAYER_TURN,
	PLAY_AGAIN,
}
public abstract class GameEffect 
{
	public Effect effect;
	public List<Trigger> triggerList;
	public GameEffect(Effect effect, List<Trigger> triggerList)
	{
		this.effect = effect;
		this.triggerList = triggerList;
	}
	public abstract string toString();
	public Effect getEffectID()
	{
		return effect;
	}
	public int pickTrigger(List<Trigger> allTriggers)
	{
		triggerList.Shuffle();
		for(int i = 0; i < triggerList.Count; i++)
		{
			for(int j = 0; j < allTriggers.Count; j++)
			{
				if (triggerList[i].toString().Equals(allTriggers[j].toString()))
					return j;
			}
		}
		return -1;
	}
}
public class ReverseTurnOrder : GameEffect
{
	public ReverseTurnOrder() : base(Effect.REVERSE_TURN_ORDER, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds) })
	{

	}
	public override string toString()
	{
		return "reverse the order of play";
	}
}
public class SkipNextPlayerTurn : GameEffect
{
	public SkipNextPlayerTurn() : base(Effect.SKIP_NEXT_PLAYER_TURN, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds) })
	{

	}
	public override string toString()
	{
		return "skip the next player's turn";
	}
}
public class PlayAgain : GameEffect
{
	public PlayAgain() : base(Effect.PLAY_AGAIN, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds) })
	{

	}
	public override string toString()
	{
		return "the current player will take another turn";
	}
}
