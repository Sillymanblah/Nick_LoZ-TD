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
            PathwayGridCell pathwayGridCell = null;

            if (other.TryGetComponent<PathwayGridCell>(out pathwayGridCell))
            {
                pathwayCells.Add(pathwayGridCell);
            }

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("GridCell"))
        {
            var thisCell = other.GetComponent<GridCell>();

            if (thisCell == pathwayCells.Contains(thisCell))
                pathwayCells.Remove(thisCell);
        }
    }

    public PathwayGridCell GetPathwayGridCell()
    {
        PathwayGridCell pathwayGridCell = null;

        foreach (Collider collider in Physics.OverlapBox(this.transform.position, new Vector3(0.75f, 0.75f, 0.25f)))
        {
            if (collider.CompareTag("GridCell"))
            {
                if (collider.TryGetComponent<PathwayGridCell>(out pathwayGridCell))
                    return pathwayGridCell;
            }
        }
        foreach (Collider collider in Physics.OverlapBox(this.transform.position, new Vector3(0.25f, 0.75f, 0.75f)))
        {
            if (collider.CompareTag("GridCell"))
            {
                if (collider.TryGetComponent<PathwayGridCell>(out pathwayGridCell))
                    return pathwayGridCell;
            }
        }

        return null;
    }
}
