using Mirror;
using UnityEngine;

public struct ConnectionRefusedMessage : NetworkMessage
{
    public string reason;

    public ConnectionRefusedMessage(string reason)
    {
        this.reason = reason;
    }
}
