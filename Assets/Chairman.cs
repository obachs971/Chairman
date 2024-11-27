using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardStuff;
using TriggerStuff;
using System.Linq;

public enum ViolationPhrases
{
	FAILURE_TO_PLAY = 0,
	PLAYING_OUT_OF_TURN = 1,
	BAD_CARD = 2,
	TALKING = 3,
	TAKE_WHITE_CIRCLE = 4,
	BAD_TAKING_WHITE_CIRCLE = 5,
}
public class Chairman : MonoBehaviour
{
	private int nextTurnCounter;
	public KMBombModule module;
	public new KMAudio audio;

	private Deck drawPile;
	private Deck playPile;
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

	public MeshRenderer[] playerMats;
	public MeshRenderer[] playerActionMats;
	public Material activeMat;
	public Material inactiveMat;

	public TextMesh[] playerText;

	public MeshRenderer drawPileMesh;
	public MeshRenderer playPileMesh;

	public MeshRenderer animateCard;
	public Material faceDownMat;

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
	private List<Rule> objectMovingRules;
	private List<Rule> triggeredValidCardRules;
	private List<Rule> triggeredCardChangingRules;
	private List<Rule> previousTriggeredRules;

	private bool prevPlayerPlayed;
	private bool currPlayerPlayed;
	//private bool willTryAgain;
	private int HIGHEST_CARD_VALUE;
	private int LOWEST_CARD_VALUE;
	//private List<Effect> triggeredEffectEnums;
	private bool hasInitPlayerDecisions = false;
	private int prevPlayer;
	private int lastPlayerPassed;
	private int lastPlayerPlayed;
	private int[] initialHandCounts;

	public MeshRenderer whiteCircleMeshRenderer;
	public WhiteCircle whiteCircle = null;
	

