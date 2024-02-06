using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/Config", order = 1)]
public class Config : ScriptableObject
{
    public Color ActiveColor;
    public Color InactiveColor;
    public Color DisabledColor;
    public float roundTotalTime;
    public float minTurnTime;
    public float maxTurnTime;
}
