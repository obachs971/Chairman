using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TriggerStuff;
using CardStuff;
using System.ComponentModel;
using System;
using System.Reflection;

public enum Effect
{
	REVERSE_TURN_ORDER,
	SKIP_NEXT_PLAYER_TURN,
	PLAY_AGAIN,
	ADD_OFFSET_TO_RANK,
	SAY_PHRASE,
	FORBIDDEN_SUIT,
	TAKE_WHITE_CIRCLE,
}
public enum Phrase
{
	[Description("Have a nice day")]
	HAVE_A_NICE_DAY = 0,
}
public enum XPlayerAction
{
	[Description("NULL")]
	NULL,
	[Description("the player currently taking their turn")]
	CURRENT_PLAYER,
	[Description("the player before the current player in turn order")]
	PREVIOUS_PLAYER,
	[Description("the player going to take their next turn")]
	NEXT_PLAYER,
	[Description("the player opposite of the player that is currently taking their turn")]
	OPPOSITE_PLAYER,
	[Description("the player clockwise from the player that is currently taking their turn")]
	CW_OF_CURRENT,
	[Description("the player counter-clockwise from the player that is currently taking their turn")]
	CCW_OF_CURRENT,
	[Description("the last player that didn't play a card from their hand to the play pile during their turn")]
	LAST_PLAYER_PASSED,
	[Description("the last player that played a card from their hand to the play pile during their turn")]
	LAST_PLAYER_PLAYED,
	[Description("the player with the white circle")]
	PLAYER_WITH_WHITE_CIRCLE
}
public abstract class GameEffect 
{
	public Effect effect;
	public List<Trigger> triggerList;
	public bool isAction = false;
	public bool spawnsObj = false;
	public bool needsObj = false;
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
	public Trigger getTrigger(string triggerStr)
	{
		foreach(Trigger trigger in triggerList)
		{
			if (trigger.toString().Equals(triggerStr))
				return trigger;
		}
		return null;
	}
}
public abstract class ActionEffect : GameEffect
{
	public string str;
	public XPlayerAction xPlayerAction = XPlayerAction.NULL;
	private List<XPlayerAction> xPlayerActions;
	public Trigger[] newTriggers = null;
	public XPlayerAction[] newXPlayerActions = null;
	public ActionEffect(Effect effect, List<XPlayerAction> xPlayerActions, List<Trigger> triggerList) : base(effect, triggerList)
	{
		this.xPlayerActions = xPlayerActions;
		base.isAction = true;
	}
	public bool pickXPlayerAction(List<XPlayerAction> availableXPlayerActions)
	{
		xPlayerActions.Shuffle();
		foreach(XPlayerAction xPlayerAction in xPlayerActions)
		{
			foreach (XPlayerAction availableXPlayerAction in availableXPlayerActions)
			{
				if(availableXPlayerAction == xPlayerAction)
				{
					this.xPlayerAction = xPlayerAction;
					return true;
				}
			}
		}
		return false;
	}
}
public class GameObjectMain
{
	public int id;
	public static int gameObjCounter = 0;
	public GameObjectObj[] gameObject;
	public GameObjectMain(GameObjectObj[] gameObject)
	{
		this.id = gameObjCounter++;
		this.gameObject = gameObject;
	}
}
public class GameObjectObj
{
	public MeshRenderer meshRenderer;
	public Vector3 vector3Size;
	public float standingY;
	public float movingY;
	public bool[][] shape;
	public bool[][] playerPlacement;
	public bool[][] tablePlacement;

	public GameObjectObj(MeshRenderer meshRenderer, bool[][] shape, float standingY, float movingY)
	{
		this.meshRenderer = meshRenderer;
		this.vector3Size = new Vector3(meshRenderer.transform.localScale.x, meshRenderer.transform.localScale.y, meshRenderer.transform.localScale.z);
		this.standingY = standingY;
		this.movingY = movingY;
		meshRenderer.transform.localScale = new Vector3(0f, 0f, 0f);
		meshRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);

