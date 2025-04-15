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
	START_GAME,
	DRAW_CARD,
	PLAY_CARD,
	RETURN_CARD,
	CHAIRMAN_VIOLATION,
	RETURN_GAME_OBJECT,
	PLAYER_TURN_START,

	REVERSE_TURN_ORDER,
	SKIP_NEXT_PLAYER_TURN,
	PLAY_AGAIN,
	ADD_OFFSET_TO_RANK,
	SAY_PHRASE,
	FORBIDDEN_SUIT,
	TAKE_GAME_OBJECT
}
public enum XPlayerAction
{
	[Description("NULL")]
	NULL,
	[Description("the player currently taking their turn")]
	CURRENT_PLAYER,
	[Description("the player that took their turn previously")]
	PREVIOUS_PLAYER,
	[Description("the player going to take their next turn")]
	NEXT_PLAYER,
	[Description("the player opposite of the player that is currently taking their turn")]
	OPPOSITE_PLAYER,
	[Description("the player clockwise from the player that is currently taking their turn")]
	CW_OF_CURRENT,
	[Description("the player counter-clockwise from the player that is currently taking their turn")]
	CCW_OF_CURRENT,
	[Description("the player with the White Circle")]
	PLAYER_WITH_WHITE_CIRCLE,
	[Description("the player with the Blue Square")]
	PLAYER_WITH_BLUE_SQUARE,

}
public class ViolationInfo
{
	public ViolationPhrases violation;
	public Phrase phrase;
	public ViolationInfo(ViolationPhrases violation)
	{
		this.violation = violation;
		this.phrase = Phrase.NULL;
	}
	public ViolationInfo(ViolationPhrases violation, Phrase phrase)
	{
		this.violation = violation;
		this.phrase = phrase;
	}
	public string toString()
	{
		FieldInfo fieldInfo = violation.GetType().GetField(violation.ToString());
		DescriptionAttribute descAttribute = System.Attribute.GetCustomAttribute(fieldInfo, typeof(DescriptionAttribute)) as DescriptionAttribute;
		string str = descAttribute.Description;
		if(phrase != Phrase.NULL)
		{
			FieldInfo fieldInfoPhrase = phrase.GetType().GetField(phrase.ToString());
			DescriptionAttribute descAttributePhrase = System.Attribute.GetCustomAttribute(fieldInfoPhrase, typeof(DescriptionAttribute)) as DescriptionAttribute;
			str = str + ": '" + descAttributePhrase.Description + "'";
		}
		return str;
	}
}
public abstract class GameEffect 
{
	public Effect effect;
	public List<Trigger> triggerList;
	public bool isAction = false;
	public bool spawnsObj = false;
	public bool needsObj = false;
	public ViolationInfo failureToPerformAction;
	public ViolationInfo illegalPerformAction;

