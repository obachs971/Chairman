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
	public bool HasTriggered(GameState gameState, int playerTakingAction, int nextTurnCounter, int prevPlayer, int lastPlayerPassed, int lastPlayerPlayed)
	{
		bool flag = true;
		if (effect.isAction)
		{
			flag = false;
			if(playerTakingAction >= 0)
				flag = true;	
		}
		if (!flag)
			return false;
		if (effect.spawnsObj || effect.needsObj)
		{
			ObjectEffect OE = (ObjectEffect)effect;
			if (OE.mustBeOnPlayerToTrigger)
			{
				flag = false;
				foreach(Bot player in gameState.players)
				{
					if(player.hasObject(OE.gameObject))
					{
						flag = true;
						break;
					}
				}
			}
			if (!flag)
				return false;
			if (OE.receivingPlayer != XPlayerAction.NULL)
			{
				int playerReceiving = new XPlayerIndex().getPlayerIndex(OE.receivingPlayer, gameState, gameState.currentPlayer, gameState.turnDirection, nextTurnCounter, gameState.players.Length, prevPlayer, lastPlayerPassed, lastPlayerPlayed);
				if (playerReceiving == -1)
					flag = false;
				else
				{
					for (int i = 0; i < gameState.players.Length; i++)
					{
						if (gameState.players[i].hasObject(OE.gameObject))
						{
							//Debug.LogFormat("PLAYER THAT HAS OBJECT: {0}", i);
							//Debug.LogFormat("PLAYER WHO NEED TO RECEIVE THE OBJECT: {0}", playerReceiving);

							if (i == playerReceiving)
							{
								flag = false;
								break;
							}

						}
					}
				}
			}
			if (!flag)
				return false;
		}
		return flag && trigger.HasTriggered(gameState);
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