		this.shape = shape;
		tablePlacement = null;
		playerPlacement = null;
	}
	public void makeObjectVisible()
	{
		meshRenderer.transform.localScale = vector3Size;
	}
	public void changeObjectLocation(Vector3 newLocation)
	{
		meshRenderer.transform.localPosition = newLocation;
	}
}
public abstract class ObjectEffect : ActionEffect
{
	public GameObjectMain gameObject;
	public bool startsOnTable;
	public bool playToTable;
	public bool playToPlayer;
	public bool mustBeOnPlayerToTrigger;
	public List<XPlayerAction> receivingPlayerList;
	public XPlayerAction receivingPlayer = XPlayerAction.NULL;
	public ObjectEffect(Effect effect, bool spawnsObj, bool needsObj, GameObjectMain gameObject, bool startsOnTable, bool playToTable, bool playToPlayer, bool mustBeOnPlayerToTrigger, List<XPlayerAction> xPlayerActions, List<Trigger> triggerList, List<XPlayerAction> receivingPlayerList) : base(effect, xPlayerActions, triggerList)
	{
		base.spawnsObj = spawnsObj;
		base.needsObj = needsObj;
		this.gameObject = gameObject;
		this.startsOnTable = startsOnTable;
		this.playToTable = playToTable;
		this.playToPlayer = playToPlayer;
		this.mustBeOnPlayerToTrigger = mustBeOnPlayerToTrigger;
		this.receivingPlayerList = receivingPlayerList;
	}
	public abstract bool pickReceivingPlayer(List<XPlayerAction> RPL);
	public abstract XPlayerAction[] getNewXPlayerActions();
	
}
public class ReverseTurnOrder : GameEffect
{
	public ReverseTurnOrder() : base(Effect.REVERSE_TURN_ORDER, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new CurrentPlayerHasXCards(1), new CurrentPlayerHasXCards(2), new CurrentPlayerHasXCards(3), new CurrentPlayerHasXCards(4), })
	{

	}
	public override string toString()
	{
		return "reverse the order of play";
	}
}
public class SkipNextPlayerTurn : GameEffect
{
	public SkipNextPlayerTurn() : base(Effect.SKIP_NEXT_PLAYER_TURN, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new CurrentPlayerHasXCards(1), new CurrentPlayerHasXCards(2), new CurrentPlayerHasXCards(3), new CurrentPlayerHasXCards(4), })
	{

	}
	public override string toString()
	{
		return "skip the next player's turn";
	}
}
public class PlayAgain : GameEffect
{
	public PlayAgain() : base(Effect.PLAY_AGAIN, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), })
	{

	}
	public override string toString()
	{
		return "the current player will take another turn";
	}
}
public class SayPhrase : ActionEffect
{
	public Phrase phrase;
	public SayPhrase(Phrase phrase, List<XPlayerAction> xPlayerActions) : base(Effect.SAY_PHRASE, xPlayerActions, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), })
	{
		this.phrase = phrase;
	}
	public SayPhrase(Phrase phrase, List<XPlayerAction> xPlayerActions, List<Trigger> triggers) : base(Effect.SAY_PHRASE, xPlayerActions, triggers)
	{
		this.phrase = phrase;
	}
	public override string toString()
	{
		FieldInfo field1 = base.xPlayerAction.GetType().GetField(base.xPlayerAction.ToString());
		DescriptionAttribute attribute1 = Attribute.GetCustomAttribute(field1, typeof(DescriptionAttribute)) as DescriptionAttribute;
		FieldInfo field2 = phrase.GetType().GetField(phrase.ToString());
		DescriptionAttribute attribute2 = Attribute.GetCustomAttribute(field2, typeof(DescriptionAttribute)) as DescriptionAttribute;
		return attribute1.Description + " must say the following phrase: " + attribute2.Description;
	}
}
public class ForbiddenSuit : GameEffect
{
	public ForbiddenSuit() : base(Effect.FORBIDDEN_SUIT, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new CurrentPlayerHasXCards(1), new CurrentPlayerHasXCards(2), new CurrentPlayerHasXCards(3), new CurrentPlayerHasXCards(4) })
	{

	}
	public override string toString()
	{
		return "the played card must not be the same suit as the previous card";
	}
}
public class AddOffsetToRank : GameEffect
{
	public int offset;
	public AddOffsetToRank(int offset) : base(Effect.ADD_OFFSET_TO_RANK, new List<Trigger>() { new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new CurrentPlayerHasXCards(1), new CurrentPlayerHasXCards(2), new CurrentPlayerHasXCards(3), new CurrentPlayerHasXCards(4) })
	{
		this.offset = offset;
	}
	public override string toString()
	{
		return "the rank of the played card is altered by this offset: " + offset;
	}
}
public class WhiteCircle : GameObjectMain
{
	public WhiteCircle(GameObjectObj[] gameObjectObjs) : base(gameObjectObjs)
	{
		
	}
}
public class TakeWhiteCircle : ObjectEffect
{
	public TakeWhiteCircle(WhiteCircle WC) : base(Effect.TAKE_WHITE_CIRCLE, true, false, WC, true, false, true, false, new List<XPlayerAction>() { XPlayerAction.CCW_OF_CURRENT, XPlayerAction.CURRENT_PLAYER, XPlayerAction.CW_OF_CURRENT, XPlayerAction.LAST_PLAYER_PASSED, XPlayerAction.LAST_PLAYER_PLAYED, XPlayerAction.NEXT_PLAYER, XPlayerAction.OPPOSITE_PLAYER, XPlayerAction.PREVIOUS_PLAYER }, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Spades), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Diamonds, Card.CardSuit.Diamonds), new CurrentPlayerHasXCards(1), new CurrentPlayerHasXCards(2), new CurrentPlayerHasXCards(3), new CurrentPlayerHasXCards(4) }, null)
	{

	}

	public override bool pickReceivingPlayer(List<XPlayerAction> RPL)
	{
		receivingPlayer = xPlayerAction;
		return true;
	}
	public override XPlayerAction[] getNewXPlayerActions()
	{
		XPlayerAction[] xPlayerActions = { XPlayerAction.PLAYER_WITH_WHITE_CIRCLE };
		return xPlayerActions;
	}

	public override string toString()
	{
		FieldInfo field1 = xPlayerAction.GetType().GetField(xPlayerAction.ToString());
		DescriptionAttribute attribute1 = Attribute.GetCustomAttribute(field1, typeof(DescriptionAttribute)) as DescriptionAttribute;
		return attribute1.Description + " must take the white circular token";
	}
}

