using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Server

    public void SetPlayerListUI(string name, int index)
    {
        transform.GetChild(index).GetComponent<PlayerSlotUI>().AssignPlayer(name);
    }

    #endregion
}
