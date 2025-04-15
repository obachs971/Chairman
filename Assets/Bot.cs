using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardStuff;
public class Bot
{

	private List<Card> hand;
	private List<Card> playPileThought;
	private Dictionary<Rule, int> ruleScores;
	private Dictionary<Rule, int> interRuleScores;
	private Dictionary<Rule, int> results;
	private int MAX_CARDS_IN_THOUGHT = 5;
	private Dictionary<int, GameObjectObj> heldObjects;
	public Bot()
	{
		hand = new List<Card>();
		playPileThought = new List<Card>();
		ruleScores = new Dictionary<Rule, int>();
		interRuleScores = new Dictionary<Rule, int>();
		results = new Dictionary<Rule, int>();
		heldObjects = new Dictionary<int, GameObjectObj>();
	}
	public void addRule(Rule rule)
	{
		ruleScores.Add(rule, 0);
		interRuleScores.Add(rule, 0);
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
	public bool willTriggerRule(Rule rule, GameState gameState, List<Rule> triggeredCardChangingRules, List<Rule> triggeredValidChangingRules, int nextTurnCounter, int prevPlayer)
	{
		bool willFollowRule = results[rule] <= ruleScores[rule];
		if (willFollowRule)
		{
			bool b = rule.HasTriggered(gameState, nextTurnCounter, prevPlayer);
			if (b)
			{
				gameState.playPile = getPlayPileThought(playPileThought);
				b = rule.HasTriggered(gameState, nextTurnCounter, prevPlayer);
				if (b)
				{
					addToInterRuleScore(rule, 10);
					foreach (Rule TVCR in triggeredCardChangingRules)
						addToInterRuleScore(TVCR, 10);
					foreach (Rule TVCR in triggeredValidChangingRules)
						addToInterRuleScore(TVCR, 10);
					return true;
				}
				else
				{
					addToInterRuleScore(rule, 5);
					foreach (Rule TVCR in triggeredCardChangingRules)
						addToInterRuleScore(TVCR, 5);
					foreach (Rule TVCR in triggeredValidChangingRules)
						addToInterRuleScore(TVCR, 5);
					return false;
				}
			}
			else
			{
				gameState.playPile = getPlayPileThought(playPileThought);
				b = rule.HasTriggered(gameState, nextTurnCounter, prevPlayer);
				if (b)
				{
					addToInterRuleScore(rule, 5);
					foreach (Rule TVCR in triggeredCardChangingRules)
						addToInterRuleScore(TVCR, 5);
					foreach (Rule TVCR in triggeredValidChangingRules)
						addToInterRuleScore(TVCR, 5);
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
			bool b = rule.HasTriggered(gameState, nextTurnCounter, prevPlayer);
			if (b)
				addToInterRuleScore(rule, 10);
			return false;
		}
	}
	public void addScoresToTheEndOfRound()
	{
		foreach (KeyValuePair<Rule, int> score in interRuleScores)
		{
			if (ruleScores[score.Key] < 100 && score.Value > 0)
			{
				ruleScores[score.Key] += score.Value;
			}
		}
		foreach (KeyValuePair<Rule, int> score in ruleScores)
		{
			interRuleScores[score.Key] = 0;
		}
	}
	public bool willTriggerRule(Rule rule, GameState gameState, int nextTurnCounter, int prevPlayer)
	{
		bool willFollowRule = results[rule] <= ruleScores[rule];
		if (willFollowRule)
		{
			return rule.HasTriggered(gameState, nextTurnCounter, prevPlayer);
		}
		else
		{
			return false;
		}
	}
	public void addToInterRuleScore(Rule rule, int score)
	{
		interRuleScores[rule] += score;
	}

	public void addPlayPileCard(Card card)
	{
		while (playPileThought.Count >= MAX_CARDS_IN_THOUGHT)
			playPileThought.RemoveAt(0);
		playPileThought.Add(card);
	}
	public Deck getPlayPileThought(List<Card> allCards)
	{
		Deck PPT = new Deck(0, allCards, null, 0f, "Play Pile Thought");
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
