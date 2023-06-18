using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyAuth : MonoBehaviour
{
    public new string name;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SyncData(PlayerNetworkInfo player)
    {
        player.SetDisplayName(name);
        Debug.Log(name);
    }

    public void SetName(string newName)
    {
        name = newName;
    }
}
