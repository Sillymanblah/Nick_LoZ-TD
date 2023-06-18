using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerSlotUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerName;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
