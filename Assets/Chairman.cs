using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardStuff;
using TriggerStuff;
using System.Linq;
using System.ComponentModel;
using System.Reflection;

public enum ViolationPhrases
{
	[Description("Failure to say")]
	FAILURE_TO_SAY = int.MinValue,
	[Description("Failure to play when the play button was pressed")]
	FAILURE_TO_PLAY = 0,
	[Description("Playing out of turn")]
	PLAYING_OUT_OF_TURN = 1,
	[Description("Bad card")]
	BAD_CARD = 2,
	[Description("Talking")]
	TALKING = 3,
	[Description("Failure to take the White Circle")]
	FAILURE_TAKING_WHITE_CIRCLE = 4,
	[Description("Bad taking of the White Circle")]
	BAD_TAKING_WHITE_CIRCLE = 5,
	[Description("Failure to take the Blue Square")]
	FAILURE_TAKING_BLUE_SQUARE = 6,
	[Description("Bad taking of the Blue Square")]
	BAD_TAKING_BLUE_SQUARE = 7,

}
public enum Phrase
{
	[Description("NULL")]
	NULL = int.MinValue,
	[Description("Have a nice day")]
	HAVE_A_NICE_DAY = 0,
}

public enum PFOButtonName
{
	PREV,
	PLAY,
	NEXT
}
public class Chairman : MonoBehaviour
{
	private readonly int RATE_OF_MOVING_OBJECT = 50;
	private readonly int RATE_OF_DRAWING_TO_START_GAME = 25;
	private readonly float BUFFER_BETWEEN_ACTIONS = 0.1f;
	private int STARTING_NUM_CARDS = 7;

	private static int moduleIdCounter = 1;
	private string moduleNameLog;
	private string[] playerNames = { "Top-Left", "Top-Middle", "Top-Right", "Bottom-Right", "Bottom-Middle", "Bottom-Left" };

	private int nextTurnCounter;
	public KMBombModule module;
	public new KMAudio audio;

	private Deck drawPile;
	private Deck playPile;

	private int[] PFOHandCounts;
	private int PFOIndex;

	private Bot[] players;
	private int turnCounter;
	//private int botThoughtTurnCounter;
	private List<int> thoughtTurnCounters;
	private int turnDirection;

	private List<Card> cardList;
	private List<Card> validCards;

	public KMSelectable playButton;
	public MeshRenderer playButtonMesh;
	public TextMesh playButtonText;

	public KMSelectable pfoButton;
	public MeshRenderer pfoButtonMesh;
	public TextMesh pfoButtonText;

	public MeshRenderer[] playerMats;
	public MeshRenderer[] playerActionMats;
	public Material activeMat;
	public Material inactiveMat;

	public TextMesh[] playerText;

	public MeshRenderer drawPileMesh;
	public MeshRenderer playPileMesh;

	public MeshRenderer animateCard;
	public Material faceDownMat;

	public KMSelectable prevPFOButton;
	public KMSelectable playPFOButton;
	public KMSelectable nextPFOButton;

	public MeshRenderer prevPFOButtonMesh;
	public MeshRenderer playPFOButtonMesh;
	public MeshRenderer nextPFOButtonMesh;

	public AudioClip introduction;
	public AudioClip introCW;
	public AudioClip introCCW;

	public AudioClip[] violationAudioClips;

	public Material[] cardMatList;
	private Dictionary<string, Material> cardMats;

	public AudioClip[] player1Phrases;
	public AudioClip[] player2Phrases;
	public AudioClip[] player3Phrases;
	public AudioClip[] player4Phrases;
	public AudioClip[] player5Phrases;
	public AudioClip[] player6Phrases;

	public AudioClip[] chairmanViolationToSayPhrases;

	private AudioClip[][] playerPhrases;

	public MeshRenderer[] tableGridMeshRenderer;
	public MeshRenderer[] playerGridMeshRenderer;

	private Vector3[][] tableGridVector3;
	private Vector3[][][] playerGridVector3;

	private Color inactiveText = Color.white;
	private Color activeText = Color.black;

	private int MAX_NUM_RULES = 6;
	private int MAX_DECK_SIZE_FOR_DRAW_PILE = 10;
	private bool HAVE_PLAY_ON_REPEAT;

	private List<Rule> rules;
	
	private List<Rule> triggeredValidCardRules;
	private List<Rule> triggeredCardChangingRules;
	private List<Rule> previousTriggeredRules;
	private List<GameObjectMain> gameObjs;

	private bool prevPlayerPlayed;
	private bool currPlayerPlayed;
	//private bool willTryAgain;
	private int HIGHEST_CARD_VALUE;
	private int LOWEST_CARD_VALUE;
	//private List<Effect> triggeredEffectEnums;
	private bool hasInitPlayerDecisions = false;
	private int prevPlayer;
	private int[] handCountsAfterDrawPlay;

	private List<GameRound> gameRounds;
	private GameRound lastRoundBeforePFO;

	public MeshRenderer whiteCircleMeshRenderer;
	private WhiteCircle whiteCircle = null;

	public MeshRenderer blueSquareMeshRenderer;
	public BlueSquare blueSquare = null;

	private Dictionary<PFOButtonName, Vector3> PFOButtonPosDict;

