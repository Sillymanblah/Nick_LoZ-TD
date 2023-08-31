using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyPlayer : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            MyNetworkMessage message = new MyNetworkMessage();
            message.bruh = 1;
            NetworkClient.Send(message); 
        }
    }
}

public struct MyNetworkMessage : NetworkMessage
{
    public int bruh;
}