	public GameEffect(Effect effect, ViolationPhrases failureToPerformAction, ViolationPhrases illegalPerformAction, List<Trigger> triggerList)
	{
		this.effect = effect;
		this.failureToPerformAction = new ViolationInfo(failureToPerformAction);
		this.illegalPerformAction = new ViolationInfo(illegalPerformAction);
		this.triggerList = triggerList;
	}
	public GameEffect(Effect effect, ViolationInfo failureToPerformAction, ViolationInfo illegalPerformAction, List<Trigger> triggerList)
	{
		this.effect = effect;
		this.failureToPerformAction = failureToPerformAction;
		this.illegalPerformAction = illegalPerformAction;
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
	public ActionEffect(Effect effect, ViolationPhrases failureToPerformAction, ViolationPhrases illegalPerformAction, List<XPlayerAction> xPlayerActions, List<Trigger> triggerList) : base(effect, failureToPerformAction, illegalPerformAction, triggerList)
	{
		this.xPlayerActions = xPlayerActions;
		base.isAction = true;
	}
	public ActionEffect(Effect effect, ViolationInfo failureToPerformAction, ViolationInfo illegalPerformAction, List<XPlayerAction> xPlayerActions, List<Trigger> triggerList) : base(effect, failureToPerformAction, illegalPerformAction, triggerList)
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
	public abstract string actionString(XPlayerAction receivingPlayer);
	public abstract string failureToPerformActionString();
	public abstract string illegalActionString();
}
public abstract class GameObjectMain
{
	public int id;
	public static int gameObjCounter = 0;
	public GameObjectObj[] gameObject;
	public GameObjectMain(GameObjectObj[] gameObject)
	{
		this.id = gameObjCounter++;
		this.gameObject = gameObject;
	}
	public abstract string toString(int index);
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
		if(meshRenderer != null)
		{
			this.meshRenderer = meshRenderer;
			this.vector3Size = new Vector3(meshRenderer.transform.localScale.x, meshRenderer.transform.localScale.y, meshRenderer.transform.localScale.z);
			meshRenderer.transform.localScale = new Vector3(0f, 0f, 0f);
			meshRenderer.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		this.standingY = standingY;
		this.movingY = movingY;
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
	public Effect undoEffect;
	public bool startsOnTable;
	public bool playToTable;
	public bool playToPlayer;
	public bool mustBeOnPlayerToTrigger;
	public List<XPlayerAction> receivingPlayerList;
	public XPlayerAction receivingPlayer = XPlayerAction.NULL;
	public XPlayerAction playerWithObject;
	public ObjectEffect(Effect effect, ViolationPhrases failureToPerformAction, ViolationPhrases illegalPerformAction, Effect undoEffect, XPlayerAction playerWithObject, bool spawnsObj, bool needsObj, GameObjectMain gameObject, bool startsOnTable, bool playToTable, bool playToPlayer, bool mustBeOnPlayerToTrigger, List<XPlayerAction> xPlayerActions, List<Trigger> triggerList, List<XPlayerAction> receivingPlayerList) : base(effect, failureToPerformAction, illegalPerformAction, xPlayerActions, triggerList)
	{
		base.spawnsObj = spawnsObj;
		base.needsObj = needsObj;
		this.gameObject = gameObject;
		this.undoEffect = undoEffect;
		this.playerWithObject = playerWithObject;
		this.startsOnTable = startsOnTable;
		this.playToTable = playToTable;
		this.playToPlayer = playToPlayer;
		this.mustBeOnPlayerToTrigger = mustBeOnPlayerToTrigger;
		this.receivingPlayerList = receivingPlayerList;
	}
	public abstract bool pickReceivingPlayer(List<XPlayerAction> RPL);
	public abstract XPlayerAction[] getNewXPlayerActions();

	public abstract string undoActionString(List<int> players, string[] playerNames);
}
public class ReverseTurnOrder : GameEffect
{
	public ReverseTurnOrder() : base(Effect.REVERSE_TURN_ORDER, ViolationPhrases.FAILURE_TO_PLAY, ViolationPhrases.PLAYING_OUT_OF_TURN, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new CurrentPlayerHasXCardsAfterPlaying(1), new CurrentPlayerHasXCardsAfterPlaying(2), new CurrentPlayerHasXCardsAfterPlaying(3), new CurrentPlayerHasXCardsAfterPlaying(4), })
	{

	}
	public override string toString()
	{
		return "reverse the order of play";
	}
}
public class SkipNextPlayerTurn : GameEffect
{
	public SkipNextPlayerTurn() : base(Effect.SKIP_NEXT_PLAYER_TURN, ViolationPhrases.FAILURE_TO_PLAY, ViolationPhrases.PLAYING_OUT_OF_TURN, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new CurrentPlayerHasXCardsAfterPlaying(1), new CurrentPlayerHasXCardsAfterPlaying(2), new CurrentPlayerHasXCardsAfterPlaying(3), new CurrentPlayerHasXCardsAfterPlaying(4), })
	{

	}
	public override string toString()
	{
		return "skip the next player's turn";
	}
}
public class PlayAgain : GameEffect
{
	public PlayAgain() : base(Effect.PLAY_AGAIN, ViolationPhrases.FAILURE_TO_PLAY, ViolationPhrases.PLAYING_OUT_OF_TURN, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), })
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
	public SayPhrase(Phrase phrase, List<XPlayerAction> xPlayerActions) : base(Effect.SAY_PHRASE, new ViolationInfo(ViolationPhrases.FAILURE_TO_SAY, phrase), new ViolationInfo(ViolationPhrases.TALKING), xPlayerActions, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), })
	{
		this.phrase = phrase;
	}
	public SayPhrase(Phrase phrase, List<XPlayerAction> xPlayerActions, List<Trigger> triggers) : base(Effect.SAY_PHRASE, new ViolationInfo(ViolationPhrases.FAILURE_TO_SAY, phrase), new ViolationInfo(ViolationPhrases.TALKING), xPlayerActions, triggers)
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
	public override string actionString(XPlayerAction receivingPlayer)
	{
		FieldInfo field2 = phrase.GetType().GetField(phrase.ToString());
		DescriptionAttribute attribute2 = Attribute.GetCustomAttribute(field2, typeof(DescriptionAttribute)) as DescriptionAttribute;
		return attribute2.Description;
	}
	public override string failureToPerformActionString()
	{
		FieldInfo field2 = phrase.GetType().GetField(phrase.ToString());
		DescriptionAttribute attribute2 = Attribute.GetCustomAttribute(field2, typeof(DescriptionAttribute)) as DescriptionAttribute;
		return "Failure to say '" + attribute2.Description + "'";
	}
	public override string illegalActionString()
	{
		return "Talking";
	}
}
public class ForbiddenSuit : GameEffect
{
	public ForbiddenSuit() : base(Effect.FORBIDDEN_SUIT, ViolationPhrases.BAD_CARD, ViolationPhrases.BAD_CARD, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new CurrentPlayerHasXCardsAfterPlaying(1), new CurrentPlayerHasXCardsAfterPlaying(2), new CurrentPlayerHasXCardsAfterPlaying(3), new CurrentPlayerHasXCardsAfterPlaying(4) })
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
	public AddOffsetToRank(int offset) : base(Effect.ADD_OFFSET_TO_RANK, ViolationPhrases.BAD_CARD, ViolationPhrases.BAD_CARD, new List<Trigger>() { new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new CurrentPlayerHasXCardsAfterPlaying(1), new CurrentPlayerHasXCardsAfterPlaying(2), new CurrentPlayerHasXCardsAfterPlaying(3), new CurrentPlayerHasXCardsAfterPlaying(4) })
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
	public override string toString(int index)
	{
		return "White Circle";
	}
}
public class TakeWhiteCircle : ObjectEffect
{
	public TakeWhiteCircle(WhiteCircle WC) : base(Effect.TAKE_GAME_OBJECT, ViolationPhrases.FAILURE_TAKING_WHITE_CIRCLE, ViolationPhrases.BAD_TAKING_WHITE_CIRCLE, Effect.RETURN_GAME_OBJECT, XPlayerAction.PLAYER_WITH_WHITE_CIRCLE, true, false, WC, true, false, true, false, new List<XPlayerAction>() { XPlayerAction.CCW_OF_CURRENT, XPlayerAction.CURRENT_PLAYER, XPlayerAction.CW_OF_CURRENT, XPlayerAction.NEXT_PLAYER, XPlayerAction.OPPOSITE_PLAYER, XPlayerAction.PREVIOUS_PLAYER, XPlayerAction.PLAYER_WITH_BLUE_SQUARE }, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Spades), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Diamonds, Card.CardSuit.Diamonds), new CurrentPlayerHasXCardsAfterPlaying(1), new CurrentPlayerHasXCardsAfterPlaying(2), new CurrentPlayerHasXCardsAfterPlaying(3), new CurrentPlayerHasXCardsAfterPlaying(4) }, null)
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
		return attribute1.Description + " must take the white circle";
	}
	public override string actionString(XPlayerAction receivingPlayer)
	{
		return "*Takes the White Circle*";
	}
	public override string failureToPerformActionString()
	{
		return "Failure to take the White Circle";
	}
	public override string illegalActionString()
	{
		return "Bad taking of the White Circle";
	}
	public override string undoActionString(List<int> players, string[] playerNames)
	{
		if(players.Count == 0)
			return "Returns the White Circle to the table";
		return "*Gives the White Circle to " + playerNames[players[0]] + " player*";
	}
}
public class BlueSquare : GameObjectMain
{
	public BlueSquare(GameObjectObj[] gameObjectObjs) : base(gameObjectObjs)
	{

	}
	public override string toString(int index)
	{
		return "Blue Square";
	}
}
public class TakeBlueSquare : ObjectEffect
{
	public TakeBlueSquare(BlueSquare BS) : base(Effect.TAKE_GAME_OBJECT, ViolationPhrases.FAILURE_TAKING_BLUE_SQUARE, ViolationPhrases.BAD_TAKING_BLUE_SQUARE, Effect.RETURN_GAME_OBJECT, XPlayerAction.PLAYER_WITH_BLUE_SQUARE, true, false, BS, true, false, true, false, new List<XPlayerAction>() { XPlayerAction.CCW_OF_CURRENT, XPlayerAction.CURRENT_PLAYER, XPlayerAction.CW_OF_CURRENT, XPlayerAction.NEXT_PLAYER, XPlayerAction.OPPOSITE_PLAYER, XPlayerAction.PREVIOUS_PLAYER, XPlayerAction.PLAYER_WITH_WHITE_CIRCLE }, new List<Trigger>() { new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King), new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds), new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King), new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds), new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4), new differentSuit(), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Spades), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Diamonds, Card.CardSuit.Diamonds), new CurrentPlayerHasXCardsAfterPlaying(1), new CurrentPlayerHasXCardsAfterPlaying(2), new CurrentPlayerHasXCardsAfterPlaying(3), new CurrentPlayerHasXCardsAfterPlaying(4) }, null)
	{

	}

	public override bool pickReceivingPlayer(List<XPlayerAction> RPL)
	{
		receivingPlayer = xPlayerAction;
		return true;
	}
	public override XPlayerAction[] getNewXPlayerActions()
	{
		XPlayerAction[] xPlayerActions = { XPlayerAction.PLAYER_WITH_BLUE_SQUARE };
		return xPlayerActions;
	}

	public override string toString()
	{
		FieldInfo field1 = xPlayerAction.GetType().GetField(xPlayerAction.ToString());
		DescriptionAttribute attribute1 = Attribute.GetCustomAttribute(field1, typeof(DescriptionAttribute)) as DescriptionAttribute;
		return attribute1.Description + " must take the blue square";
	}
	public override string actionString(XPlayerAction receivingPlayer)
	{
		return "*Takes the Blue Square*";
	}
	public override string failureToPerformActionString()
	{
		return "Failure to take the Blue Square";
	}
	public override string illegalActionString()
	{
		return "Bad taking of the Blue Square";
	}
	public override string undoActionString(List<int> players, string[] playerNames)
	{
		if (players.Count == 0)
			return "Returns the Blue Square to the table";
		return "*Gives the Blue Square to " + playerNames[players[0]] + " player*";
	}
}

