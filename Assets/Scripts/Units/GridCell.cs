using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public enum GridType
{
    ground,
    air,
    disabled,
    pathway
}
public class GridCell : MonoBehaviour
{
    public bool isOccupied { get; private set;}
    public GridType gridType;
    public int listIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetOccupence(bool value)
    {
        isOccupied = value;
    }

    public bool CheckAvailability(GridType unitGridType)
    {
        if (gridType == GridType.disabled) return false;

        if (gridType != unitGridType) return false;

        if (isOccupied) return false;

        return true;
    }
}
