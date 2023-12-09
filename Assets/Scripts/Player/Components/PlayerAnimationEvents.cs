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

    public void PlayFootStep()
    {
        player.PlayFootStep();
    }

    public void PlayJumpSound()
    {
        player.PlayJumpSound();
    }
}
