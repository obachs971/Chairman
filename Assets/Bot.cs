using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CardStuff;
public class Bot 
{

	private List<Card> hand;
	private Dictionary<Effect, int> brain;
	public Bot(List<Rule> rules)
	{
		hand = new List<Card>();
		brain = new Dictionary<Effect, int>();
		foreach (Rule rule in rules)
			brain.Add(rule.getGameEffect().getEffectID(), 0);
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
	public bool followsRule(Effect effect, int followScore)
	{
		int decision = UnityEngine.Random.Range(0, 100) + 1;
		bool willFollowRule = decision <= brain[effect];
		//Debug.LogFormat("{0} <= {1} -> {2}", decision, brain[effectID], willFollowRule);
		if (brain[effect] < 100)
		{
			brain[effect] += followScore;
		}
		return willFollowRule;
	}
}
