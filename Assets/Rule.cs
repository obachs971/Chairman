using System.Collections;
using System.Collections.Generic;
using TriggerStuff;
using UnityEngine;

public class Rule 
{
	private Trigger trigger;
	private GameEffect effect;
	public Rule(Trigger trigger, GameEffect effect)
	{
		this.trigger = trigger;
		this.effect = effect;
	}
	public bool HasTriggered(GameState gameState)
	{
		return trigger.HasTriggered(gameState);
	}
	public GameEffect getGameEffect()
	{
		return effect;
	}
	public string toString()
	{
		return trigger.toString() + effect.toString();
	}
}
