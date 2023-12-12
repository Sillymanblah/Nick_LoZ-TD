using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrottoSpawnPad : MonoBehaviour
{
    bool isNearPad;
    [SerializeField] GameObject uiText;

    // Start is called before the first frame update
    void Start()
    {
        uiText.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isNearPad)
        {
            uiText.transform.LookAt(uiText.transform.position + Camera.main.transform.forward);   

            if (Input.GetKeyDown(KeyCode.E))
            {
                Grotto.instance.CmdTeleportBackToSpawn(Grotto.instance.localPlayer);
                uiText.SetActive(false);
                isNearPad = false;
            }
        }
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPad = true;
            uiText.SetActive(true);
        }
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isNearPad = false;
            uiText.SetActive(false);

        }
    }
}
