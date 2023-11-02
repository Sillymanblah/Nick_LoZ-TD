using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGridCellLocator : MonoBehaviour
{
    List<GridCell> pathwayCells = new List<GridCell>();

    public bool CheckForPathwayGrid() { return pathwayCells.Count > 0; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GridCell"))
        {
            var thisCell = other.GetComponent<GridCell>();

            if (thisCell.gridType == GridType.pathway)
                pathwayCells.Add(thisCell);
        }
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GridCell"))
        {
            var thisCell = other.GetComponent<GridCell>();

            if (thisCell.gridType == GridType.pathway)
                pathwayCells.Remove(thisCell);
        }
    }

}