	void Awake()
	{
		int moduleId = moduleIdCounter++;
		moduleNameLog = "[Mao #" + moduleId + "]";
		PFOButtonPosDict = new Dictionary<PFOButtonName, Vector3>();
		PFOButtonPosDict.Add(PFOButtonName.PREV, new Vector3(prevPFOButtonMesh.transform.localPosition.x, prevPFOButtonMesh.transform.localPosition.y, prevPFOButtonMesh.transform.localPosition.z));
		PFOButtonPosDict.Add(PFOButtonName.PLAY, new Vector3(playPFOButtonMesh.transform.localPosition.x, playPFOButtonMesh.transform.localPosition.y, playPFOButtonMesh.transform.localPosition.z));
		PFOButtonPosDict.Add(PFOButtonName.NEXT, new Vector3(nextPFOButtonMesh.transform.localPosition.x, nextPFOButtonMesh.transform.localPosition.y, nextPFOButtonMesh.transform.localPosition.z));
		prevPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		playPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		nextPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		initGameObjects();
		playerPhrases = new AudioClip[][] { player1Phrases, player2Phrases, player3Phrases, player4Phrases, player5Phrases, player6Phrases };
		tableGridVector3 = new Vector3[][]
		{
			new Vector3[]{ tableGridMeshRenderer[0].transform.localPosition, tableGridMeshRenderer[1].transform.localPosition, tableGridMeshRenderer[2].transform.localPosition, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), tableGridMeshRenderer[9].transform.localPosition, tableGridMeshRenderer[10].transform.localPosition, tableGridMeshRenderer[11].transform.localPosition, tableGridMeshRenderer[12].transform.localPosition },
			new Vector3[]{ tableGridMeshRenderer[3].transform.localPosition, tableGridMeshRenderer[4].transform.localPosition, tableGridMeshRenderer[5].transform.localPosition, new Vector3(-0.024f, 0f, -0.02f), new Vector3(-0.006f, 0f, -0.02f), tableGridMeshRenderer[13].transform.localPosition, tableGridMeshRenderer[14].transform.localPosition, tableGridMeshRenderer[15].transform.localPosition, tableGridMeshRenderer[16].transform.localPosition },
			new Vector3[]{ tableGridMeshRenderer[6].transform.localPosition, tableGridMeshRenderer[7].transform.localPosition, tableGridMeshRenderer[8].transform.localPosition, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), tableGridMeshRenderer[17].transform.localPosition, tableGridMeshRenderer[18].transform.localPosition, tableGridMeshRenderer[19].transform.localPosition, tableGridMeshRenderer[20].transform.localPosition }
		};
		playerGridVector3 = new Vector3[][][]
		{
			new Vector3[][]{ new Vector3[]{ playerGridMeshRenderer[0].transform.localPosition, playerGridMeshRenderer[1].transform.localPosition }, new Vector3[]{ playerGridMeshRenderer[2].transform.localPosition, playerGridMeshRenderer[3].transform.localPosition } },
			new Vector3[][]{ new Vector3[]{ playerGridMeshRenderer[4].transform.localPosition, playerGridMeshRenderer[5].transform.localPosition }, new Vector3[]{ playerGridMeshRenderer[6].transform.localPosition, playerGridMeshRenderer[7].transform.localPosition } },
			new Vector3[][]{ new Vector3[]{ playerGridMeshRenderer[8].transform.localPosition, playerGridMeshRenderer[9].transform.localPosition }, new Vector3[]{ playerGridMeshRenderer[10].transform.localPosition, playerGridMeshRenderer[11].transform.localPosition } },
			new Vector3[][]{ new Vector3[]{ playerGridMeshRenderer[12].transform.localPosition, playerGridMeshRenderer[13].transform.localPosition }, new Vector3[]{ playerGridMeshRenderer[14].transform.localPosition, playerGridMeshRenderer[15].transform.localPosition } },
			new Vector3[][]{ new Vector3[]{ playerGridMeshRenderer[16].transform.localPosition, playerGridMeshRenderer[17].transform.localPosition }, new Vector3[]{ playerGridMeshRenderer[18].transform.localPosition, playerGridMeshRenderer[19].transform.localPosition } },
			new Vector3[][]{ new Vector3[]{ playerGridMeshRenderer[20].transform.localPosition, playerGridMeshRenderer[21].transform.localPosition }, new Vector3[]{ playerGridMeshRenderer[22].transform.localPosition, playerGridMeshRenderer[23].transform.localPosition } }
		};



		players = new Bot[6];
		for (int aa = 0; aa < players.Length; aa++)
			players[aa] = new Bot();
		gameObjs = new List<GameObjectMain>();
		List<bool> playsToTable = new List<bool>();
		List<bool> playsToPlayer = new List<bool>();



		//Generate cards
		cardList = new List<Card>()
		{
			new Card(Card.CardRank.Ace, Card.CardSuit.Spades),new Card(Card.CardRank.Two, Card.CardSuit.Spades), new Card(Card.CardRank.Three, Card.CardSuit.Spades),new Card(Card.CardRank.Four, Card.CardSuit.Spades),new Card(Card.CardRank.Five, Card.CardSuit.Spades),new Card(Card.CardRank.Six, Card.CardSuit.Spades),new Card(Card.CardRank.Seven, Card.CardSuit.Spades),new Card(Card.CardRank.Eight, Card.CardSuit.Spades),new Card(Card.CardRank.Nine, Card.CardSuit.Spades),new Card(Card.CardRank.Ten, Card.CardSuit.Spades),new Card(Card.CardRank.Jack, Card.CardSuit.Spades),new Card(Card.CardRank.Queen, Card.CardSuit.Spades),new Card(Card.CardRank.King, Card.CardSuit.Spades),
			new Card(Card.CardRank.Ace, Card.CardSuit.Hearts),new Card(Card.CardRank.Two, Card.CardSuit.Hearts), new Card(Card.CardRank.Three, Card.CardSuit.Hearts),new Card(Card.CardRank.Four, Card.CardSuit.Hearts),new Card(Card.CardRank.Five, Card.CardSuit.Hearts),new Card(Card.CardRank.Six, Card.CardSuit.Hearts),new Card(Card.CardRank.Seven, Card.CardSuit.Hearts),new Card(Card.CardRank.Eight, Card.CardSuit.Hearts),new Card(Card.CardRank.Nine, Card.CardSuit.Hearts),new Card(Card.CardRank.Ten, Card.CardSuit.Hearts),new Card(Card.CardRank.Jack, Card.CardSuit.Hearts),new Card(Card.CardRank.Queen, Card.CardSuit.Hearts),new Card(Card.CardRank.King, Card.CardSuit.Hearts),
			new Card(Card.CardRank.Ace, Card.CardSuit.Clubs),new Card(Card.CardRank.Two, Card.CardSuit.Clubs), new Card(Card.CardRank.Three, Card.CardSuit.Clubs),new Card(Card.CardRank.Four, Card.CardSuit.Clubs),new Card(Card.CardRank.Five, Card.CardSuit.Clubs),new Card(Card.CardRank.Six, Card.CardSuit.Clubs),new Card(Card.CardRank.Seven, Card.CardSuit.Clubs),new Card(Card.CardRank.Eight, Card.CardSuit.Clubs),new Card(Card.CardRank.Nine, Card.CardSuit.Clubs),new Card(Card.CardRank.Ten, Card.CardSuit.Clubs),new Card(Card.CardRank.Jack, Card.CardSuit.Clubs),new Card(Card.CardRank.Queen, Card.CardSuit.Clubs),new Card(Card.CardRank.King, Card.CardSuit.Clubs),
			new Card(Card.CardRank.Ace, Card.CardSuit.Diamonds),new Card(Card.CardRank.Two, Card.CardSuit.Diamonds), new Card(Card.CardRank.Three, Card.CardSuit.Diamonds),new Card(Card.CardRank.Four, Card.CardSuit.Diamonds),new Card(Card.CardRank.Five, Card.CardSuit.Diamonds),new Card(Card.CardRank.Six, Card.CardSuit.Diamonds),new Card(Card.CardRank.Seven, Card.CardSuit.Diamonds),new Card(Card.CardRank.Eight, Card.CardSuit.Diamonds),new Card(Card.CardRank.Nine, Card.CardSuit.Diamonds),new Card(Card.CardRank.Ten, Card.CardSuit.Diamonds),new Card(Card.CardRank.Jack, Card.CardSuit.Diamonds),new Card(Card.CardRank.Queen, Card.CardSuit.Diamonds),new Card(Card.CardRank.King, Card.CardSuit.Diamonds)
		};
		cardMats = new Dictionary<string, Material>();
		for (int i = 0; i < cardList.Count; i++)
		{
			cardMats.Add(cardList[i].toString(), cardMatList[i]);
		}
		bool[][] tableVacancy =
		{
			new bool[]{ true, true, true, false, false, true, true, true, true },
			new bool[]{ true, true, true, false, false, true, true, true, true },
			new bool[]{ true, true, true, false, false, true, true, true, true }
		};
		bool[][] playerVacancy =
		{
			new bool[]{ true, true },
			new bool[]{ true, true }
		};

		drawPile = new Deck(MAX_DECK_SIZE_FOR_DRAW_PILE, cardList, drawPileMesh, drawPileMesh.transform.localPosition.y, "Draw Pile");
		playPile = new Deck(0, cardList, playPileMesh, playPileMesh.transform.localPosition.y, "Play Pile");
		drawPile.gameObject[0].tablePlacement = new bool[][] { new bool[] { false, false, false, false, false, false, false, false, false }, new bool[] { false, false, false, true, false, false, false, false, false }, new bool[] { false, false, false, false, false, false, false, false, false }};
		playPile.gameObject[0].tablePlacement = new bool[][] { new bool[] { false, false, false, false, false, false, false, false, false }, new bool[] { false, false, false, false, true, false, false, false, false }, new bool[] { false, false, false, false, false, false, false, false, false }};
		gameObjs.Add(drawPile);
		gameObjs.Add(playPile);

		drawPile.gameObject[0].makeObjectVisible();
		drawPile.gameObject[0].changeObjectLocation(getNewObjectLocation(drawPile.gameObject[0].tablePlacement, tableGridVector3, drawPile.gameObject[0].standingY));
		playPile.gameObject[0].makeObjectVisible();
		playPile.gameObject[0].changeObjectLocation(getNewObjectLocation(playPile.gameObject[0].tablePlacement, tableGridVector3, 0f));

		//Generate random rules
		List<Trigger> triggerList = new List<Trigger>()
		{
			new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King),
			new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds),
			new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King),
			new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds),
			new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4),
			new sameSuit(), new differentSuit(),
			new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Spades), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Diamonds, Card.CardSuit.Diamonds),
			
		};
		List<GameEffect> gameEffectList = new List<GameEffect>()
		{
			new ReverseTurnOrder(),
			new SkipNextPlayerTurn(),
			new PlayAgain(),
			new SayPhrase(Phrase.HAVE_A_NICE_DAY, new List<XPlayerAction>() { XPlayerAction.CCW_OF_CURRENT,XPlayerAction.CURRENT_PLAYER,XPlayerAction.CW_OF_CURRENT,XPlayerAction.NEXT_PLAYER,XPlayerAction.OPPOSITE_PLAYER,XPlayerAction.PREVIOUS_PLAYER,XPlayerAction.PLAYER_WITH_WHITE_CIRCLE,XPlayerAction.PLAYER_WITH_BLUE_SQUARE}),
			new ForbiddenSuit(),
			new AddOffsetToRank(1),
			new TakeWhiteCircle(whiteCircle),
			//new TakeBlueSquare(blueSquare)

		};
		List<XPlayerAction> xPlayerActions = new List<XPlayerAction>()
		{
			XPlayerAction.CCW_OF_CURRENT,
			XPlayerAction.CURRENT_PLAYER,
			XPlayerAction.CW_OF_CURRENT,
			XPlayerAction.NEXT_PLAYER,
			XPlayerAction.OPPOSITE_PLAYER,
			XPlayerAction.PREVIOUS_PLAYER
		};
		gameEffectList.Shuffle();
		rules = new List<Rule>();
		List<Rule> objectMovingRules = new List<Rule>();
		for (int i = 0; rules.Count < MAX_NUM_RULES && i < gameEffectList.Count; i++)
		{
			int triggerIndex = gameEffectList[i].pickTrigger(triggerList);
			if (triggerIndex >= 0)
			{
				if(gameEffectList[i].isAction)
				{
					bool flag = ((ActionEffect)gameEffectList[i]).pickXPlayerAction(xPlayerActions);
					if(flag)
					{
						if(gameEffectList[i].spawnsObj || gameEffectList[i].needsObj)
						{
							ObjectEffect OE = (ObjectEffect)gameEffectList[i];
							if (OE.pickReceivingPlayer(xPlayerActions))
							{
								//Does this object already exist on the table
								if (gameObjs.Contains(OE.gameObject))
								{
									int index = gameObjs.IndexOf(OE.gameObject);
									//Check to see if there's room on the table
									if (!playsToTable[index] && OE.playToTable)
									{
										bool[][] tablePlacement = getObjectPlacement(tableVacancy, OE.gameObject.gameObject[0].shape);
										if (tablePlacement == null)
											flag = false;
										else
										{
											gameObjs[index].gameObject[0].tablePlacement = tablePlacement;
											playsToTable[index] = true;
										}
									}
									if (!playsToPlayer[index] && OE.playToPlayer)
									{
										bool[][] playerPlacement = getObjectPlacement(playerVacancy, OE.gameObject.gameObject[0].shape);
										if (playerPlacement == null)
											flag = false;
										else
										{
											gameObjs[index].gameObject[0].playerPlacement = playerPlacement;
											playsToPlayer[index] = true;
										}
									}
									if (flag)
									{
										string triggerStr = triggerList[triggerIndex].toString();
										triggerList.RemoveAt(triggerIndex);
										rules.Add(new Rule(gameEffectList[i].getTrigger(triggerStr), gameEffectList[i]));
										if (playsToPlayer[index] || playsToTable[index])
											objectMovingRules.Add(rules[rules.Count - 1]);
										XPlayerAction[] XPAS = OE.getNewXPlayerActions();
										if (XPAS != null)
										{
											foreach (XPlayerAction XPA in XPAS)
											{
												if (!xPlayerActions.Contains(XPA))
													xPlayerActions.Add(XPA);
											}
										}
										playerVacancy = getNewVacancy(playerVacancy, gameObjs[index].gameObject[0].playerPlacement);
										tableVacancy = getNewVacancy(tableVacancy, gameObjs[index].gameObject[0].tablePlacement);
										
									}
								}
								else if (!gameEffectList[i].needsObj)
								{
									//Check to see if there's room on the table
									if (OE.playToTable || OE.startsOnTable)
									{
										bool[][] tablePlacement = getObjectPlacement(tableVacancy, OE.gameObject.gameObject[0].shape);
										if (tablePlacement == null)
											flag = false;
										OE.gameObject.gameObject[0].tablePlacement = tablePlacement;
									}
									if (OE.playToPlayer || !OE.startsOnTable)
									{
										bool[][] playerPlacement = getObjectPlacement(playerVacancy, OE.gameObject.gameObject[0].shape);
										if (playerPlacement == null)
											flag = false;
										foreach (GameObjectObj gameObject in OE.gameObject.gameObject)
											gameObject.playerPlacement = playerPlacement;
									}
									//Object has passed all validations, time to spawn the object and add the rule
									if (flag)
									{
										playsToPlayer.Add(OE.playToPlayer);
										playsToTable.Add(OE.playToTable);
										if (OE.startsOnTable)
										{
											OE.gameObject.gameObject[0].changeObjectLocation(getNewObjectLocation(OE.gameObject.gameObject[0].tablePlacement, tableGridVector3, OE.gameObject.gameObject[0].standingY));
										}
										else if (OE.gameObject.gameObject.Length == 1)
										{
											int randomPlayer = Random.Range(0, players.Length);
											OE.gameObject.gameObject[0].changeObjectLocation(getNewObjectLocation(OE.gameObject.gameObject[0].playerPlacement, playerGridVector3[randomPlayer], OE.gameObject.gameObject[0].standingY));
											players[randomPlayer].addObject(OE.gameObject, 0);
										}
										else
										{
											for (int player = 0; player < players.Length; player++)
											{
												OE.gameObject.gameObject[player].changeObjectLocation(getNewObjectLocation(OE.gameObject.gameObject[player].playerPlacement, playerGridVector3[player], OE.gameObject.gameObject[player].standingY));
												players[player].addObject(OE.gameObject, player);
											}
										}
										foreach (GameObjectObj gameObj in OE.gameObject.gameObject)
											gameObj.makeObjectVisible();
										gameObjs.Add(OE.gameObject);
										string triggerStr = triggerList[triggerIndex].toString();
										triggerList.RemoveAt(triggerIndex);
										
										rules.Add(new Rule(gameEffectList[i].getTrigger(triggerStr), gameEffectList[i]));
										if (OE.playToTable || OE.playToPlayer)
											objectMovingRules.Add(rules[rules.Count - 1]);
										XPlayerAction[] XPAS = OE.getNewXPlayerActions();
										if (XPAS != null)
										{
											foreach (XPlayerAction XPA in XPAS)
											{
												if (!xPlayerActions.Contains(XPA))
													xPlayerActions.Add(XPA);
											}
										}
										playerVacancy = getNewVacancy(playerVacancy, OE.gameObject.gameObject[0].playerPlacement);
										tableVacancy = getNewVacancy(tableVacancy, OE.gameObject.gameObject[0].tablePlacement);
									}
								}
							}
						}
						else
						{
							string triggerStr = triggerList[triggerIndex].toString();
							triggerList.RemoveAt(triggerIndex);
							rules.Add(new Rule(gameEffectList[i].getTrigger(triggerStr), gameEffectList[i]));
						}
					}
				}
				else
				{
					string triggerStr = triggerList[triggerIndex].toString();
					triggerList.RemoveAt(triggerIndex);
					rules.Add(new Rule(gameEffectList[i].getTrigger(triggerStr), gameEffectList[i]));
				}
			}
		}

		foreach(Rule rule in objectMovingRules)
			rules.RemoveAt(rules.IndexOf(rule));
		objectMovingRules.Shuffle();
		foreach (Rule rule in objectMovingRules)
			rules.Add(rule);
		Debug.LogFormat("{0} ***RULES***", moduleNameLog);
		foreach (Rule rule in rules)
		{
			Debug.LogFormat("{0} {1}", moduleNameLog, rule.toString());
			foreach (Bot player in players)
				player.addRule(rule);
		}
		//Debug.LogFormat("RULE BLOCKER");
		HIGHEST_CARD_VALUE = int.MinValue;
		LOWEST_CARD_VALUE = int.MaxValue;
		foreach (Card card in cardList)
		{
			int value = (int)card.getRank();
			if (value > HIGHEST_CARD_VALUE)
				HIGHEST_CARD_VALUE = value;
			if (value < LOWEST_CARD_VALUE)
				LOWEST_CARD_VALUE = value;
		}
		
		turnCounter = Random.Range(0, players.Length);
		turnDirection = new int[] { -1, 1 }[Random.Range(0, 2)];
		thoughtTurnCounters = new List<int>();
		thoughtTurnCounters.Add(turnCounter);
		currPlayerPlayed = false;
		prevPlayerPlayed = false;
		playButton.OnInteract = delegate { StartCoroutine(StartGame()); return false; };
		prevPlayer = -1;
		handCountsAfterDrawPlay = new int[players.Length];
		previousTriggeredRules = new List<Rule>();
		gameRounds = new List<GameRound>();
	}
	private bool[][] getObjectPlacement(bool[][] vacancy, bool[][] objectShape)
	{
		if (objectShape.Length > vacancy.Length || objectShape.Length == 0)
			return null;
		int maxRow = objectShape.Length;
		int maxCol = objectShape[0].Length;
		foreach (bool[] row in objectShape)
			maxCol = Mathf.Max(maxCol, row.Length);
		if (maxCol == 0)
			return null;
		bool[][] placement = new bool[vacancy.Length][];
		for (int i = 0; i < placement.Length; i++)
		{
			placement[i] = new bool[vacancy[i].Length];
			for (int j = 0; j < placement[i].Length; j++)
				placement[i][j] = false;
		}
		for (int col = 0; col < vacancy[0].Length - (maxCol - 1); col++)
		{
			for(int row = 0; row < vacancy.Length - (maxRow - 1); row++)
			{
				bool flag = true;
				for(int r = 0; r < objectShape.Length; r++)
				{
					for(int c = 0; c < objectShape[r].Length; c++)
					{
						if (objectShape[r][c] && !vacancy[row + r][col + c])
						{
							flag = false;
							goto skipCheck;
						}
					}
				}
				skipCheck:
				if(flag)
				{
					for (int r = 0; r < objectShape.Length; r++)
					{
						for (int c = 0; c < objectShape[r].Length; c++)
							placement[row + r][col + c] = objectShape[r][c];
					}
					return placement;
				}
			}
		}
		return null;
	}
	private Vector3 getNewObjectLocation(bool[][] location, Vector3[][] locationXYZ, float y)
	{
		int lowestRow = int.MaxValue;
		int highestRow = int.MinValue;
		int lowestCol = int.MaxValue;
		int highestCol = int.MinValue;
		for(int i = 0; i < location.Length; i++)
		{
			for(int j = 0; j < location[i].Length; j++)
			{
				if(location[i][j])
				{
					lowestRow = Mathf.Min(lowestRow, i);
					highestRow = Mathf.Max(highestRow, i);
					lowestCol = Mathf.Min(lowestCol, j);
					highestCol = Mathf.Max(highestCol, j);
				}
			}
		}
		float lowestX, highestX, lowestZ, highestZ;
		if(locationXYZ[lowestRow][lowestCol].x > locationXYZ[highestRow][highestCol].x)
		{
			highestX = locationXYZ[lowestRow][lowestCol].x;
			lowestX = locationXYZ[highestRow][highestCol].x;
		}
		else
		{
			highestX = locationXYZ[highestRow][highestCol].x;
			lowestX = locationXYZ[lowestRow][lowestCol].x;
		}
		if(locationXYZ[lowestRow][lowestCol].z > locationXYZ[highestRow][highestCol].z)
		{
			highestZ = locationXYZ[lowestRow][lowestCol].z;
			lowestZ = locationXYZ[highestRow][highestCol].z;
		}
		else
		{
			highestZ = locationXYZ[highestRow][highestCol].z;
			lowestZ = locationXYZ[lowestRow][lowestCol].z;
		}
		float newX = lowestX + ((highestX - lowestX) / 2);
		float newZ = lowestZ + ((highestZ - lowestZ) / 2);

		return new Vector3(newX, y, newZ);
	}
	private bool[][] getNewVacancy(bool[][] vacancy, bool[][] objectPlacement)
	{
		if(objectPlacement != null)
		{
			for (int i = 0; i < objectPlacement.Length; i++)
			{
				for (int j = 0; j < objectPlacement[i].Length; j++)
				{
					if (objectPlacement[i][j])
						vacancy[i][j] = false;
				}
			}
		}
		return vacancy;
	}
	IEnumerator StartGame()
	{
		StartOfRoundInfo SORI = getStartOfRoundInfo();

		


		ActionInfo action = new ActionInfo();
		action.turnDirection = turnDirection;
		action.playerPerformingAction = turnCounter;
		playButton.OnInteract = null;
		yield return new WaitForSeconds(0f);
		for (int i = 0; i < STARTING_NUM_CARDS; i++)
		{
			for (int player = 0; player < players.Length; player++)
			{
				yield return StartCoroutine(DrawCardToStartGame(player, RATE_OF_DRAWING_TO_START_GAME));
			}
		}
		playPile.AddToDeck(drawPile.Draw());
		if (drawPile.getDeckSize() == 0)
			drawPile = new Deck(MAX_DECK_SIZE_FOR_DRAW_PILE, cardList, drawPileMesh, drawPileMesh.transform.localPosition.y, "Draw Pile");
		Card c = playPile.GetCard(0);
		playPile.gameObject[0].meshRenderer.material = cardMats[c.toString()];
		action.material = cardMats[c.toString()];
		for (int player = 0; player < players.Length; player++)
			players[player].addPlayPileCard(playPile.GetCard(0));
		float endX = playPileMesh.transform.localPosition.x;
		float endZ = playPileMesh.transform.localPosition.z;
		float startX = drawPileMesh.transform.localPosition.x;
		float startZ = drawPileMesh.transform.localPosition.z;
		float y = 0.02f;
		float absoluteX = Mathf.Abs(endX - startX);
		float absoluteZ = Mathf.Abs(endZ - startZ);
		int rate = 100;
		float movementX = absoluteX / rate;
		if (endX < startX)
			movementX *= (-1);
		float movementZ = absoluteZ / rate;
		if (endZ < startZ)
			movementZ *= (-1);
		animateCard.material = faceDownMat;
		animateCard.transform.localPosition = new Vector3(startX, y, startZ);
		yield return new WaitForSeconds(0.001f);
		for (int i = 0; i < rate; i++)
		{
			startX += movementX;
			startZ += movementZ;
			animateCard.transform.localPosition = new Vector3(startX, y, startZ);
			yield return new WaitForSeconds(0.001f);
		}
		playPile.gameObject[0].changeObjectLocation(getNewObjectLocation(playPile.gameObject[0].tablePlacement, tableGridVector3, playPile.gameObject[0].standingY));
		animateCard.transform.localPosition = new Vector3(0f, 0f, 0f);
		validCards = new List<Card>();
		audio.PlaySoundAtTransform(introduction.name, transform);
		Debug.LogFormat("{0} Chairman: 'You're playing standard 7-card Cambridge Mao.'", moduleNameLog);
		yield return new WaitForSeconds(introduction.length);
		if (turnDirection == 1)
		{
			audio.PlaySoundAtTransform(introCW.name, transform);
			Debug.LogFormat("{0} Chairman: 'Play will begin with this player ({1}) and will proceed in a clockwise direction.'", moduleNameLog, playerNames[turnCounter]);
		}	
		else
		{
			audio.PlaySoundAtTransform(introCCW.name, transform);
			Debug.LogFormat("{0} Chairman: 'Play will begin with this player ({1}) and will proceed in a counter-clockwise direction.'", moduleNameLog, playerNames[turnCounter]);
		}
		yield return new WaitForSeconds(1.8f);
		playerMats[turnCounter].material = activeMat;
		playerText[turnCounter].color = activeText;
		yield return new WaitForSeconds(3f);
		playButton.OnInteract = delegate { StartCoroutine(PlayRound(true)); return false; };		
		gameRounds.Add(new GameRound());
		gameRounds[gameRounds.Count - 1].startOfRoundInfo = SORI;
		gameRounds[gameRounds.Count - 1].actionsPerformed.Add(action);
		pfoButton.OnInteract = delegate { enablePFOMode(); return false; };

		HAVE_PLAY_ON_REPEAT = false;
		if (HAVE_PLAY_ON_REPEAT)
		{

			StartCoroutine(GameLoop());

		}
	}
	StartOfRoundInfo getStartOfRoundInfo()
	{
		StartOfRoundInfo SORI = new StartOfRoundInfo();
		int[] startingHandCounts = new int[players.Length];
		for (int i = 0; i < startingHandCounts.Length; i++)
			startingHandCounts[i] = players[i].getHandCount();
		SORI.startingHandCounts = startingHandCounts;
		SORI.playButtonColor = playButtonMesh.material;
		SORI.playButtonTextColor = playButtonText.color;
		List<GameMainObjInfo> GMOI = getStartOfRoundObjectInfo();
		SORI.gameObjInfo = GMOI;
		return SORI;
	}
	
	IEnumerator GameLoop()
	{
		yield return new WaitForSeconds(1f);
		repeatPlaying:
		playButton.OnInteract();
		bool flag = true;
		repeatLoop:
		if (flag)
		{
			yield return new WaitForSeconds(0.01f);
			if (playButton.OnInteract != null)
				flag = false;
			goto repeatLoop;
		}
		goto repeatPlaying;
	}
	IEnumerator PlayRound(bool isNewRound)
	{
		if (isNewRound)
		{
			gameRounds.Add(new GameRound());
			gameRounds[gameRounds.Count - 1].startOfRoundInfo = getStartOfRoundInfo();
			Debug.LogFormat("{0} ***START OF TURN***", moduleNameLog);
			for (int i = 0; i < players.Length; i++)
			{
				Debug.LogFormat("{0} {1} Player has {2} card(s)", moduleNameLog, playerNames[i], players[i].getHandCount());
			}
			Debug.LogFormat("{0} Play pile is showing a {1}", moduleNameLog, playPile.GetCard(0).toString());
			Debug.LogFormat("{0} Actual value of the card is {1}", moduleNameLog, playPile.GetCard(0).toActualString());
			foreach (GameObjectMain GOM in gameObjs)
			{
				bool objectNotOnPlayer = true;
				for (int i = 0; i < players.Length; i++)
				{
					if (players[i].hasObject(GOM))
					{
						if (GOM.gameObject.Length > 1)
						{
							Debug.LogFormat("{0} {1} Player has a {2}", moduleNameLog, playerNames[i], GOM.toString(i));
						}
						else
						{
							Debug.LogFormat("{0} {1} Player has a {2}", moduleNameLog, playerNames[i], GOM.toString(0));
						}

						objectNotOnPlayer = false;
					}
				}
				if (objectNotOnPlayer)
					Debug.LogFormat("{0} {1} is on the table", moduleNameLog, GOM.toString(0));
			}
			Debug.LogFormat("{0} ***TURN BEGINS***", moduleNameLog);
		}
		yield return new WaitForSeconds(0f);
		playButton.OnInteract = null;
		currPlayerPlayed = false;
		bool willTryAgain = true;
		StartOfRound:
		if (!hasInitPlayerDecisions)
		{
			foreach (Rule rule in rules)
			{
				for (int i = 0; i < players.Length; i++)
					players[i].rollThoughtResult(rule);
			}
			hasInitPlayerDecisions = true;
		}
		for (int i = 0; i < players.Length; i++)
		{
			playerMats[i].material = inactiveMat;
			playerText[i].color = inactiveText;
		}
		if (thoughtTurnCounters.Count == 0) //No one knows who turn is it
		{
			Debug.LogFormat("{0} The {1} player is not taking their turn.", moduleNameLog, playerNames[turnCounter]);
			playButtonMesh.material = activeMat;
			playButtonText.color = activeText;
			playButton.OnInteract = delegate { StartCoroutine(PlayMissedRound()); return false; };
		}
		else if (thoughtTurnCounters[0] != turnCounter) //A player is currently playing when it's not their turn
		{
			int playCardIndex = -1;
			List<Card> currHand = players[thoughtTurnCounters[0]].getHand();
			if (players[thoughtTurnCounters[0]].getHandCount() == 1)
				goto wrongSkipper;
			for (int i = 0; i < currHand.Count; i++)
			{
				Deck PPT = players[turnCounter].getPlayPileThought(cardList);
				//Go through each card to see which card the player thinks is valid
				Card card = currHand[i];
				currHand.RemoveAt(i);
				//Here change the card based on the current rules
				//Card newCard = new Card(card.rank, card.suit);
				PPT.AddToDeck(card);
				currPlayerPlayed = true;
				List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), PPT), PPT);
				HasTriggeredCardChangingRules(TR, PPT, false);
				TR = GetListOfTriggeredRules(getGameState(TR, PPT), PPT);
				card = PPT.RemoveCard(0);
				card.revertCardChanges();
				PPT.AddToDeck(card);
				List<Rule> PTR = GetListOfTriggeredRules(turnCounter, TR, PPT);
				HasTriggeredCardChangingRules(PTR, PPT, false);
				PTR = GetListOfTriggeredRules(turnCounter, TR, PPT);
				List<Card> playerValidCards = getValidCards(PTR, false, PPT);
				card = PPT.RemoveCard(0);
				currPlayerPlayed = false;
				bool isValid = isValidCard(playerValidCards, card);
				if (isValid)
				{
					playCardIndex = i;
					card.revertCardChanges();
					currHand.Insert(playCardIndex, card);
					goto wrongSkipper;
				}
				card.revertCardChanges();
				currHand.Insert(i, card);
			}
			wrongSkipper:
			//Player is attempting to play a card
			if (playCardIndex >= 0)
			{
				playerMats[thoughtTurnCounters[0]].material = activeMat;
				playerText[thoughtTurnCounters[0]].color = activeText;
				
				gameRounds[gameRounds.Count - 1].actionsPerformed.Add(new ActionInfo());
				gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].actionEffect = Effect.PLAYER_TURN_START;
				gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].playerPerformingAction = thoughtTurnCounters[0];
				Debug.LogFormat("{0} {1} player: *Plays the {2} to the play pile*", moduleNameLog, playerNames[thoughtTurnCounters[0]], currHand[playCardIndex].toString());
				yield return StartCoroutine(PlayCard(thoughtTurnCounters[0], RATE_OF_MOVING_OBJECT, playCardIndex));
				Debug.LogFormat("{0} Chairman: *Returns the {1} from the play pile to the {2} player*", moduleNameLog, playPile.GetCard(0).toString(), playerNames[thoughtTurnCounters[0]]);
				yield return StartCoroutine(ReturnCardToPlayer(thoughtTurnCounters[0], RATE_OF_MOVING_OBJECT));
				yield return StartCoroutine(ChairmanViolation(new ViolationInfo(ViolationPhrases.PLAYING_OUT_OF_TURN), thoughtTurnCounters[0]));
				thoughtTurnCounters.RemoveAt(0);
				goto StartOfRound;
			}
			else
			{
				thoughtTurnCounters.RemoveAt(0);
				goto StartOfRound;
			}
		}
		else //A player is currently playing their turn
		{
			playerMats[turnCounter].material = activeMat;
			playerText[turnCounter].color = activeText;

			gameRounds[gameRounds.Count - 1].actionsPerformed.Add(new ActionInfo());
			gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].actionEffect = Effect.PLAYER_TURN_START;
			gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].playerPerformingAction = turnCounter;

			if (willTryAgain) //Player is trying to play again after a failed attempt to play
			{
				bool hasDrawn = false;
				tryToPlayAgain:
				int playCardIndex = -1;
				List<Card> currHand = players[turnCounter].getHand();
				if (players[turnCounter].getHandCount() == 1)
					goto skipper;
				for (int i = 0; i < currHand.Count; i++)
				{
					Deck PPT = players[turnCounter].getPlayPileThought(cardList);
					//Go through each card to see which card the player thinks is valid
					Card card = currHand[i];
					currHand.RemoveAt(i);
					//Here change the card based on the current rules
					//Card newCard = new Card(card.rank, card.suit);
					PPT.AddToDeck(card);
					currPlayerPlayed = true;
					List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), PPT), PPT);
					HasTriggeredCardChangingRules(TR, PPT, false);
					TR = GetListOfTriggeredRules(getGameState(TR, PPT), PPT);
					card = PPT.RemoveCard(0);
					card.revertCardChanges();
					PPT.AddToDeck(card);
					List<Rule> PTR = GetListOfTriggeredRules(turnCounter, TR, PPT);
					HasTriggeredCardChangingRules(PTR, PPT, false);
					PTR = GetListOfTriggeredRules(turnCounter, TR, PPT);
					List<Card> playerValidCards = getValidCards(PTR, false, PPT);
					card = PPT.RemoveCard(0);
					currPlayerPlayed = false;
					bool isValid = isValidCard(playerValidCards, card);
					if (isValid)
					{
						playCardIndex = i;
						card.revertCardChanges();
						currHand.Insert(playCardIndex, card);
						goto skipper;
					}
					card.revertCardChanges();
					currHand.Insert(i, card);
				}
				skipper:
				if (playCardIndex == -1) 
				{
					if (!hasDrawn) //Player is drawing a card
					{
						//Debug.LogFormat("PLAYER IS DRAWING");
						Debug.LogFormat("{0} {1} player: *Draws a card from the draw pile*", moduleNameLog, playerNames[turnCounter]);
						yield return StartCoroutine(DrawCard(turnCounter, RATE_OF_MOVING_OBJECT, false));
						hasDrawn = true;
						goto tryToPlayAgain;
					}
					else //Player is passing
					{
						Debug.LogFormat("{0} {1} player is passing", moduleNameLog, playerNames[turnCounter]);
						//Checking if any rules have been triggered
						//Debug.LogFormat("CHECKING RULES");
						for (int i = 0; i < players.Length; i++)
							handCountsAfterDrawPlay[i] = players[i].getHandCount();
						List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), playPile), playPile);
						yield return StartCoroutine(HasTriggeredActionBasedRules(TR, new List<Rule>(), new List<Rule>(), playPile));
						HasTriggeredTurnBasedRules(TR, new List<Rule>(), new List<Rule>(), playPile);
						prevPlayer = turnCounter + 0;
						turnCounter = mod(nextTurnCounter + turnDirection, players.Length);
						playButton.OnInteract = delegate { StartCoroutine(PlayRound(true)); return false; };
						hasInitPlayerDecisions = false;
						prevPlayerPlayed = currPlayerPlayed;
						
						previousTriggeredRules = new List<Rule>();
						previousTriggeredRules.AddRange(TR);
					}
				}
				else //Player is playing a card
				{
					yield return StartCoroutine(PlayCard(turnCounter, RATE_OF_MOVING_OBJECT, playCardIndex));
					Debug.LogFormat("{0} {1} player: *Plays the {2} to the play pile*", moduleNameLog, playerNames[turnCounter], playPile.GetCard(0).toString());
					currPlayerPlayed = true;
					for (int i = 0; i < players.Length; i++)
						handCountsAfterDrawPlay[i] = players[i].getHandCount();
					List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), playPile), playPile);
					triggeredCardChangingRules = new List<Rule>();
					triggeredValidCardRules = new List<Rule>();
					HasTriggeredCardChangingRules(TR, playPile, true);
					Debug.LogFormat("{0} Actual value of the card is {1}", moduleNameLog, playPile.GetCard(0).toActualString());
					TR = GetListOfTriggeredRules(getGameState(TR, playPile), playPile);
					validCards = getValidCards(TR, true, playPile);
					hasDrawn = false;
					bool isValid = isValidCard(validCards, playPile.GetCard(0));
					if (!isValid)   //Player has played an invalid card
					{
						for (int i = 0; i < players.Length; i++)
						{
							foreach (Rule TVCR in triggeredValidCardRules)
							{
								int score = 5;
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TVCR, getGameState(TR, playPile), nextTurnCounter, prevPlayer))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToInterRuleScore(TVCR, score);
							}
							foreach (Rule TCCR in triggeredCardChangingRules)
							{
								int score = 5;
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TCCR, getGameState(TR, playPile), nextTurnCounter, prevPlayer))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToInterRuleScore(TCCR, score);
							}
						}
						currPlayerPlayed = false;
						Debug.LogFormat("{0} Chairman: *Returns the {1} ({2}) from the play pile to the {3} player*", moduleNameLog, playPile.GetCard(0).toString(), playPile.GetCard(0).toActualString(), playerNames[turnCounter]);
						yield return StartCoroutine(ReturnCardToPlayer(turnCounter, RATE_OF_MOVING_OBJECT));
						Debug.LogFormat("{0} Chairman: *Gives the top card from the draw pile to {1} player*", moduleNameLog, playerNames[turnCounter]);
						yield return StartCoroutine(ChairmanViolation(new ViolationInfo(ViolationPhrases.BAD_CARD), turnCounter));
						willTryAgain = Random.Range(0, 2) == 0;
						goto StartOfRound;
					}
					else //Player has played a valid card
					{
						for (int i = 0; i < players.Length; i++)
						{
							foreach (Rule TVCR in triggeredValidCardRules)
							{
								int score = 0;
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TVCR, getGameState(TR, playPile), nextTurnCounter, prevPlayer))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToInterRuleScore(TVCR, score);
							}
							foreach (Rule TCCR in triggeredCardChangingRules)
							{
								int score = 0;
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TCCR, getGameState(TR, playPile), nextTurnCounter, prevPlayer))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToInterRuleScore(TCCR, score);
							}
						}
						//Add Card to player thoughts
						Card cardToChange = playPile.GetCard(0);
						Card cardToKeepChanges = new Card(cardToChange.getRank(), cardToChange.getSuit());
						for (int i = 0; i < players.Length; i++)
						{
							cardToChange.revertCardChanges();
							players[i].addPlayPileCard(new Card(cardToChange.getRank(), cardToChange.getSuit()));
							Deck tempPPT = players[i].getPlayPileThought(cardList);
							List<Rule> PTR = GetListOfTriggeredRules(turnCounter, TR, tempPPT);
							HasTriggeredCardChangingRules(PTR, tempPPT, false);
						}
						playPile.GetCard(0).changeCardValues(cardToKeepChanges.getRank(), cardToKeepChanges.getSuit());
						//Start triggering rules
						TR = GetListOfTriggeredRules(getGameState(TR, playPile), playPile);
						TR.AddRange(triggeredCardChangingRules);
						TR.AddRange(triggeredValidCardRules);
						yield return StartCoroutine(HasTriggeredActionBasedRules(TR, triggeredCardChangingRules, triggeredValidCardRules, playPile));
						HasTriggeredTurnBasedRules(TR, triggeredCardChangingRules, triggeredValidCardRules, playPile);
						prevPlayer = turnCounter + 0;
						turnCounter = mod(nextTurnCounter + turnDirection, players.Length);
						playButton.OnInteract = delegate { StartCoroutine(PlayRound(true)); return false; };
						hasInitPlayerDecisions = false;
						prevPlayerPlayed = currPlayerPlayed;
						previousTriggeredRules = new List<Rule>();
						previousTriggeredRules.AddRange(triggeredCardChangingRules);
						previousTriggeredRules.AddRange(triggeredValidCardRules);
						previousTriggeredRules.AddRange(TR);
					}
				}
			}
			else //Player is drawing and passing
			{
				yield return StartCoroutine(DrawCard(turnCounter, RATE_OF_MOVING_OBJECT, false));
				Debug.LogFormat("{0} {1} player is passing", moduleNameLog, playerNames[turnCounter]);
				for (int i = 0; i < players.Length; i++)
					handCountsAfterDrawPlay[i] = players[i].getHandCount();
				List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), playPile), playPile);
				yield return StartCoroutine(HasTriggeredActionBasedRules(TR, new List<Rule>(), new List<Rule>(), playPile));
				HasTriggeredTurnBasedRules(TR, new List<Rule>(), new List<Rule>(), playPile);
				prevPlayer = turnCounter + 0;
				turnCounter = mod(nextTurnCounter + turnDirection, players.Length);
				playButton.OnInteract = delegate { StartCoroutine(PlayRound(true)); return false; };
				hasInitPlayerDecisions = false;
				prevPlayerPlayed = currPlayerPlayed;
				previousTriggeredRules = new List<Rule>();
				previousTriggeredRules.AddRange(TR);
			}
		}
		if(gameRounds[gameRounds.Count - 1].actionsPerformed.Count == 0)
		{
			gameRounds.RemoveAt(gameRounds.Count - 1);
		}

		//Debug.LogFormat("PLAYER BRAINS");
		for(int i = 0; i < players.Length; i++)
		{
			players[i].addScoresToTheEndOfRound();
			//Debug.LogFormat("PLAYER {0} BRAIN", (i + 1));
			//players[i].printScore();
		}
	}
	void enablePFOMode()
	{

		pfoButtonMesh.material = activeMat;
		pfoButtonText.color = activeText;
		pfoButton.OnInteract = delegate { disablePFOMode(); return false; };

		lastRoundBeforePFO = new GameRound();
		lastRoundBeforePFO.startOfRoundInfo = getStartOfRoundInfo();
		playButton.OnInteract = null;
		//Disable take over mode or simple override the pfobutton

		//Get the buttons for going through the rounds
		prevPFOButtonMesh.transform.localPosition = new Vector3(PFOButtonPosDict[PFOButtonName.PREV].x, PFOButtonPosDict[PFOButtonName.PREV].y, PFOButtonPosDict[PFOButtonName.PREV].z);
		playPFOButtonMesh.transform.localPosition = new Vector3(PFOButtonPosDict[PFOButtonName.PLAY].x, PFOButtonPosDict[PFOButtonName.PLAY].y, PFOButtonPosDict[PFOButtonName.PLAY].z);
		nextPFOButtonMesh.transform.localPosition = new Vector3(PFOButtonPosDict[PFOButtonName.NEXT].x, PFOButtonPosDict[PFOButtonName.NEXT].y, PFOButtonPosDict[PFOButtonName.NEXT].z);


		PFOIndex = gameRounds.Count - 1;
		displayStartOfRound(gameRounds[PFOIndex]);
	}
	void disablePFOMode()
	{
		pfoButtonMesh.material = inactiveMat;
		pfoButtonText.color = inactiveText;
		playButtonMesh.material = lastRoundBeforePFO.startOfRoundInfo.playButtonColor;
		playButtonText.color = lastRoundBeforePFO.startOfRoundInfo.playButtonTextColor;
		prevPFOButton.OnInteract = null;
		prevPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		playPFOButton.OnInteract = null;
		playPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		nextPFOButton.OnInteract = null;
		nextPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		pfoButton.OnInteract = delegate { enablePFOMode(); return false; };
		for (int i = 0; i < players.Length; i++)
		{
			playerMats[i].material = inactiveMat;
			playerText[i].color = inactiveText;
			playerText[i].text = players[i].getHandCount() + "";
		}
		if(playButtonText.color != activeText)
		{
			if(prevPlayer == -1)
			{
				playerMats[turnCounter].material = activeMat;
				playerText[turnCounter].color = activeText;
			}
			else
			{
				playerMats[prevPlayer].material = activeMat;
				playerText[prevPlayer].color = activeText;
				
			}
			playButton.OnInteract = delegate { StartCoroutine(PlayRound(true)); return false; };
		}
		else
			playButton.OnInteract = delegate { StartCoroutine(PlayMissedRound()); return false; };
		displayStartOfRoundObjects(lastRoundBeforePFO);
	}
	IEnumerator DrawCardToStartGame(int player, int rate)
	{
		Card c = drawPile.Draw();
		if (drawPile.getDeckSize() == 0)
			drawPile = new Deck(MAX_DECK_SIZE_FOR_DRAW_PILE, cardList, drawPileMesh, drawPileMesh.transform.localPosition.y, "Draw Pile");
		yield return StartCoroutine(animateDrawCard(player, rate));
		players[player].Draw(c);
		playerText[player].text = players[player].getHandCount() + "";
	}
	IEnumerator DrawCard(int player, int rate, bool isChairman)
	{
		Card c = drawPile.Draw();
		if (drawPile.getDeckSize() == 0)
			drawPile = new Deck(MAX_DECK_SIZE_FOR_DRAW_PILE, cardList, drawPileMesh, drawPileMesh.transform.localPosition.y, "Draw Pile");
		if(isChairman)
			Debug.LogFormat("{0} Chairman: *Gives the top card from the draw pile to {1} player*", moduleNameLog, playerNames[player]);
		gameRounds[gameRounds.Count - 1].actionsPerformed.Add(new ActionInfo());
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].actionEffect = Effect.DRAW_CARD;
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].playerPerformingAction = player;
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].material = faceDownMat;
		yield return StartCoroutine(animateDrawCard(player, rate));
		players[player].Draw(c);
		playerText[player].text = players[player].getHandCount() + "";
		yield return new WaitForSeconds(BUFFER_BETWEEN_ACTIONS);
	}
	IEnumerator animateDrawCard(int player, int rate)
	{
		float endX = playerMats[player].transform.localPosition.x;
		float endZ = playerMats[player].transform.localPosition.z;
		float startX = drawPileMesh.transform.localPosition.x;
		float startZ = drawPileMesh.transform.localPosition.z;
		float y = 0.02f;
		float absoluteX = Mathf.Abs(endX - startX);
		float absoluteZ = Mathf.Abs(endZ - startZ);
		float movementX = absoluteX / rate;
		if (endX < startX)
			movementX *= (-1);
		float movementZ = absoluteZ / rate;
		if (endZ < startZ)
			movementZ *= (-1);
		animateCard.material = faceDownMat;
		animateCard.transform.localPosition = new Vector3(startX, y, startZ);
		yield return new WaitForSeconds(0.001f);
		for (int i = 0; i < rate; i++)
		{
			startX += movementX;
			startZ += movementZ;
			animateCard.transform.localPosition = new Vector3(startX, y, startZ);
			yield return new WaitForSeconds(0.001f);
		}
		animateCard.transform.localPosition = new Vector3(0f, 0f, 0f);
	}

	IEnumerator PlayCard(int player, int rate, int cardIndex)
	{
		Card card = players[player].Play(cardIndex);
		playerText[player].text = players[player].getHandCount() + "";
		gameRounds[gameRounds.Count - 1].actionsPerformed.Add(new ActionInfo());
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].actionEffect = Effect.PLAY_CARD;
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].playerPerformingAction = player;
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].material = cardMats[card.toString()];
		yield return StartCoroutine(animatePlayCard(player, rate, cardMats[card.toString()]));
		playPile.AddToDeck(card);
	}
	IEnumerator animatePlayCard(int player, int rate, Material cardMat)
	{
		float endX = playPileMesh.transform.localPosition.x;
		float endZ = playPileMesh.transform.localPosition.z;
		float startX = playerMats[player].transform.localPosition.x;
		float startZ = playerMats[player].transform.localPosition.z;

		float y = 0.02f;
		float absoluteX = Mathf.Abs(endX - startX);
		float absoluteZ = Mathf.Abs(endZ - startZ);
		float movementX = absoluteX / rate;
		if (endX < startX)
			movementX *= (-1);
		float movementZ = absoluteZ / rate;
		if (endZ < startZ)
			movementZ *= (-1);
		animateCard.material = cardMat;
		animateCard.transform.localPosition = new Vector3(startX, y, startZ);
		yield return new WaitForSeconds(0.001f);
		for (int i = 0; i < rate; i++)
		{
			startX += movementX;
			startZ += movementZ;
			animateCard.transform.localPosition = new Vector3(startX, y, startZ);
			yield return new WaitForSeconds(0.001f);
		}
		animateCard.transform.localPosition = new Vector3(0f, 0f, 0f);
		playPileMesh.material = cardMat;
		yield return new WaitForSeconds(BUFFER_BETWEEN_ACTIONS);
	}
	IEnumerator ReturnCardToPlayer(int player, int rate)
	{
		Card card = playPile.RemoveCard(0);
		card.revertCardChanges();
		gameRounds[gameRounds.Count - 1].actionsPerformed.Add(new ActionInfo());
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].actionEffect = Effect.RETURN_CARD;
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].playerPerformingAction = player;
		gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].material = cardMats[card.toString()];
		yield return animateReturnCardToPlayer(player, rate, cardMats[playPile.GetCard(0).toString()], cardMats[card.toString()]);
		players[player].Draw(card);
		playerText[player].text = players[player].getHandCount() + "";
		yield return new WaitForSeconds(BUFFER_BETWEEN_ACTIONS);
	}
	IEnumerator animateReturnCardToPlayer(int player, int rate, Material playPileMat, Material playedCardMat)
	{
		playPileMesh.material = playPileMat;
		float startX = playPileMesh.transform.localPosition.x;
		float startZ = playPileMesh.transform.localPosition.z;
		float endX = playerMats[player].transform.localPosition.x;
		float endZ = playerMats[player].transform.localPosition.z;
		float y = 0.02f;
		float absoluteX = Mathf.Abs(endX - startX);
		float absoluteZ = Mathf.Abs(endZ - startZ);
		float movementX = absoluteX / rate;
		if (endX < startX)
			movementX *= (-1);
		float movementZ = absoluteZ / rate;
		if (endZ < startZ)
			movementZ *= (-1);
		animateCard.material = playedCardMat;
		animateCard.transform.localPosition = new Vector3(startX, y, startZ);
		yield return new WaitForSeconds(0.001f);
		for (int i = 0; i < rate; i++)
		{
			startX += movementX;
			startZ += movementZ;
			animateCard.transform.localPosition = new Vector3(startX, y, startZ);
			yield return new WaitForSeconds(0.001f);
		}
		animateCard.transform.localPosition = new Vector3(0f, 0f, 0f);
	}
	IEnumerator ChairmanViolation(ViolationInfo violationInfo, int player)
	{
		Debug.LogFormat("{0} Chairman: *Gives the top card from the draw pile to {1} player*", moduleNameLog, playerNames[player]);
		yield return StartCoroutine(DrawCard(thoughtTurnCounters[0], RATE_OF_MOVING_OBJECT, true));
		Debug.LogFormat("{0} Chairman: {1}", moduleNameLog, violationInfo.toString());
		if (violationInfo.phrase == Phrase.NULL)
		{
			audio.PlaySoundAtTransform(violationAudioClips[(int)violationInfo.violation].name, transform);
			yield return new WaitForSeconds(violationAudioClips[(int)violationInfo.violation].length);
			gameRounds[gameRounds.Count - 1].actionsPerformed.Add(new ActionInfo());
			gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].actionEffect = Effect.CHAIRMAN_VIOLATION;
			gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].violationInfo = violationInfo;
		}
		else
		{
			audio.PlaySoundAtTransform(chairmanViolationToSayPhrases[(int)violationInfo.phrase].name, transform);
			yield return new WaitForSeconds(chairmanViolationToSayPhrases[(int)violationInfo.phrase].length);
			gameRounds[gameRounds.Count - 1].actionsPerformed.Add(new ActionInfo());
			gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].actionEffect = Effect.CHAIRMAN_VIOLATION;
			gameRounds[gameRounds.Count - 1].actionsPerformed[gameRounds[gameRounds.Count - 1].actionsPerformed.Count - 1].violationInfo = violationInfo;
		}
	}

	IEnumerator PlayMissedRound()
	{
		gameRounds.Add(new GameRound());
		gameRounds[gameRounds.Count - 1].startOfRoundInfo = getStartOfRoundInfo();
		playButtonMesh.material = inactiveMat;
		playButtonText.color = inactiveText;
		playButton.OnInteract = null;
		Debug.LogFormat("{0} ***START OF TURN***", moduleNameLog);
		for (int i = 0; i < players.Length; i++)
		{
			Debug.LogFormat("{0} {1} Player has {2} card(s)", moduleNameLog, playerNames[i], players[i].getHandCount());
		}
		Debug.LogFormat("{0} Play pile is showing a {1}", moduleNameLog, playPile.GetCard(0).toString());
		Debug.LogFormat("{0} Actual value of the card is {1}", moduleNameLog, playPile.GetCard(0).toActualString());
		foreach (GameObjectMain GOM in gameObjs)
		{
			bool objectNotOnPlayer = true;
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].hasObject(GOM))
				{
					if (GOM.gameObject.Length > 1)
					{
						Debug.LogFormat("{0} {1} Player has a {2}", moduleNameLog, playerNames[i], GOM.toString(i));
					}
					else
					{
						Debug.LogFormat("{0} {1} Player has a {2}", moduleNameLog, playerNames[i], GOM.toString(0));
					}

					objectNotOnPlayer = false;
				}
			}
			if (objectNotOnPlayer)
				Debug.LogFormat("{0} {1} is on the table", moduleNameLog, GOM.toString(0));
		}
		Debug.LogFormat("{0} ***TURN BEGINS***", moduleNameLog);
		yield return StartCoroutine(ChairmanViolation(new ViolationInfo(ViolationPhrases.FAILURE_TO_PLAY), turnCounter));
		thoughtTurnCounters.Add(turnCounter);
		StartCoroutine(PlayRound(false));
	}
	List<Rule> GetListOfTriggeredRules(GameState gameState, Deck PP)
	{
		//Debug.LogFormat("SIZE OF RULES: {0}", rules.Count);
		List<Rule> tempTrigRules = new List<Rule>();
		foreach (Rule rule in rules)
		{
			if (rule.HasTriggered(gameState, nextTurnCounter, prevPlayer))
				tempTrigRules.Add(rule);
		}
		List<Rule> TR;
		do
		{
			TR = new List<Rule>();
			TR.AddRange(tempTrigRules);
			gameState = getGameState(TR, PP);
			tempTrigRules = new List<Rule>();
			foreach (Rule rule in rules)
			{
				if (rule.HasTriggered(gameState, nextTurnCounter, prevPlayer))
					tempTrigRules.Add(rule);
			}
		} while (TR.Count != tempTrigRules.Count);
		//foreach (Rule r in TR)
		//	Debug.LogFormat("RULE TRIGGERED: {0}", r.toString());
		return TR;
	}
	List<Rule> GetListOfTriggeredRules(int n, List<Rule> TR, Deck PP)
	{
		List<Rule> tempTrigRules = new List<Rule>();
		foreach (Rule rule in rules)
		{
			if (players[n].willTriggerRule(rule, getGameState(TR, PP), nextTurnCounter, prevPlayer))
				tempTrigRules.Add(rule);
		}
		return tempTrigRules;
	}
	private bool isValidCard(List<Card> VC, Card card)
	{
		foreach (Card c in VC)
		{
			if (card.getRank() == c.getRank() && card.getSuit() == c.getSuit())
				return true;
		}
		return false;
	}
	IEnumerator moveGameObject(GameObjectMain gameObject, Vector3 newLocation, int rate)
	{
		float endX = newLocation.x;
		float endZ = newLocation.z;
		float startX = gameObject.gameObject[0].meshRenderer.transform.localPosition.x;
		float startZ = gameObject.gameObject[0].meshRenderer.transform.localPosition.z;
		float y = gameObject.gameObject[0].movingY;
		float absoluteX = Mathf.Abs(endX - startX);
		float absoluteZ = Mathf.Abs(endZ - startZ);
		float movementX = absoluteX / rate;
		if (endX < startX)
			movementX *= (-1);
		float movementZ = absoluteZ / rate;
		if (endZ < startZ)
			movementZ *= (-1);
		yield return new WaitForSeconds(0.001f);
		for (int i = 0; i < rate; i++)
		{
			startX += movementX;
			startZ += movementZ;
			gameObject.gameObject[0].meshRenderer.transform.localPosition = new Vector3(startX, y, startZ);
			yield return new WaitForSeconds(0.001f);
		}
		gameObject.gameObject[0].meshRenderer.transform.localPosition = new Vector3(endX, gameObject.gameObject[0].standingY, endZ);
		yield return new WaitForSeconds(BUFFER_BETWEEN_ACTIONS);
	}
	int mod(int n, int m)
	{
		return (n % m + m) % m;
	}
	private void HasTriggeredCardChangingRules(List<Rule> TR, Deck PP, bool notPlayer)
	{
		List<Rule> TCCR = new List<Rule>();
		foreach (Rule rule in TR)
		{
			switch (rule.getGameEffect().getEffectID())
			{
				case Effect.ADD_OFFSET_TO_RANK:
					TCCR.Add(rule);
					int offset = ((AddOffsetToRank)rule.getGameEffect()).offset;
					Card card = PP.RemoveCard(0);
					int rankVal = mod(((int)card.getRank()) - LOWEST_CARD_VALUE + offset, HIGHEST_CARD_VALUE) + LOWEST_CARD_VALUE;
					card.changeCardValues((Card.CardRank)rankVal, card.getSuit());
					PP.AddToDeck(card);
					break;
			}
		}
		if (notPlayer)
		{
			triggeredCardChangingRules = new List<Rule>();
			triggeredCardChangingRules.AddRange(TCCR);
			foreach(Rule rule in TCCR)
				Debug.LogFormat("{0} Rule Triggered: {1}", moduleNameLog, rule.toString());
			//Debug.LogFormat("ALL VALID CARDS: {0}", string.Join(",", temp));
		}
	}
	List<Card> getValidCards(List<Rule> TR, bool notPlayer, Deck PP)
	{
		List<Card> VC = new List<Card>();
		Card currCard = PP.GetCard(1);
		foreach (Card card in cardList)
		{
			if (currCard.getRank() == card.getRank() || currCard.getSuit() == card.getSuit())
				VC.Add(card);
		}
		List<Rule> TVCR = new List<Rule>();
		foreach (Rule rule in TR)
		{
			switch(rule.getGameEffect().getEffectID())
			{
				case Effect.FORBIDDEN_SUIT:
					TVCR.Add(rule);
					VC = new List<Card>();
					foreach (Card card in cardList)
					{
						if (currCard.getSuit() != card.getSuit())
							VC.Add(card);
					}
					break;
			}
		}
		if (notPlayer)
		{
			triggeredValidCardRules = new List<Rule>();
			triggeredValidCardRules.AddRange(TVCR);
			foreach (Rule rule in TVCR)
				Debug.LogFormat("{0} Rule Triggered: {1}", moduleNameLog, rule.toString());
			/*
			string[] temp = new string[VC.Count];
			for (int i = 0; i < VC.Count; i++)
				temp[i] = VC[i].toString();
			Debug.LogFormat("ALL VALID CARDS: {0}", string.Join(",", temp));
			*/
		}
		return VC;
	}
	void HasTriggeredTurnBasedRules(List<Rule> TR, List<Rule> TCCR, List<Rule> TVCR, Deck PP)
	{
		nextTurnCounter = turnCounter;
		GameState gameState = getGameState(TR, PP);
		int[] playerTurnDirectionBrain = new int[players.Length];
		int[] playerTurnCounterBrain = new int[players.Length];
		List<Effect> triggeredEffectEnums = new List<Effect>();
		List<Effect> ruleEffectEnums = new List<Effect>();
		bool flag = false;
		thoughtTurnCounters = new List<int>();
		foreach (Rule rule in rules)
			ruleEffectEnums.Add(rule.getGameEffect().getEffectID());
		foreach (Rule rule in TR)
			triggeredEffectEnums.Add(rule.getGameEffect().getEffectID());
		for (int i = 0; i < players.Length; i++)
		{
			playerTurnCounterBrain[i] = turnCounter + 0;
			playerTurnDirectionBrain[i] = turnDirection + 0;
		}
		if (ruleEffectEnums.Contains(Effect.REVERSE_TURN_ORDER))
		{
			if (triggeredEffectEnums.Contains(Effect.REVERSE_TURN_ORDER))
			{
				Debug.LogFormat("{0} Rule Triggered: {1}", moduleNameLog, TR[triggeredEffectEnums.IndexOf(Effect.REVERSE_TURN_ORDER)].toString());
				turnDirection *= (-1);
				flag = true;
			}
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].willTriggerRule(rules[ruleEffectEnums.IndexOf(Effect.REVERSE_TURN_ORDER)], gameState, TCCR, TVCR, nextTurnCounter, prevPlayer))
				{
					playerTurnDirectionBrain[i] *= (-1);
					flag = true;
				}
			}
		}
		if (ruleEffectEnums.Contains(Effect.PLAY_AGAIN))
		{
			if (triggeredEffectEnums.Contains(Effect.PLAY_AGAIN))
			{
				Debug.LogFormat("{0} Rule Triggered: {1}", moduleNameLog, TR[triggeredEffectEnums.IndexOf(Effect.PLAY_AGAIN)].toString());
				nextTurnCounter = mod(nextTurnCounter - turnDirection, players.Length);
				flag = true;
			}
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].willTriggerRule(rules[ruleEffectEnums.IndexOf(Effect.PLAY_AGAIN)], gameState, TCCR, TVCR, nextTurnCounter, prevPlayer))
				{
					playerTurnCounterBrain[i] = mod(playerTurnCounterBrain[i] - playerTurnDirectionBrain[i], players.Length);
					flag = true;
				}
			}
		}
		if (ruleEffectEnums.Contains(Effect.SKIP_NEXT_PLAYER_TURN))
		{
			if (triggeredEffectEnums.Contains(Effect.SKIP_NEXT_PLAYER_TURN))
			{
				Debug.LogFormat("{0} Rule Triggered: {1}", moduleNameLog, TR[triggeredEffectEnums.IndexOf(Effect.SKIP_NEXT_PLAYER_TURN)].toString());
				nextTurnCounter = mod(nextTurnCounter + turnDirection, players.Length);
				flag = true;
			}
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].willTriggerRule(rules[ruleEffectEnums.IndexOf(Effect.SKIP_NEXT_PLAYER_TURN)], gameState, TCCR, TVCR, nextTurnCounter, prevPlayer))
				{
					playerTurnCounterBrain[i] = mod(playerTurnCounterBrain[i] + playerTurnDirectionBrain[i], players.Length);
					flag = true;
				}
			}
		}
		if(flag)
		{
			for (int i = 0; i < players.Length; i++)
			{
				int nextTurn = mod(playerTurnCounterBrain[i] + playerTurnDirectionBrain[i], players.Length);
				//Debug.LogFormat("PLAYER #" + (i) + " TURN THOUGHT: " + nextTurn);
				if (i == nextTurn)
				{
					//Debug.LogFormat("PLAYER #" + (i) + " ADDED TO TURN COUNTERS");
					thoughtTurnCounters.Add(i);
				}
			}
			thoughtTurnCounters.Shuffle();
		}
		else
		{
			thoughtTurnCounters.Add(mod(nextTurnCounter + turnDirection, players.Length));
		}
	}
	IEnumerator HasTriggeredActionBasedRules(List<Rule> TR, List<Rule> TCCR, List<Rule> TVCR, Deck PP)
	{
		yield return new WaitForSeconds(0f);
		List<Rule> actionsNotTaken = new List<Rule>();
		List<int> playersToPerformAction = new List<int>();
		List<List<int>> playersToReceiveObject = new List<List<int>>();
		List<int> playerIndexes;
		ActionEffect actionEffect;
		ObjectEffect objectEffect;
		ActionInfo actionInfo;
		foreach (Rule rule in rules)
		{
			if (rule.getGameEffect().isAction)
			{
				bool hasTriggered = rule.HasTriggered(getGameState(TR, PP), nextTurnCounter, prevPlayer);
				//bool hasTriggered = TR.Contains(rule);
				if (hasTriggered)
					Debug.LogFormat("{0} Rule Triggered: {1}", moduleNameLog, rule.toString());
				actionEffect = (ActionEffect)rule.getGameEffect();
				playerIndexes = new XPlayerIndex().getPlayerIndexes(actionEffect.xPlayerAction, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer);
				if(playerIndexes.Count > 0)
				{
					//Non-object based rule
					if (!(rule.getGameEffect().spawnsObj || rule.getGameEffect().needsObj))
					{
						foreach(int playerIndex in playerIndexes)
						{
							if (players[playerIndex].willTriggerRule(rule, getGameState(TR, PP), TCCR, TVCR, nextTurnCounter, prevPlayer))
							{
								for (int i = 1; i < players.Length; i++)
									players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, TVCR, nextTurnCounter, prevPlayer);
								actionInfo = new ActionInfo();
								actionInfo.gameEffect = rule.getGameEffect();
								actionInfo.actionEffect = rule.getGameEffect().getEffectID();
								actionInfo.playerPerformingAction = playerIndex;
								Debug.LogFormat("{0} {1} player: {2}", moduleNameLog, playerNames[playerIndex], actionEffect.actionString(XPlayerAction.NULL));
								yield return StartCoroutine(performAction(actionInfo, true, false, false));
								if (!hasTriggered)
								{
									yield return StartCoroutine(ChairmanViolation(actionEffect.illegalPerformAction, playerIndex));
								}
							}
							else if (hasTriggered)
							{
								for (int i = 1; i < players.Length; i++)
									players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, TVCR, nextTurnCounter, prevPlayer);
								actionsNotTaken.Add(rule);
								playersToPerformAction.Add(playerIndex);
								playersToReceiveObject.Add(new List<int>());
							}
						}
					}
					else
					{
						//Object based rule
						objectEffect = (ObjectEffect)actionEffect;
						foreach (int playerIndex in playerIndexes)
						{
							if (players[playerIndex].willTriggerRule(rule, getGameState(TR, PP), TCCR, TVCR, nextTurnCounter, prevPlayer))
							{
								for (int i = 1; i < players.Length; i++)
									players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, TVCR, nextTurnCounter, prevPlayer);
								actionInfo = new ActionInfo();
								actionInfo.gameEffect = rule.getGameEffect();
								actionInfo.actionEffect = rule.getGameEffect().getEffectID();
								actionInfo.playerPerformingAction = playerIndex;
								actionInfo.playerRecievingObj = new XPlayerIndex().getPlayerIndexes(objectEffect.receivingPlayer, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer);
								Debug.LogFormat("{0} {1} player: {2}", moduleNameLog, playerNames[playerIndex], objectEffect.actionString(objectEffect.receivingPlayer));
								yield return StartCoroutine(performAction(actionInfo, true, hasTriggered, false));
								if (!hasTriggered)
								{
									actionInfo = new ActionInfo();
									actionInfo.gameEffect = rule.getGameEffect();
									actionInfo.actionEffect = objectEffect.undoEffect;
									actionInfo.playerRecievingObj = new XPlayerIndex().getPlayerIndexes(objectEffect.playerWithObject, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer);
									yield return StartCoroutine(ChairmanViolation(objectEffect.illegalPerformAction, playerIndex));
									Debug.LogFormat("{0} Chairman: {1}", moduleNameLog, objectEffect.undoActionString(actionInfo.playerRecievingObj, playerNames));
									yield return StartCoroutine(performAction(actionInfo, true, false, false));
								}
							}
							else if (hasTriggered)
							{
								for (int i = 1; i < players.Length; i++)
									players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, TVCR, nextTurnCounter, prevPlayer);
								actionInfo = new ActionInfo();
								actionInfo.gameEffect = rule.getGameEffect();
								actionInfo.actionEffect = rule.getGameEffect().getEffectID();
								actionInfo.playerPerformingAction = playerIndex;
								actionInfo.playerRecievingObj = new XPlayerIndex().getPlayerIndexes(objectEffect.receivingPlayer, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer);
								playersToReceiveObject.Add(new XPlayerIndex().getPlayerIndexes(objectEffect.receivingPlayer, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer));
								yield return StartCoroutine(performAction(actionInfo, false, true, false));
								actionsNotTaken.Add(rule);
								playersToPerformAction.Add(playerIndex);
							}
						}
					}
				}
			}
		}
		for(int i = 0; i < actionsNotTaken.Count; i++)
		{
			actionEffect = (ActionEffect)actionsNotTaken[i].getGameEffect();
			yield return StartCoroutine(ChairmanViolation(actionEffect.failureToPerformAction, playersToPerformAction[i]));
			if (actionsNotTaken[i].getGameEffect().spawnsObj || actionsNotTaken[i].getGameEffect().needsObj)
			{
				objectEffect = (ObjectEffect)actionEffect;
				actionInfo = new ActionInfo();
				actionInfo.gameEffect = actionsNotTaken[i].getGameEffect();
				actionInfo.actionEffect = actionsNotTaken[i].getGameEffect().getEffectID();
				actionInfo.playerPerformingAction = playersToPerformAction[i];
				actionInfo.playerRecievingObj = playersToReceiveObject[i];
				Debug.LogFormat("{0} {1} player: {2}", moduleNameLog, playerNames[playersToPerformAction[i]], objectEffect.actionString(objectEffect.receivingPlayer));
				yield return StartCoroutine(performAction(actionInfo, true, false, false));
			}
			else
			{
				actionInfo = new ActionInfo();
				actionInfo.gameEffect = actionsNotTaken[i].getGameEffect();
				actionInfo.actionEffect = actionsNotTaken[i].getGameEffect().getEffectID();
				actionInfo.playerPerformingAction = playersToPerformAction[i];
				Debug.LogFormat("{0} {1} player: {2}", moduleNameLog, playerNames[playersToPerformAction[i]], actionEffect.actionString(XPlayerAction.NULL));
				yield return StartCoroutine(performAction(actionInfo, true, false, false));
			}
		}
	}
	void displayStartOfRound(GameRound gameRound)
	{
		PFOHandCounts = new int[players.Length];
		for (int i = 0; i < PFOHandCounts.Length; i++)
		{
			PFOHandCounts[i] = gameRound.startOfRoundInfo.startingHandCounts[i];
			playerText[i].text = gameRound.startOfRoundInfo.startingHandCounts[i] + "";
			playerMats[i].material = inactiveMat;
			playerText[i].color = inactiveText;
		}
		playButtonMesh.material = gameRound.startOfRoundInfo.playButtonColor;
		playButtonText.color = gameRound.startOfRoundInfo.playButtonTextColor;
		bool flag = true;
		foreach(ActionInfo actionInfo in gameRound.actionsPerformed)
		{
			if(actionInfo.actionEffect == Effect.CHAIRMAN_VIOLATION && actionInfo.violationInfo.violation == ViolationPhrases.FAILURE_TO_PLAY)
			{
				flag = false;
				break;
			}
		}
		if(flag)
		{
			for (int i = 0; i < gameRound.actionsPerformed.Count; i++)
			{
				if (gameRound.actionsPerformed[i].actionEffect == Effect.PLAYER_TURN_START)
				{
					playerMats[gameRound.actionsPerformed[i].playerPerformingAction].material = activeMat;
					playerText[gameRound.actionsPerformed[i].playerPerformingAction].color = activeText;
					break;
				}
			}
		}
		displayStartOfRoundObjects(gameRound);
		setupPFOButtons();
	}
	IEnumerator replayRound(GameRound gameRound)
	{
		prevPFOButton.OnInteract = null;
		playPFOButton.OnInteract = null;
		nextPFOButton.OnInteract = null;
		pfoButton.OnInteract = null;
		yield return new WaitForSeconds(0f);
		foreach(ActionInfo action in gameRound.actionsPerformed)
		{
			switch (action.actionEffect)
			{
				case Effect.START_GAME:
					for (int i = 0; i < STARTING_NUM_CARDS; i++)
					{
						for (int player = 0; player < players.Length; player++)
						{
							yield return StartCoroutine(animateDrawCard(player, RATE_OF_DRAWING_TO_START_GAME));
							PFOHandCounts[player]++;
							playerText[player].text = PFOHandCounts[player] + "";
						}
					}
					float endX = playPileMesh.transform.localPosition.x;
					float endZ = playPileMesh.transform.localPosition.z;
					float startX = drawPileMesh.transform.localPosition.x;
					float startZ = drawPileMesh.transform.localPosition.z;
					float y = 0.02f;
					float absoluteX = Mathf.Abs(endX - startX);
					float absoluteZ = Mathf.Abs(endZ - startZ);
					int rate = 100;
					float movementX = absoluteX / rate;
					if (endX < startX)
						movementX *= (-1);
					float movementZ = absoluteZ / rate;
					if (endZ < startZ)
						movementZ *= (-1);
					animateCard.material = faceDownMat;
					animateCard.transform.localPosition = new Vector3(startX, y, startZ);
					yield return new WaitForSeconds(0.001f);
					for (int i = 0; i < rate; i++)
					{
						startX += movementX;
						startZ += movementZ;
						animateCard.transform.localPosition = new Vector3(startX, y, startZ);
						yield return new WaitForSeconds(0.001f);
					}
					playPile.gameObject[0].meshRenderer.material = action.material;
					playPile.gameObject[0].changeObjectLocation(getNewObjectLocation(playPile.gameObject[0].tablePlacement, tableGridVector3, playPile.gameObject[0].standingY));
					animateCard.transform.localPosition = new Vector3(0f, 0f, 0f);
					audio.PlaySoundAtTransform(introduction.name, transform);
					yield return new WaitForSeconds(introduction.length);
					if (action.turnDirection == 1)
						audio.PlaySoundAtTransform(introCW.name, transform);
					else
						audio.PlaySoundAtTransform(introCCW.name, transform);
					yield return new WaitForSeconds(1.8f);
					playerMats[action.playerPerformingAction].material = activeMat;
					playerText[action.playerPerformingAction].color = activeText;
					yield return new WaitForSeconds(3f);
					break;
				case Effect.PLAY_CARD:
					PFOHandCounts[action.playerPerformingAction]--;
					playerText[action.playerPerformingAction].text = PFOHandCounts[action.playerPerformingAction] + "";
					yield return StartCoroutine(animatePlayCard(action.playerPerformingAction, RATE_OF_MOVING_OBJECT, action.material));
					break;
				case Effect.DRAW_CARD:
					yield return StartCoroutine(animateDrawCard(action.playerPerformingAction, RATE_OF_MOVING_OBJECT));
					PFOHandCounts[action.playerPerformingAction]++;
					playerText[action.playerPerformingAction].text = PFOHandCounts[action.playerPerformingAction] + "";
					yield return new WaitForSeconds(BUFFER_BETWEEN_ACTIONS);
					break;
				case Effect.RETURN_CARD:
					Material tempPlayPileMat = null;
					foreach(GameMainObjInfo GOI in gameRound.startOfRoundInfo.gameObjInfo)
					{
						if(GOI.gameObj.id == playPile.id)
						{
							tempPlayPileMat = GOI.gameObjMats[0];
							break;
						}
					}
					yield return StartCoroutine(animateReturnCardToPlayer(action.playerPerformingAction, RATE_OF_MOVING_OBJECT, tempPlayPileMat, action.material));
					PFOHandCounts[action.playerPerformingAction]++;
					playerText[action.playerPerformingAction].text = PFOHandCounts[action.playerPerformingAction] + "";
					yield return new WaitForSeconds(BUFFER_BETWEEN_ACTIONS);
					break;
				case Effect.CHAIRMAN_VIOLATION:
					if(action.violationInfo.violation == ViolationPhrases.FAILURE_TO_PLAY)
					{
						playButtonMesh.material = inactiveMat;
						playButtonText.color = inactiveText;
					}
					if(action.violationInfo.violation == ViolationPhrases.FAILURE_TO_SAY)
					{
						audio.PlaySoundAtTransform(chairmanViolationToSayPhrases[(int)action.violationInfo.phrase].name, transform);
						yield return new WaitForSeconds(chairmanViolationToSayPhrases[(int)action.violationInfo.phrase].length);
					}
					else
					{
						audio.PlaySoundAtTransform(violationAudioClips[(int)action.violationInfo.violation].name, transform);
						yield return new WaitForSeconds(violationAudioClips[(int)action.violationInfo.violation].length);
					}
					break;
				case Effect.PLAYER_TURN_START:
					for(int i = 0; i < players.Length; i++)
					{
						playerMats[i].material = inactiveMat;
						playerText[i].color = inactiveText;
					}
					playerMats[action.playerPerformingAction].material = activeMat;
					playerText[action.playerPerformingAction].color = activeText;
					break;
				default:
					yield return StartCoroutine(performAction(action, true, false, true));
					break;
			}
		}
		PFOIndex++;
		if (PFOIndex >= gameRounds.Count)
			displayStartOfRound(lastRoundBeforePFO);
		else
			displayStartOfRound(gameRounds[PFOIndex]);
		pfoButton.OnInteract = delegate { disablePFOMode(); return false; };
		setupPFOButtons();
	}
	void setupPFOButtons()
	{
		if (PFOIndex >= (gameRounds.Count - 1))
		{
			//Disable PFO Next Button
			nextPFOButton.OnInteract = null;
			nextPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			//Enable PFO Next Button
			nextPFOButton.OnInteract = delegate { PFOIndex++; displayStartOfRound(gameRounds[PFOIndex]); return false; };
			nextPFOButtonMesh.transform.localPosition = new Vector3(PFOButtonPosDict[PFOButtonName.NEXT].x, PFOButtonPosDict[PFOButtonName.NEXT].y, PFOButtonPosDict[PFOButtonName.NEXT].z);
		}
		if (PFOIndex >= gameRounds.Count)
		{
			//Disable PFO Play Button
			playPFOButton.OnInteract = null;
			playPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			//Enable PFO Play Button
			playPFOButton.OnInteract = delegate { StartCoroutine(replayRound(gameRounds[PFOIndex])); return false; };
			playPFOButtonMesh.transform.localPosition = new Vector3(PFOButtonPosDict[PFOButtonName.PLAY].x, PFOButtonPosDict[PFOButtonName.PLAY].y, PFOButtonPosDict[PFOButtonName.PLAY].z);
		}
		if (PFOIndex == 0)
		{
			prevPFOButton.OnInteract = null;
			prevPFOButtonMesh.transform.localPosition = new Vector3(0f, 0f, 0f);
		}
		else
		{
			prevPFOButton.OnInteract = delegate { PFOIndex--; displayStartOfRound(gameRounds[PFOIndex]); return false; };
			prevPFOButtonMesh.transform.localPosition = new Vector3(PFOButtonPosDict[PFOButtonName.PREV].x, PFOButtonPosDict[PFOButtonName.PREV].y, PFOButtonPosDict[PFOButtonName.PREV].z);
		}
	}
	IEnumerator performAction(ActionInfo action, bool animateObjectMovement, bool changeObjectInfo, bool isPFO)
	{
		yield return new WaitForSeconds(0f);
		Vector3 newLocation;
		Phrase phrase;
		ObjectEffect OE;
		//objectEffect.gameObject;
		switch (action.actionEffect)
		{
			case Effect.SAY_PHRASE:
				phrase = ((SayPhrase)action.gameEffect).phrase;
				audio.PlaySoundAtTransform(playerPhrases[action.playerPerformingAction][(int)phrase].name, transform);
				playerActionMats[action.playerPerformingAction].material = activeMat;
				yield return new WaitForSeconds(playerPhrases[action.playerPerformingAction][(int)phrase].length);
				playerActionMats[action.playerPerformingAction].material = inactiveMat;
				break;
			case Effect.TAKE_GAME_OBJECT:
				OE = (ObjectEffect)action.gameEffect;
				if(animateObjectMovement)
				{
					newLocation = getNewObjectLocation(OE.gameObject.gameObject[0].playerPlacement, playerGridVector3[action.playerRecievingObj[0]], OE.gameObject.gameObject[0].standingY);
					playerActionMats[action.playerPerformingAction].material = activeMat;
					yield return moveGameObject(OE.gameObject, newLocation, RATE_OF_MOVING_OBJECT);
					playerActionMats[action.playerPerformingAction].material = inactiveMat;
				}
				if(changeObjectInfo)
				{
					for (int i = 0; i < players.Length; i++)
					{
						if (players[i].hasObject(OE.gameObject))
						{
							players[i].removeObject(OE.gameObject);
							break;
						}
					}
					players[action.playerRecievingObj[0]].addObject(OE.gameObject, 0);
				}
				break;
			case Effect.RETURN_GAME_OBJECT:
				OE = (ObjectEffect)action.gameEffect;
				if(animateObjectMovement)
				{
					if (action.playerRecievingObj.Count == 0)
					{
						newLocation = getNewObjectLocation(OE.gameObject.gameObject[0].tablePlacement, tableGridVector3, OE.gameObject.gameObject[0].standingY);
						yield return moveGameObject(OE.gameObject, newLocation, RATE_OF_MOVING_OBJECT);
					}
					else
					{
						newLocation = getNewObjectLocation(OE.gameObject.gameObject[0].playerPlacement, playerGridVector3[action.playerRecievingObj[0]], OE.gameObject.gameObject[0].standingY);
						yield return moveGameObject(OE.gameObject, newLocation, RATE_OF_MOVING_OBJECT);
					}
				}
				break;
		}
		if(animateObjectMovement && !isPFO)
		{
			gameRounds[gameRounds.Count - 1].actionsPerformed.Add(action);
		}
	}
	void initGameObjects()
	{
		foreach(MeshRenderer TGMR in tableGridMeshRenderer)
			TGMR.transform.localScale = new Vector3(0f, 0f, 0f);
		foreach (MeshRenderer PGMR in playerGridMeshRenderer)
			PGMR.transform.localScale = new Vector3(0f, 0f, 0f);
		whiteCircle = new WhiteCircle(new GameObjectObj[] { new GameObjectObj(whiteCircleMeshRenderer, new bool[][] { new bool[] { true } }, 0.0153f, 0.02f) });
		blueSquare = new BlueSquare(new GameObjectObj[] { new GameObjectObj(blueSquareMeshRenderer, new bool[][] { new bool[] { true } }, 0.0153f, 0.02f) });
	}
	List<GameMainObjInfo> getStartOfRoundObjectInfo()
	{
		List<GameMainObjInfo> GMOI = new List<GameMainObjInfo>();
		foreach (GameObjectMain gameObj in gameObjs)
		{
			Vector3[] gameObjSizes = new Vector3[gameObj.gameObject.Length];
			Vector3[] gameObjLocs = new Vector3[gameObj.gameObject.Length];
			Quaternion[] gameObjRots = new Quaternion[gameObj.gameObject.Length];
			Material[] gameObjMats = new Material[gameObj.gameObject.Length];
			for (int i = 0; i < gameObj.gameObject.Length; i++)
			{
				gameObjSizes[i] = new Vector3(gameObj.gameObject[i].meshRenderer.transform.localScale.x, gameObj.gameObject[i].meshRenderer.transform.localScale.y, gameObj.gameObject[i].meshRenderer.transform.localScale.z);
				gameObjLocs[i] = new Vector3(gameObj.gameObject[i].meshRenderer.transform.localPosition.x, gameObj.gameObject[i].meshRenderer.transform.localPosition.y, gameObj.gameObject[i].meshRenderer.transform.localPosition.z);
				gameObjRots[i] = new Quaternion(gameObj.gameObject[i].meshRenderer.transform.localRotation.x, gameObj.gameObject[i].meshRenderer.transform.localRotation.y, gameObj.gameObject[i].meshRenderer.transform.localRotation.z, gameObj.gameObject[i].meshRenderer.transform.localRotation.w);
				gameObjMats[i] = gameObj.gameObject[i].meshRenderer.material;
			}
			GameMainObjInfo GMO = new GameMainObjInfo(gameObj, gameObjSizes, gameObjLocs, gameObjRots, gameObjMats);
			//If there is any additional info needed, add it here
			GMOI.Add(GMO);
		}
		return GMOI;
	}
	void displayStartOfRoundObjects(GameRound gameRound)
	{
		foreach (GameMainObjInfo gameMainObj in gameRound.startOfRoundInfo.gameObjInfo)
		{
			for (int i = 0; i < gameMainObj.gameObj.gameObject.Length; i++)
			{
				gameMainObj.gameObj.gameObject[i].meshRenderer.transform.localScale = new Vector3(gameMainObj.gameObjSizes[i].x, gameMainObj.gameObjSizes[i].y, gameMainObj.gameObjSizes[i].z);
				gameMainObj.gameObj.gameObject[i].meshRenderer.transform.localPosition = new Vector3(gameMainObj.gameObjPositions[i].x, gameMainObj.gameObjPositions[i].y, gameMainObj.gameObjPositions[i].z);
				gameMainObj.gameObj.gameObject[i].meshRenderer.transform.localRotation = new Quaternion(gameMainObj.gameObjRotations[i].x, gameMainObj.gameObjRotations[i].y, gameMainObj.gameObjRotations[i].z, gameMainObj.gameObjRotations[i].w);
				gameMainObj.gameObj.gameObject[i].meshRenderer.material = gameMainObj.gameObjMats[i];
				//Any other stuff you need to set up, add it here
				
			}
		}
	}
	public GameState getGameState(List<Rule> triggeredRules, Deck PP)
	{
		GameState gameState = new GameState();
		gameState.playPile = PP;
		gameState.players = players;
		gameState.turnDirection = turnDirection;
		gameState.currentPlayer = turnCounter;
		gameState.currPlayerPlayed = currPlayerPlayed;
		gameState.prevPlayerPlayed = prevPlayerPlayed;
		gameState.highestCardValue = HIGHEST_CARD_VALUE;
		gameState.triggeredRules = triggeredRules;
		gameState.handCountsAfterDrawPlay = handCountsAfterDrawPlay;
		gameState.whiteCircle = whiteCircle;
		gameState.blueSquare = blueSquare;

		return gameState;
	}
}

