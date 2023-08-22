using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName;

    public void AssignPlayer(string player)
    {
        if (player == null)
        {
            playerName.text = string.Empty;
            return;
        }

        playerName.text = player;
    }
}
