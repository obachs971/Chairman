using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRound 
{
    public List<ActionInfo> actionsPerformed = new List<ActionInfo>();
    public StartOfRoundInfo startOfRoundInfo;
}
public class StartOfRoundInfo
{
    public int[] startingHandCounts;
    public Material playButtonColor;
    public Color playButtonTextColor;
    public List<GameMainObjInfo> gameObjInfo;
    
}
public class GameMainObjInfo
{
    public GameObjectMain gameObj;
    public Vector3[] gameObjSizes;
    public Vector3[] gameObjPositions;
    public Quaternion[] gameObjRotations;
    public Material[] gameObjMats;
    public GameMainObjInfo(GameObjectMain gameObj, Vector3[] gameObjSizes, Vector3[] gameObjPositions, Quaternion[] gameObjRotations, Material[] gameObjMats)
    {
        this.gameObj = gameObj;
        this.gameObjSizes = gameObjSizes;
        this.gameObjPositions = gameObjPositions;
        this.gameObjRotations = gameObjRotations;
        this.gameObjMats = gameObjMats;
        
    }
}



public class ActionInfo
{
    public GameEffect gameEffect;

    public Vector3 playPileV3;
    public int turnDirection;

    public Effect actionEffect;
    public int playerPerformingAction;

    public Material material;

    public ViolationInfo violationInfo;
    public Phrase phrase1;
    
    public List<int> playerRecievingObj;
}
