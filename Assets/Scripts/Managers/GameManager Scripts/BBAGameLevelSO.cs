using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BBA Level", menuName ="Levels/BBA Game Level")]
public class BBAGameLevelSO : GameLevelSO
{
    [Space]
    public int waveNumReward;

    public override UnitSO GetRandomUnitReward(int waveNumber, bool win)
    {
        if (waveNumber >= waveNumReward)
        {
            return unitDrops[0].unit;
        } else return null;
    }
}