	void Awake()
	{
		initGameObjects();

		playPileMesh.transform.localPosition = new Vector3(playPileMesh.transform.localPosition.x, 0f, playPileMesh.transform.localPosition.z);
		playerPhrases = new AudioClip[][] { player1Phrases, player2Phrases, player3Phrases, player4Phrases, player5Phrases, player6Phrases };
		players = new Bot[6];
		for (int aa = 0; aa < players.Length; aa++)
			players[aa] = new Bot();
		List<GameObjectMain> gameObjs = new List<GameObjectMain>();
		List<bool> playsToTable = new List<bool>();
		List<bool> playsToPlayer = new List<bool>();

		tableGridVector3 = new Vector3[][]
		{
			new Vector3[]{ tableGridMeshRenderer[0].transform.localPosition, tableGridMeshRenderer[1].transform.localPosition, tableGridMeshRenderer[2].transform.localPosition, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), tableGridMeshRenderer[9].transform.localPosition, tableGridMeshRenderer[10].transform.localPosition, tableGridMeshRenderer[11].transform.localPosition, tableGridMeshRenderer[12].transform.localPosition },
			new Vector3[]{ tableGridMeshRenderer[3].transform.localPosition, tableGridMeshRenderer[4].transform.localPosition, tableGridMeshRenderer[5].transform.localPosition, new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), tableGridMeshRenderer[13].transform.localPosition, tableGridMeshRenderer[14].transform.localPosition, tableGridMeshRenderer[15].transform.localPosition, tableGridMeshRenderer[16].transform.localPosition },
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
		//Generate random rules
		List<Trigger> triggerList = new List<Trigger>()
		{
			new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King),
			//new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds),
			//new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King),
			//new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds),
			//new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4),
			//new sameSuit(), new differentSuit(),
			//new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Spades), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Diamonds, Card.CardSuit.Diamonds),
			//new CurrentPlayerHasXCards(1), new CurrentPlayerHasXCards(2), new CurrentPlayerHasXCards(3), new CurrentPlayerHasXCards(4),
		};
		List<GameEffect> gameEffectList = new List<GameEffect>()
		{
			//new ReverseTurnOrder(),
			//new SkipNextPlayerTurn(),
			//new PlayAgain(),
			new SayPhrase(Phrase.HAVE_A_NICE_DAY, new List<XPlayerAction>() { XPlayerAction.CCW_OF_CURRENT,XPlayerAction.CURRENT_PLAYER,XPlayerAction.CW_OF_CURRENT,XPlayerAction.LAST_PLAYER_PASSED,XPlayerAction.LAST_PLAYER_PLAYED,XPlayerAction.NEXT_PLAYER,XPlayerAction.OPPOSITE_PLAYER,XPlayerAction.PREVIOUS_PLAYER,XPlayerAction.PLAYER_WITH_WHITE_CIRCLE}),
			//new ForbiddenSuit(),
			//new AddOffsetToRank(1),
			new TakeWhiteCircle(whiteCircle)

		};
		List<XPlayerAction> xPlayerActions = new List<XPlayerAction>()
		{
			//XPlayerAction.CCW_OF_CURRENT,
			XPlayerAction.CURRENT_PLAYER,
			//XPlayerAction.CW_OF_CURRENT,
			//XPlayerAction.LAST_PLAYER_PASSED,
			//XPlayerAction.LAST_PLAYER_PLAYED,
			//XPlayerAction.NEXT_PLAYER,
			//XPlayerAction.OPPOSITE_PLAYER,
			//XPlayerAction.PREVIOUS_PLAYER
		};
		gameEffectList.Shuffle();
		rules = new List<Rule>();
		objectMovingRules = new List<Rule>();
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
								//1. Does this object already exist on the table
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
		foreach (Rule rule in rules)
		{
			Debug.LogFormat("{0}", rule.toString());
			foreach (Bot player in players)
				player.addRule(rule);
		}
		Debug.LogFormat("RULE BLOCKER");
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
		drawPile = new Deck(MAX_DECK_SIZE_FOR_DRAW_PILE, cardList);
		playPile = new Deck(0, cardList);
		turnCounter = Random.Range(0, players.Length);
		turnDirection = new int[] { -1, 1 }[Random.Range(0, 2)];
		thoughtTurnCounters = new List<int>();
		thoughtTurnCounters.Add(turnCounter);
		currPlayerPlayed = false;
		prevPlayerPlayed = false;
		playButton.OnInteract = delegate { StartCoroutine(StartGame()); return false; };
		prevPlayer = -1;
		lastPlayerPassed = -1;
		lastPlayerPlayed = -1;
		initialHandCounts = null;
		previousTriggeredRules = new List<Rule>();
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
		playButton.OnInteract = null;
		yield return new WaitForSeconds(0f);
		for (int i = 0; i < 7; i++)
		{
			for (int player = 0; player < players.Length; player++)
			{
				yield return StartCoroutine(DrawCard(player, 25));
			}
		}
		playPile.AddToDeck(drawPile.Draw());
		if (drawPile.getDeckSize() == 0)
			drawPile = new Deck(MAX_DECK_SIZE_FOR_DRAW_PILE, cardList);
		Card c = playPile.GetCard(0);
		playPileMesh.material = cardMats[c.toString()];
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
		playPileMesh.transform.localPosition = new Vector3(playPileMesh.transform.localPosition.x, drawPileMesh.transform.localPosition.y, playPileMesh.transform.localPosition.z);
		animateCard.transform.localPosition = new Vector3(0f, 0f, 0f);
		validCards = new List<Card>();
		audio.PlaySoundAtTransform(introduction.name, transform);
		yield return new WaitForSeconds(introduction.length);
		if (turnDirection == 1)
			audio.PlaySoundAtTransform(introCW.name, transform);
		else
			audio.PlaySoundAtTransform(introCCW.name, transform);
		yield return new WaitForSeconds(1.8f);
		playerMats[turnCounter].material = activeMat;
		playerText[turnCounter].color = activeText;
		yield return new WaitForSeconds(3f);
		playButton.OnInteract = delegate { StartCoroutine(PlayRound()); return false; };

		HAVE_PLAY_ON_REPEAT = true;
		if (HAVE_PLAY_ON_REPEAT)
		{

			StartCoroutine(GameLoop());

		}
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
	IEnumerator PlayRound()
	{
		yield return new WaitForSeconds(0f);
		if (initialHandCounts == null)
			initialHandCounts = new int[] { players[0].getHandCount(), players[1].getHandCount(), players[2].getHandCount(), players[3].getHandCount(), players[4].getHandCount(), players[5].getHandCount() };
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
			if (playCardIndex >= 0)
			{
				playerMats[thoughtTurnCounters[0]].material = activeMat;
				playerText[thoughtTurnCounters[0]].color = activeText;
				yield return StartCoroutine(PlayCard(thoughtTurnCounters[0], 50, playCardIndex));
				yield return StartCoroutine(ReturnCardToPlayer(thoughtTurnCounters[0], 50));
				yield return StartCoroutine(DrawCard(thoughtTurnCounters[0], 50));
				audio.PlaySoundAtTransform(violationAudioClips[1].name, transform);
				yield return new WaitForSeconds(violationAudioClips[1].length);
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
						yield return StartCoroutine(DrawCard(turnCounter, 50));
						hasDrawn = true;
						goto tryToPlayAgain;
					}
					else //Player is passing
					{
						//Checking if any rules have been triggered
						//Debug.LogFormat("CHECKING RULES");
						List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), playPile), playPile);
						HasTriggeredTurnBasedRules(TR, triggeredCardChangingRules, playPile);
						yield return StartCoroutine(HasTriggeredActionBasedRules(TR, triggeredCardChangingRules, playPile));
						prevPlayer = turnCounter + 0;
						lastPlayerPassed = turnCounter + 0;
						turnCounter = mod(nextTurnCounter + turnDirection, players.Length);
						playButton.OnInteract = delegate { StartCoroutine(PlayRound()); return false; };
						hasInitPlayerDecisions = false;
						prevPlayerPlayed = currPlayerPlayed;
						initialHandCounts = null;
						previousTriggeredRules = new List<Rule>();
						previousTriggeredRules.AddRange(TR);
					}
				}
				else //Player is playing a card
				{
					yield return StartCoroutine(PlayCard(turnCounter, 50, playCardIndex));
					currPlayerPlayed = true;
					List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), playPile), playPile);
					triggeredCardChangingRules = new List<Rule>();
					HasTriggeredCardChangingRules(TR, playPile, true);
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
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TVCR, getGameState(TR, playPile), 0, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToRuleScore(TVCR, score);
							}
							foreach (Rule TCCR in triggeredCardChangingRules)
							{
								int score = 5;
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TCCR, getGameState(TR, playPile), 0, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToRuleScore(TCCR, score);
							}
						}
						currPlayerPlayed = false;
						yield return StartCoroutine(ReturnCardToPlayer(thoughtTurnCounters[0], 50));
						yield return StartCoroutine(DrawCard(thoughtTurnCounters[0], 50));
						audio.PlaySoundAtTransform(violationAudioClips[2].name, transform);
						yield return new WaitForSeconds(violationAudioClips[1].length);
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
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TVCR, getGameState(TR, playPile), 0, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToRuleScore(TVCR, score);
							}
							foreach (Rule TCCR in triggeredCardChangingRules)
							{
								int score = 0;
								if (players[mod(turnCounter + i, players.Length)].willTriggerRule(TCCR, getGameState(TR, playPile), 0, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
									score = 10;
								players[mod(turnCounter + i, players.Length)].addToRuleScore(TCCR, score);
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
						HasTriggeredTurnBasedRules(TR, triggeredCardChangingRules, playPile);
						yield return StartCoroutine(HasTriggeredActionBasedRules(TR, triggeredCardChangingRules, playPile));
						prevPlayer = turnCounter + 0;
						lastPlayerPlayed = turnCounter + 0;
						turnCounter = mod(nextTurnCounter + turnDirection, players.Length);
						playButton.OnInteract = delegate { StartCoroutine(PlayRound()); return false; };
						hasInitPlayerDecisions = false;
						prevPlayerPlayed = currPlayerPlayed;
						initialHandCounts = null;
						previousTriggeredRules = new List<Rule>();
						previousTriggeredRules.AddRange(triggeredCardChangingRules);
						previousTriggeredRules.AddRange(triggeredValidCardRules);
						previousTriggeredRules.AddRange(TR);
					}
				}
			}
			else //Player is drawing and passing
			{
				yield return StartCoroutine(DrawCard(turnCounter, 50));
				List<Rule> TR = GetListOfTriggeredRules(getGameState(new List<Rule>(), playPile), playPile);
				HasTriggeredTurnBasedRules(TR, triggeredCardChangingRules, playPile);
				yield return StartCoroutine(HasTriggeredActionBasedRules(TR, triggeredCardChangingRules, playPile));
				prevPlayer = turnCounter + 0;
				lastPlayerPassed = turnCounter + 0;
				turnCounter = mod(nextTurnCounter + turnDirection, players.Length);
				playButton.OnInteract = delegate { StartCoroutine(PlayRound()); return false; };
				hasInitPlayerDecisions = false;
				prevPlayerPlayed = currPlayerPlayed;
				initialHandCounts = null;
				previousTriggeredRules = new List<Rule>();
				previousTriggeredRules.AddRange(TR);
			}
		}
	}
	IEnumerator DrawCard(int player, int rate)
	{
		Card c = drawPile.Draw();
		if (drawPile.getDeckSize() == 0)
			drawPile = new Deck(MAX_DECK_SIZE_FOR_DRAW_PILE, cardList);
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
		players[player].Draw(c);
		playerText[player].text = players[player].getHandCount() + "";
		yield return new WaitForSeconds(0.1f);
	}
	
	IEnumerator PlayCard(int player, int rate, int cardIndex)
	{
		Card card = players[player].Play(cardIndex);
		playerText[player].text = players[player].getHandCount() + "";

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
		animateCard.material = cardMats[card.toString()];
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
		playPile.AddToDeck(card);
		playPileMesh.material = cardMats[card.toString()];
		yield return new WaitForSeconds(0.1f);
	}
	IEnumerator ReturnCardToPlayer(int player, int rate)
	{
		Card card = playPile.RemoveCard(0);
		card.revertCardChanges();
		playPileMesh.material = cardMats[playPile.GetCard(0).toString()];
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
		animateCard.material = cardMats[card.toString()];
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
		players[player].Draw(card);
		playerText[player].text = players[player].getHandCount() + "";
		yield return new WaitForSeconds(0.1f);
	}

	IEnumerator PlayMissedRound()
	{
		playButtonMesh.material = inactiveMat;
		playButtonText.color = inactiveText;
		playButton.OnInteract = null;
		yield return StartCoroutine(DrawCard(turnCounter, 50));
		audio.PlaySoundAtTransform(violationAudioClips[0].name, transform);
		yield return new WaitForSeconds(violationAudioClips[0].length);
		thoughtTurnCounters.Add(turnCounter);
		StartCoroutine(PlayRound());
	}
	List<Rule> GetListOfTriggeredRules(GameState gameState, Deck PP)
	{
		//Debug.LogFormat("SIZE OF RULES: {0}", rules.Count);
		List<Rule> tempTrigRules = new List<Rule>();
		List<Rule> TR = new List<Rule>();
		foreach (Rule rule in rules)
		{
			int playerTakingAction = 0;
			if(rule.getGameEffect().isAction)
				playerTakingAction = new XPlayerIndex().getPlayerIndex(((ActionEffect)rule.getGameEffect()).xPlayerAction, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
			if (rule.HasTriggered(gameState, playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
				tempTrigRules.Add(rule);
		}
		do
		{
			TR = new List<Rule>();
			TR.AddRange(tempTrigRules);
			gameState = getGameState(TR, PP);
			tempTrigRules = new List<Rule>();
			foreach (Rule rule in rules)
			{
				int playerTakingAction = 0;
				if (rule.getGameEffect().isAction)
					playerTakingAction = new XPlayerIndex().getPlayerIndex(((ActionEffect)rule.getGameEffect()).xPlayerAction, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
				if (rule.HasTriggered(gameState, playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
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
			int playerTakingAction = 0;
			if (rule.getGameEffect().isAction)
				playerTakingAction = new XPlayerIndex().getPlayerIndex(((ActionEffect)rule.getGameEffect()).xPlayerAction, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
			if (players[n].willTriggerRule(rule, getGameState(TR, PP), playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
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
		yield return new WaitForSeconds(0.1f);
	}
	int mod(int n, int m)
	{
		return (n % m + m) % m;
	}
	private void HasTriggeredCardChangingRules(List<Rule> TR, Deck PP, bool notPlayer)
	{
		List<Rule> cardChangeRules = new List<Rule>();
		List<Effect> cardChangeEffects = new List<Effect>();
		foreach (Rule rule in TR)
		{
			switch (rule.getGameEffect().getEffectID())
			{
				case Effect.ADD_OFFSET_TO_RANK:
					cardChangeRules.Add(rule);
					cardChangeEffects.Add(rule.getGameEffect().getEffectID());
					break;
			}
		}
		if (cardChangeEffects.Contains(Effect.ADD_OFFSET_TO_RANK))
		{
			int offset = 0;
			foreach (Rule cardChangeRule in cardChangeRules)
			{
				if (cardChangeRule.getGameEffect().getEffectID() == Effect.ADD_OFFSET_TO_RANK)
					offset += ((AddOffsetToRank)cardChangeRule.getGameEffect()).offset;
			}
			Card card = PP.RemoveCard(0);
			int rankVal = mod(((int)card.getRank()) - LOWEST_CARD_VALUE + offset, HIGHEST_CARD_VALUE) + LOWEST_CARD_VALUE;
			card.changeCardValues((Card.CardRank)rankVal, card.getSuit());
			PP.AddToDeck(card);
		}
		if (notPlayer)
		{
			triggeredCardChangingRules.AddRange(cardChangeRules);
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
		List<Effect> triggeredEffectEnums = new List<Effect>();
		foreach (Rule rule in TR)
			triggeredEffectEnums.Add(rule.getGameEffect().getEffectID());
		List<Effect> triggeredValidCardEffects = new List<Effect>();
		if (triggeredEffectEnums.Contains(Effect.FORBIDDEN_SUIT))
		{
			triggeredValidCardEffects.Add(Effect.FORBIDDEN_SUIT);
			VC = new List<Card>();
			foreach (Card card in cardList)
			{
				if (currCard.getSuit() != card.getSuit())
					VC.Add(card);
			}
		}
		if (notPlayer)
		{
			triggeredValidCardRules = new List<Rule>();
			foreach (Effect TVCE in triggeredValidCardEffects)
			{
				foreach (Rule tr in TR)
				{
					if (tr.getGameEffect().getEffectID() == TVCE)
						triggeredValidCardRules.Add(tr);
				}
			}
			string[] temp = new string[VC.Count];
			for (int i = 0; i < VC.Count; i++)
				temp[i] = VC[i].toString();
			//Debug.LogFormat("ALL VALID CARDS: {0}", string.Join(",", temp));
		}
		return VC;
	}
	void HasTriggeredTurnBasedRules(List<Rule> TR, List<Rule> TCCR, Deck PP)
	{
		nextTurnCounter = turnCounter;
		GameState gameState = getGameState(TR, PP);
		int[] playerTurnDirectionBrain = new int[players.Length];
		int[] playerTurnCounterBrain = new int[players.Length];
		List<Effect> triggeredEffectEnums = new List<Effect>();
		List<Effect> ruleEffectEnums = new List<Effect>();
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
				turnDirection *= (-1);
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].willTriggerRule(rules[ruleEffectEnums.IndexOf(Effect.REVERSE_TURN_ORDER)], gameState, TCCR, 0, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
					playerTurnDirectionBrain[i] *= (-1);
			}
		}
		if (ruleEffectEnums.Contains(Effect.PLAY_AGAIN))
		{
			if (triggeredEffectEnums.Contains(Effect.PLAY_AGAIN))
				nextTurnCounter = mod(nextTurnCounter - turnDirection, players.Length);
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].willTriggerRule(rules[ruleEffectEnums.IndexOf(Effect.PLAY_AGAIN)], gameState, TCCR, 0, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
					playerTurnCounterBrain[i] = mod(playerTurnCounterBrain[i] - playerTurnDirectionBrain[i], players.Length);
			}
		}
		if (ruleEffectEnums.Contains(Effect.SKIP_NEXT_PLAYER_TURN))
		{
			if (triggeredEffectEnums.Contains(Effect.SKIP_NEXT_PLAYER_TURN))
				nextTurnCounter = mod(nextTurnCounter + turnDirection, players.Length);
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].willTriggerRule(rules[ruleEffectEnums.IndexOf(Effect.SKIP_NEXT_PLAYER_TURN)], gameState, TCCR, 0, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
					playerTurnCounterBrain[i] = mod(playerTurnCounterBrain[i] + playerTurnDirectionBrain[i], players.Length);
			}
		}
		if (TR.Count > 0)
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
	IEnumerator HasTriggeredActionBasedRules(List<Rule> TR, List<Rule> TCCR, Deck PP)
	{
		yield return new WaitForSeconds(0f);
		List<Rule> actionsNotTaken = new List<Rule>();
		List<int> playerToPerformAction = new List<int>();
		int playerIndex;
		int playerThatPossessObject = -1;
		Vector3 newLocation;
		//Actions that don't involve object movement
		foreach (Rule rule in rules)
		{
			if(rule.getGameEffect().isAction)
			{
				playerIndex = new XPlayerIndex().getPlayerIndex(((ActionEffect)rule.getGameEffect()).xPlayerAction, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
				if (playerIndex >= 0)
				{
					bool hasTriggered = rule.HasTriggered(getGameState(TR, PP), playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
					switch (rule.getGameEffect().getEffectID())
					{
						case Effect.SAY_PHRASE:
							if (players[playerIndex].willTriggerRule(rule, getGameState(TR, PP), TCCR, playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
							{
								for (int i = 1; i < players.Length; i++)
									players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
								SayPhrase sayPhrase = (SayPhrase)rule.getGameEffect();
								audio.PlaySoundAtTransform(playerPhrases[playerIndex][(int)sayPhrase.phrase].name, transform);
								playerActionMats[playerIndex].material = activeMat;
								yield return new WaitForSeconds(playerPhrases[playerIndex][(int)Phrase.HAVE_A_NICE_DAY].length);
								if (!hasTriggered)
								{
									yield return StartCoroutine(DrawCard(playerIndex, 50));
									audio.PlaySoundAtTransform(violationAudioClips[(int)ViolationPhrases.TALKING].name, transform);
									yield return new WaitForSeconds(violationAudioClips[(int)ViolationPhrases.TALKING].length);
								}
								playerActionMats[playerIndex].material = inactiveMat;
							}
							else if (hasTriggered)
							{
								for (int i = 1; i < players.Length; i++)
									players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
								actionsNotTaken.Add(rule);
								playerToPerformAction.Add(playerIndex);
							}
							break;
					}
				}
				
			}
		}
		//Actions that involve object movement
		foreach (Rule rule in objectMovingRules)
		{
			playerIndex = new XPlayerIndex().getPlayerIndex(((ActionEffect)rule.getGameEffect()).xPlayerAction, getGameState(null, null), turnCounter, turnDirection, nextTurnCounter, players.Length, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
			if (playerIndex >= 0)
			{
				bool hasTriggered = rule.HasTriggered(getGameState(TR, PP), playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
				switch (rule.getGameEffect().getEffectID())
				{
					case Effect.TAKE_WHITE_CIRCLE:
						if (players[playerIndex].willTriggerRule(rule, getGameState(TR, PP), TCCR, playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed))
						{
							for (int i = 1; i < players.Length; i++)
								players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
							playerThatPossessObject = -1;
							for (int i = 0; i < players.Length; i++)
							{
								if (players[i].hasObject(whiteCircle))
								{
									players[i].removeObject(whiteCircle);
									playerThatPossessObject = i;
									break;
								}
							}
							newLocation = getNewObjectLocation(whiteCircle.gameObject[0].playerPlacement, playerGridVector3[playerIndex], whiteCircle.gameObject[0].standingY);
							playerActionMats[playerIndex].material = activeMat;
							yield return moveGameObject(whiteCircle, newLocation, 50);
							players[playerIndex].addObject(whiteCircle, 0);
							if (!hasTriggered)
							{
								yield return StartCoroutine(DrawCard(playerIndex, 50));
								audio.PlaySoundAtTransform(violationAudioClips[(int)ViolationPhrases.BAD_TAKING_WHITE_CIRCLE].name, transform);
								yield return new WaitForSeconds(violationAudioClips[(int)ViolationPhrases.BAD_TAKING_WHITE_CIRCLE].length);
								players[playerIndex].removeObject(whiteCircle);
								if (playerThatPossessObject == -1)
									newLocation = getNewObjectLocation(whiteCircle.gameObject[0].tablePlacement, tableGridVector3, whiteCircle.gameObject[0].standingY);
								else
								{
									newLocation = getNewObjectLocation(whiteCircle.gameObject[0].playerPlacement, playerGridVector3[playerThatPossessObject], whiteCircle.gameObject[0].standingY);
									players[playerThatPossessObject].addObject(whiteCircle, 0);
								}
								yield return moveGameObject(whiteCircle, newLocation, 50);
							}
							playerActionMats[playerIndex].material = inactiveMat;
						}
						else if (hasTriggered)
						{
							for (int i = 1; i < players.Length; i++)
								players[mod(playerIndex + i, players.Length)].willTriggerRule(rule, getGameState(TR, PP), TCCR, playerIndex, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
							actionsNotTaken.Add(rule);
							playerToPerformAction.Add(playerIndex);
							for (int i = 0; i < players.Length; i++)
							{
								if (players[i].hasObject(whiteCircle))
								{
									players[i].removeObject(whiteCircle);
									break;
								}
							}
							players[playerIndex].addObject(whiteCircle, 0);
						}
						break;
				}
			}
		}
		for(int i = 0; i < actionsNotTaken.Count; i++)
		{
			switch (actionsNotTaken[i].getGameEffect().getEffectID())
			{
				case Effect.SAY_PHRASE:
					playerIndex = playerToPerformAction[i];
					SayPhrase sayPhrase = (SayPhrase)actionsNotTaken[i].getGameEffect();
					yield return StartCoroutine(DrawCard(playerIndex, 50));
					audio.PlaySoundAtTransform(chairmanViolationToSayPhrases[(int)sayPhrase.phrase].name, transform);
					yield return new WaitForSeconds(chairmanViolationToSayPhrases[(int)sayPhrase.phrase].length);
					audio.PlaySoundAtTransform(playerPhrases[playerIndex][(int)sayPhrase.phrase].name, transform);
					playerActionMats[playerIndex].material = activeMat;
					yield return new WaitForSeconds(playerPhrases[playerIndex][(int)sayPhrase.phrase].length);
					playerActionMats[playerIndex].material = inactiveMat;
					break;
				case Effect.TAKE_WHITE_CIRCLE:
					playerIndex = playerToPerformAction[i];
					yield return StartCoroutine(DrawCard(playerIndex, 50));
					audio.PlaySoundAtTransform(violationAudioClips[(int)ViolationPhrases.TAKE_WHITE_CIRCLE].name, transform);
					yield return new WaitForSeconds(violationAudioClips[(int)ViolationPhrases.TAKE_WHITE_CIRCLE].length);
					newLocation = getNewObjectLocation(whiteCircle.gameObject[0].playerPlacement, playerGridVector3[playerIndex], whiteCircle.gameObject[0].standingY);
					playerActionMats[playerIndex].material = activeMat;
					yield return moveGameObject(whiteCircle, newLocation, 50);
					playerActionMats[playerIndex].material = inactiveMat;
					break;
			}
		}
	}
	
	void initGameObjects()
	{
		foreach(MeshRenderer TGMR in tableGridMeshRenderer)
			TGMR.transform.localScale = new Vector3(0f, 0f, 0f);
		foreach (MeshRenderer PGMR in playerGridMeshRenderer)
			PGMR.transform.localScale = new Vector3(0f, 0f, 0f);
		whiteCircle = new WhiteCircle(new GameObjectObj[] { new GameObjectObj(whiteCircleMeshRenderer, new bool[][] { new bool[] { true } }, 0.0153f, 0.02f) });
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
		gameState.initialHandCounts = initialHandCounts;
		gameState.whiteCircle = whiteCircle;

		return gameState;
	}
}

