using System.Collections.Generic;
using UnityEngine;

public class GridCellLocator : MonoBehaviour
{
    Unit thisUnit;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        thisUnit = transform.parent.GetComponent<Unit>();
        
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (thisUnit.isServer) return;

        if (other.gameObject.CompareTag("GridCell"))
        {
            thisUnit.gridCells.Add(other.GetComponent<GridCell>().listIndex);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (thisUnit.isServer) return;

        if (other.gameObject.CompareTag("GridCell"))
        {
            thisUnit.gridCells.Remove(other.GetComponent<GridCell>().listIndex);
        }
    }
}
