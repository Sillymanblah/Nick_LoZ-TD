using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class AoEUnit : Unit
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    protected override void DealDamageToEnemy()
    {
        for (int i = 0; i < enemiesInRange.Count; i++)
        {
            if (enemiesInRange[i] == null)
            {
                enemiesInRange.RemoveAt(i);
                i--;
            }
            else if (enemiesInRange[i].isDead == true)
            {
                enemiesInRange.RemoveAt(i);
            }

            else
            {
                enemiesInRange[i].DealDamage(attack);
            }
        }
    }

    public override void SelectUnit()
    {
        isSelected = true;
        rangeVisualSprite.gameObject.SetActive(true);
        rangeCollider.radius = range / 5;
        rangeVisualSprite.localScale = new Vector3(1,1) * (rangeCollider.radius * 0.2f);
        UIUnitStats.instance.UpdateUnitStatsAoE(this, true);
    }

    [TargetRpc]
    protected override void UpdateLocalClient(Vector3 newScale, bool activeUI)
    {
        Debug.Log($"is this being called");
        rangeVisualSprite.localScale = newScale;
        UIUnitStats.instance.UpdateUnitStatsAoE(this, activeUI);
    }

}
