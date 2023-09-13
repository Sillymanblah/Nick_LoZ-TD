using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SplashWeapon : MonoBehaviour
{
    [SerializeField] SplashUnit thisUnit;
    SphereCollider rangeCollider;
    List<EnemyUnit> enemyUnitsInRange = new List<EnemyUnit>();

    // Start is called before the first frame update
    void Start()
    {
        rangeCollider = GetComponent<SphereCollider>();
        rangeCollider.radius = thisUnit.splashRange;
    }

    public void SplashAttack(float splashRange, float splashDamage, Vector3 firstEnemyPos, EnemyUnit firstEnemy)
    {
        this.transform.position = firstEnemyPos;

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

            if (enemyUnitsInRange[i] == null) 
            {
                enemyUnitsInRange.RemoveAt(i);
            }
            
        }

        foreach (EnemyUnit unit in enemyUnitsInRange)
        {
            
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyUnitsInRange.Add(other.GetComponent<EnemyUnit>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyUnitsInRange.Remove(other.GetComponent<EnemyUnit>());
        }
    }
}
