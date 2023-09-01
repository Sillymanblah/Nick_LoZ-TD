using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class BBAWayPointsManager : WayPointsManager
{
    public new static BBAWayPointsManager instance;


    int currentTarget = 0;
    public UnityEvent takeDownTarget1;
    public UnityEvent takeDownTarget2;
    public UnityEvent takeDownTarget3;

    public UnityEvent SERVERtakeDownTarget1;
    public UnityEvent SERVERtakeDownTarget2;
    public UnityEvent SERVERtakeDownTarget3;


    public override void OnStartServer()
    {
        base.OnStartServer();

        instance = this;
    }

    // return 0 = false
    // return 1 = true with damaging base
    // return 2 = true without damaging base
    [Server]
    public new int CheckForEnemyPosition(int index)
    {
        switch(index)
        {
            // final target
            case 4:
                if (currentTarget == 2)
                {
                    RpcTakeDownTarget3();

                    if (!isServerOnly) return 1;

                    SERVERtakeDownTarget3.Invoke();
                }
                return 1;

            case 3:
                if (currentTarget == 1)
                {
                    RpcTakeDownTarget2();

                    if (isServerOnly)
                    {   
                        SERVERtakeDownTarget2.Invoke();
                    }

                    currentTarget++;
                    return 2;
                }
                
                else return 0;
            case 2:
                if (currentTarget == 0)
                {
                    RpcTakeDownTarget1();

                    if (isServerOnly)
                    { 
                        SERVERtakeDownTarget1.Invoke();
                    }

                    currentTarget++;
                    return 2;
                }
                
                else return 0;

            default:
                return 0;
        }
    }

    [ClientRpc]
    private void RpcTakeDownTarget3()
    {
        takeDownTarget3.Invoke();
    }

    [ClientRpc]
    void RpcTakeDownTarget1()
    {
        takeDownTarget1.Invoke();
    }



    [ClientRpc]
    void RpcTakeDownTarget2()
    {
        takeDownTarget2.Invoke();
    }
}
