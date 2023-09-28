using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    PlayerAnimationManager player;
    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.GetComponent<PlayerAnimationManager>();
    }

    public void StopEmoting()
    {
        player.StopEmoting();
    }
}
