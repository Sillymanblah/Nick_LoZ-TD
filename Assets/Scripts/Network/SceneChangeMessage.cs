using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public struct SceneChangeMessage : NetworkMessage
{
    public string sceneName;
}
