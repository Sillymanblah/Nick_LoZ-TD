using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathwayGridCell : GridCell
{
    public int waypointIndex;

    private void OnValidate()
    {
        gridType = GridType.pathway;
    }
}
