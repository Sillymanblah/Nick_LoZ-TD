using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitCollider : MonoBehaviour
{
    Unit unit;
    // Start is called before the first frame update
    void Start()
    {
        unit = transform.parent.GetComponent<Unit>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (unit == null) unit = transform.parent.GetComponent<Unit>();

        if (!unit.isServer) return;

        if (other.CompareTag("Enemy"))
        {
            unit.enemiesInRange.Add(other.GetComponent<EnemyUnit>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!unit.isServer) return;

        if (other.CompareTag("Enemy"))
        {
            unit.enemiesInRange.Remove(other.GetComponent<EnemyUnit>());
        }
    }
}
