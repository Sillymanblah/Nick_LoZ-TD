using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class WayPointsManager : NetworkBehaviour
{
    public Transform[] points;
    public static WayPointsManager instance;

    public override void OnStartServer()
    {
        base.OnStartServer();

        instance = this;

        points = new Transform[transform.childCount];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        instance = this;

        points = new Transform[transform.childCount];

        for (int i = 0; i < points.Length; i++)
        {
            points[i] = transform.GetChild(i);
        }
    }

    // return 0 = false
    // return 1 = true with damaging base
    // return 2 = true without damaging base
    public virtual bool CheckForEnemyPosition(int index)
    {
        if (index >= points.Length - 1)
        {
            return true;
        }

        else return false;
    }
}
