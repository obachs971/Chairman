using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardStuff;
using TriggerStuff;

public class Chairman : MonoBehaviour {

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
	public Material activeMat;
	public Material inactiveMat;

	public TextMesh[] playerText;

	public MeshRenderer playPileMesh;

	public MeshRenderer animateCard;
	public Material faceDownMat;

	public AudioClip introduction;
	public AudioClip introCW;
	public AudioClip introCCW;

	public AudioClip[] violationAudioClips;

	public Material[] cardMatList;
	private Dictionary<string, Material> cardMats;

	private Color inactiveText = Color.white;
	private Color activeText = Color.black;

	private int MAX_NUM_RULES = 6;
	private List<Rule> rules;
	private List<Effect> triggeredEffects;
	void Awake () 
	{
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

		//Generate random rules
		List<Trigger> triggerList = new List<Trigger>()
		{
			new onRank(Card.CardRank.Ace), new onRank(Card.CardRank.Two), new onRank(Card.CardRank.Three), new onRank(Card.CardRank.Four), new onRank(Card.CardRank.Five), new onRank(Card.CardRank.Six), new onRank(Card.CardRank.Seven), new onRank(Card.CardRank.Eight), new onRank(Card.CardRank.Nine), new onRank(Card.CardRank.Ten), new onRank(Card.CardRank.Jack), new onRank(Card.CardRank.Queen), new onRank(Card.CardRank.King),
			new onSuit(Card.CardSuit.Spades), new onSuit(Card.CardSuit.Hearts), new onSuit(Card.CardSuit.Clubs), new onSuit(Card.CardSuit.Diamonds),
			new prevCardIsRank(Card.CardRank.Ace), new prevCardIsRank(Card.CardRank.Two), new prevCardIsRank(Card.CardRank.Three), new prevCardIsRank(Card.CardRank.Four), new prevCardIsRank(Card.CardRank.Five), new prevCardIsRank(Card.CardRank.Six), new prevCardIsRank(Card.CardRank.Seven), new prevCardIsRank(Card.CardRank.Eight), new prevCardIsRank(Card.CardRank.Nine), new prevCardIsRank(Card.CardRank.Ten), new prevCardIsRank(Card.CardRank.Jack), new prevCardIsRank(Card.CardRank.Queen), new prevCardIsRank(Card.CardRank.King),
			new prevCardIsSuit(Card.CardSuit.Spades), new prevCardIsSuit(Card.CardSuit.Hearts), new prevCardIsSuit(Card.CardSuit.Clubs), new prevCardIsSuit(Card.CardSuit.Diamonds),
			new differenceOfTwoCards(0), new differenceOfTwoCards(1), new differenceOfTwoCards(2), new differenceOfTwoCards(3), new differenceOfTwoCards(4),
			new sameSuit(), new differentSuit(),
			new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Spades), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Spades, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Hearts), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Hearts, Card.CardSuit.Diamonds),	new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Clubs), new SuitWithSuit(Card.CardSuit.Clubs, Card.CardSuit.Diamonds), new SuitWithSuit(Card.CardSuit.Diamonds, Card.CardSuit.Diamonds),
			new CurrentPlayerHasXCards(1), new CurrentPlayerHasXCards(2), new CurrentPlayerHasXCards(3), new CurrentPlayerHasXCards(4),
		};
		List<GameEffect> gameEffectList = new List<GameEffect>()
		{
			new ReverseTurnOrder(), new SkipNextPlayerTurn(), new PlayAgain()
		};
		gameEffectList.Shuffle();
		rules = new List<Rule>();
		for(int i = 0; i < MAX_NUM_RULES && i < gameEffectList.Count; i++)
		{
			int triggerIndex = gameEffectList[i].pickTrigger(triggerList);
			if(triggerIndex >= 0)
			{
				rules.Add(new Rule(triggerList[triggerIndex], gameEffectList[i]));
				Debug.LogFormat("{0}", rules[rules.Count - 1].toString());
				triggerList.RemoveAt(triggerIndex);
			}
		}
		drawPile = new Deck(10, cardList);
		playPile = new Deck(0, cardList);
		players = new Bot[6];
		for(int aa = 0; aa < players.Length; aa++)
			players[aa] = new Bot(rules);
		turnCounter = UnityEngine.Random.Range(0, players.Length);
		turnDirection = new int[] { -1, 1 }[UnityEngine.Random.Range(0, 2)];
		thoughtTurnCounters = new List<int>();
		thoughtTurnCounters.Add(turnCounter);
		playButton.OnInteract = delegate { StartCoroutine(StartGame()); return false; };
	}
	IEnumerator StartGame()
	{
		playButton.OnInteract = null;
		yield return new WaitForSeconds(0f);
		for (int i = 0; i < 7; i++)
		{
			for (int player = 0; player < players.Length; player++)
			{
				StartCoroutine(DrawCard(player, 25));
				yield return new WaitForSeconds(0.12f);
			}
		}
		playPile.AddToDeck(drawPile.Draw());
		if (drawPile.getDeckSize() == 0)
			drawPile = new Deck(10, cardList);
		Card c = playPile.getCard(playPile.getDeckSize() - 1);
		playPileMesh.material = cardMats[c.toString()];
		float endX = -0.004f;
		float endZ = -0.02f;
		float startX = -0.026f;
		float startZ = -0.02f;
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
		playPileMesh.transform.localPosition = new Vector3(-0.004f, 0.0153f, -0.02f);
		animateCard.transform.localPosition = new Vector3(0f, 0f, 0f);
		validCards = getValidCards();
		audio.PlaySoundAtTransform(introduction.name, transform);

		yield return new WaitForSeconds(5f);
		if(turnDirection == 1)
			audio.PlaySoundAtTransform(introCW.name, transform);
		else
			audio.PlaySoundAtTransform(introCCW.name, transform);
		yield return new WaitForSeconds(1.8f);
		playerMats[turnCounter].material = activeMat;
		playerText[turnCounter].color = activeText;
		yield return new WaitForSeconds(5f);
		triggeredEffects = new List<Effect>();
		playButton.OnInteract = delegate { StartCoroutine(PlayRound()); return false; };
	}
	IEnumerator DrawCard(int player, int rate)
	{
		Card c = drawPile.Draw();
		if (drawPile.getDeckSize() == 0)
			drawPile = new Deck(10, cardList);
		float endX = playerMats[player].transform.localPosition.x;
		float endZ = playerMats[player].transform.localPosition.z;
		float startX = -0.026f;
		float startZ = -0.02f;
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
	}
	IEnumerator ReturnCardToPlayer(int player, int rate)
	{
		Card card = playPile.removeCard(playPile.getDeckSize() - 1);
		playPileMesh.material = cardMats[playPile.getCard(playPile.getDeckSize() - 1).toString()];
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
	}
	IEnumerator PlayRound()
	{
		playButton.OnInteract = null;
		yield return new WaitForSeconds(0f);
		bool playedCard = false;
		for (int i = 0; i < players.Length; i++)
		{
			playerMats[i].material = inactiveMat;
			playerText[i].color = inactiveText;
		}
		if (thoughtTurnCounters.Count == 0)
		{
			playButtonMesh.material = activeMat;
			playButtonText.color = activeText;
			playButton.OnInteract = delegate { StartCoroutine(PlayMissedRound()); return false; };
		}
		else if(thoughtTurnCounters[0] != turnCounter)
		{
			int playCardIndex = -1;
			List<Card> currHand = players[thoughtTurnCounters[0]].getHand();
			if (players[thoughtTurnCounters[0]].getHandCount() == 1)
				goto wrongSkipper;
			for (int i = 0; i < currHand.Count; i++)
			{
				Card card = currHand[i];
				currHand.RemoveAt(i);
				//Here change the card based on the current rules
				Card newCard = card;
				foreach (Card validCard in validCards)
				{
					if (validCard.rank == newCard.rank && validCard.suit == newCard.suit)
					{
						playCardIndex = i;
						currHand.Insert(playCardIndex, card);
						goto wrongSkipper;
					}
				}
				currHand.Insert(i, card);
			}
			wrongSkipper:
			if (playCardIndex >= 0)
			{
				playerMats[thoughtTurnCounters[0]].material = activeMat;
				playerText[thoughtTurnCounters[0]].color = activeText;
				StartCoroutine(PlayCard(thoughtTurnCounters[0], 50, playCardIndex));
				yield return new WaitForSeconds(0.5f);
				StartCoroutine(ReturnCardToPlayer(thoughtTurnCounters[0], 50));
				yield return new WaitForSeconds(0.5f);
				StartCoroutine(DrawCard(thoughtTurnCounters[0], 50));
				yield return new WaitForSeconds(0.5f);
				audio.PlaySoundAtTransform(violationAudioClips[1].name, transform);
				yield return new WaitForSeconds(2f);
				thoughtTurnCounters.RemoveAt(0);
				StartCoroutine(PlayRound());
			}
			else
			{
				thoughtTurnCounters.RemoveAt(0);
				StartCoroutine(PlayRound());
			}
		}
		else
		{
			playerMats[turnCounter].material = activeMat;
			playerText[turnCounter].color = activeText;
			bool hasDrawn = false;
			tryToPlayAgain:
			int playCardIndex = -1;
			List<Card> currHand = players[turnCounter].getHand();
			if (players[turnCounter].getHandCount() == 1)
				goto skipper;
			for (int i = 0; i < currHand.Count; i++)
			{
				Card card = currHand[i];
				currHand.RemoveAt(i);
				//Here change the card based on the current rules
				Card newCard = card;
				foreach (Card validCard in validCards)
				{
					if (validCard.rank == newCard.rank && validCard.suit == newCard.suit)
					{
						playCardIndex = i;
						currHand.Insert(playCardIndex, card);
						goto skipper;
					}
				}
				currHand.Insert(i, card);
			}
			skipper:
			if (playCardIndex == -1)
			{
				if (!hasDrawn)
				{
					StartCoroutine(DrawCard(turnCounter, 50));
					yield return new WaitForSeconds(0.5f);
					hasDrawn = true;
					goto tryToPlayAgain;
				}
			}
			else
			{
				StartCoroutine(PlayCard(turnCounter, 50, playCardIndex));
				playedCard = true;
				yield return new WaitForSeconds(0.5f);
			}
			thoughtTurnCounters.RemoveAt(0);
			GetListOfTriggeredRules(playedCard);
			HasTriggeredTurnBasedRules();
			turnCounter = mod(turnCounter + turnDirection, players.Length);
			validCards = getValidCards();
			triggeredEffects = new List<Effect>();
			playButton.OnInteract = delegate { StartCoroutine(PlayRound()); return false; };
		}	
	}
	IEnumerator PlayMissedRound()
	{
		playButtonMesh.material = inactiveMat;
		playButtonText.color = inactiveText;
		playButton.OnInteract = null;
		yield return new WaitForSeconds(0f);
		StartCoroutine(DrawCard(turnCounter, 50));
		yield return new WaitForSeconds(0.5f);
		audio.PlaySoundAtTransform(violationAudioClips[0].name, transform);
		yield return new WaitForSeconds(3f);
		thoughtTurnCounters.Add(turnCounter);
		StartCoroutine(PlayRound());
	}

	List<Card> getValidCards()
	{
		validCards = new List<Card>();
		Card currCard = playPile.getCard(playPile.getDeckSize() - 1);
		foreach(Card card in cardList)
		{
			if (currCard.rank == card.rank || currCard.suit == card.suit)
				validCards.Add(card);
		}
		return validCards;
	}
	void GetListOfTriggeredRules(bool hasPlayed)
	{
		triggeredEffects = new List<Effect>();
		foreach(Rule rule in rules)
		{
			if (rule.HasTriggered(new GameState(playPile, players, turnDirection, turnCounter, hasPlayed)))
				triggeredEffects.Add(rule.getGameEffect().getEffectID());
		}
	}
	void HasTriggeredTurnBasedRules()
	{
		List<Effect> triggeredTurnBasedRules = new List<Effect>();
		int[] playerTurnDirectionBrain = new int[players.Length];
		int[] playerTurnCounterBrain = new int[players.Length];
		for(int i = 0; i < players.Length; i++)
		{
			playerTurnCounterBrain[i] = turnCounter;
			playerTurnDirectionBrain[i] = turnDirection;
		}
		if (triggeredEffects.Contains(Effect.REVERSE_TURN_ORDER))
		{
			turnDirection *= (-1);
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].followsRule(Effect.REVERSE_TURN_ORDER, 10))
					playerTurnDirectionBrain[i] *= (-1);
			}
			triggeredTurnBasedRules.Add(Effect.REVERSE_TURN_ORDER);
		}
		if(triggeredEffects.Contains(Effect.PLAY_AGAIN))
		{
			turnCounter = mod(turnCounter - turnDirection, players.Length);
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].followsRule(Effect.PLAY_AGAIN, 10))
					playerTurnCounterBrain[i] = mod(playerTurnCounterBrain[i] - playerTurnDirectionBrain[i], players.Length);
			}
			triggeredTurnBasedRules.Add(Effect.PLAY_AGAIN);
		}
		if (triggeredEffects.Contains(Effect.SKIP_NEXT_PLAYER_TURN))
		{
			turnCounter = mod(turnCounter + turnDirection, players.Length);
			for (int i = 0; i < players.Length; i++)
			{
				if (players[i].followsRule(Effect.SKIP_NEXT_PLAYER_TURN, 10))
					playerTurnCounterBrain[i] = mod(playerTurnCounterBrain[i] + playerTurnDirectionBrain[i], players.Length);
			}
			triggeredTurnBasedRules.Add(Effect.SKIP_NEXT_PLAYER_TURN);
		}
		if (triggeredEffects.Count > 0)
		{
			for (int i = 0; i < players.Length; i++)
			{
				int nextTurn = mod(playerTurnCounterBrain[i] + playerTurnDirectionBrain[i], players.Length);
				if (i == nextTurn)
					thoughtTurnCounters.Add(i);
			}
			thoughtTurnCounters.Shuffle();
		}
		else
		{
			thoughtTurnCounters.Add(mod(turnCounter + turnDirection, players.Length));
		}
	}
	int mod(int n, int m)
	{
		return (n % m + m) % m;
	}
}

