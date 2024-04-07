using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SplashWeapon : MonoBehaviour
{
    [SerializeField] SplashUnit thisUnit;
    [SerializeField] LayerMask collidersLayer;

    public void SplashAttack(float splashDamage, float splashRange, Vector3 firstEnemyPos, EnemyUnit firstEnemy)
    {
        this.transform.position = firstEnemyPos;

        List<EnemyUnit> enemyUnitsInRange = new List<EnemyUnit>();

        foreach (Collider collider in Physics.OverlapSphere(this.transform.position, splashRange, collidersLayer))
        {
            enemyUnitsInRange.Add(collider.GetComponent<EnemyUnit>());
        }

        for (int i = 0; i < enemyUnitsInRange.Count; i++)
        {
            if (enemyUnitsInRange[i] == null) 
            {
                enemyUnitsInRange.RemoveAt(i);
                i--;
                continue;
            }

            if (enemyUnitsInRange[i] == firstEnemy) continue;

            enemyUnitsInRange[i].DealDamage(splashDamage);
            enemyUnitsInRange[i].DamageUIText(splashDamage);


            if (enemyUnitsInRange[i] == null) 
            {
                enemyUnitsInRange.RemoveAt(i);
            }
            
        }
    }

    /// <summary>
    /// Callback to draw gizmos that are pickable and always drawn.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(this.transform.position, thisUnit.splashRange);
    }
}
