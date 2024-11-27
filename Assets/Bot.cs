using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardStuff;
public class Bot
{

	private List<Card> hand;
	private List<Card> playPileThought;
	private Dictionary<Rule, int> ruleScores;
	private Dictionary<Rule, int> results;
	private int MAX_CARDS_IN_THOUGHT = 5;
	private Dictionary<int, GameObjectObj> heldObjects;
	public Bot()
	{
		hand = new List<Card>();
		playPileThought = new List<Card>();
		ruleScores = new Dictionary<Rule, int>();
		results = new Dictionary<Rule, int>();
		heldObjects = new Dictionary<int, GameObjectObj>();
	}
	public void addRule(Rule rule)
	{
		ruleScores.Add(rule, 0);
		results.Add(rule, 0);
	}
	public void Draw(Card c)
	{
		hand.Add(c);
	}
	public Card Play(int n)
	{
		Card temp = hand[n];
		hand.RemoveAt(n);
		return temp;
	}
	public List<Card> getHand()
	{
		return hand;
	}
	public int getHandCount()
	{
		return hand.Count;
	}
	public void rollThoughtResult(Rule rule)
	{
		results[rule] = UnityEngine.Random.Range(0, 100) + 1;
	}
	public bool hasObject(GameObjectMain gameObject)
	{
		return heldObjects.ContainsKey(gameObject.id);
	}
	public void addObject(GameObjectMain gameObject, int index)
	{
		heldObjects.Add(gameObject.id, gameObject.gameObject[index]);
	}
	public void removeObject(GameObjectMain gameObject)
	{
		heldObjects.Remove(gameObject.id);
	}
	public bool willTriggerRule(Rule rule, GameState gameState, List<Rule> triggeredCardChangingRules, int playerTakingAction, int nextTurnCounter, int prevPlayer, int lastPlayerPassed, int lastPlayerPlayed)
	{
		bool willFollowRule = results[rule] <= ruleScores[rule];
		if (willFollowRule)
		{
			bool b = rule.HasTriggered(gameState, playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
			if (b)
			{
				gameState.playPile = getPlayPileThought(playPileThought);
				b = rule.HasTriggered(gameState, playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
				if (b)
				{
					addToRuleScore(rule, 10);
					foreach (Rule TVCR in triggeredCardChangingRules)
						addToRuleScore(TVCR, 10);
					return true;
				}
				else
				{
					addToRuleScore(rule, 5);
					foreach (Rule TVCR in triggeredCardChangingRules)
						addToRuleScore(TVCR, 5);
					return false;
				}
			}
			else
			{
				gameState.playPile = getPlayPileThought(playPileThought);
				b = rule.HasTriggered(gameState, playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
				if (b)
				{
					addToRuleScore(rule, 5);
					foreach (Rule TVCR in triggeredCardChangingRules)
						addToRuleScore(TVCR, 5);
					return true;
				}
				else
				{
					return false;
				}
			}
		}
		else
		{
			bool b = rule.HasTriggered(gameState, playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
			if (b)
				addToRuleScore(rule, 10);
			return false;
		}
	}
	public bool willTriggerRule(Rule rule, GameState gameState, int playerTakingAction, int nextTurnCounter, int prevPlayer, int lastPlayerPassed, int lastPlayerPlayed)
	{
		bool willFollowRule = results[rule] <= ruleScores[rule];
		if (willFollowRule)
		{
			return rule.HasTriggered(gameState, playerTakingAction, nextTurnCounter, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
		}
		else
		{
			return false;
		}
	}
	public void addToRuleScore(Rule rule, int score)
	{
		if (ruleScores[rule] < 100)
			ruleScores[rule] += score;
	}

	public void addPlayPileCard(Card card)
	{
		while (playPileThought.Count >= MAX_CARDS_IN_THOUGHT)
			playPileThought.RemoveAt(0);
		playPileThought.Add(card);
	}
	public Deck getPlayPileThought(List<Card> allCards)
	{
		Deck PPT = new Deck(0, allCards);
		foreach (Card card in playPileThought)
			PPT.AddToDeck(card);
		return PPT;
	}
	public void printScore()
	{
		foreach (KeyValuePair<Rule, int> score in ruleScores)
		{
			Debug.LogFormat("{0}: {1}", score.Key.toString(), score.Value);
		}
	}
}
