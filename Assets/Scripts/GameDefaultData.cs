using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "GameDefaultDataInstance", menuName = "ScriptableObjects/GameDefaultDataInstance", order = 1)]
public class GameDefaultData : ScriptableObject
{
    public MatchSetup defaultSetup;

    public Sprite[] balls;

    public PlayerColors[] presets;

    public Court[] courts;
}
