using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public PlayerUnitManager player;
    [SerializeField] TextMeshProUGUI moneyText;
    [SerializeField] Transform unitLoadout;
    [SerializeField] TextMeshProUGUI exceptionText;
    public static UIManager instance;
    public bool MouseOnUI;

    [Space]
    [SerializeField] TextMeshProUGUI readyCount;
    [SerializeField] GameObject readyButton;

    [Space]
    [SerializeField] TextMeshProUGUI skipWaveCount;
    [SerializeField] GameObject skipWaveButton;

    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform slot in unitLoadout)
        {
            if (slot.name == "SlotNums") { break; }
            slot.transform.GetChild(1).GetComponent<Text>().text = string.Empty;
        }

        skipWaveButton.SetActive(false);
        exceptionText.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        // So if the instance is null, we wont get errors until server starts/client joins
        if (GameManager.instance == null) return;

        if (player == null) return;

        if (!player.isLocalPlayer) return;

        MoneyText();
    }



    void MoneyText()
    {
        moneyText.text = player.money.ToString();
    }

    public void SlotLoadout()
    {
        for (int i = 0; i < player.unitsLoadout.Count; i++)
        {
            if (player.unitsLoadout[i] == null)
            {
                Debug.Log($"oh brother");
                unitLoadout.GetChild(i).Find("Cost").GetComponent<Text>().text = string.Empty;
                unitLoadout.GetChild(i).Find("Count").GetComponent<Text>().text = string.Empty;
                continue;
            }

            Image image = unitLoadout.GetChild(i).Find("UnitIcon").GetComponent<Image>();
            Text cost = unitLoadout.GetChild(i).Find("Cost").GetComponent<Text>();
            Text count = unitLoadout.GetChild(i).Find("Count").GetComponent<Text>();
            
            count.text = "0";
            image.enabled = true;
            image.sprite = player.unitsLoadout[i]?.icon;
            cost.text = "$" + player.unitsLoadout[i]?.NextCost(1);
        }
    }
    public void SlotUnitCount(int index)
    {
        Text count = unitLoadout.GetChild(index).Find("Count").GetComponent<Text>();
        count.text = player.loadoutCount[index].ToString();
    }

    public void UIExceptionMessage(string message)
    {
        StopCoroutine("StartExceptionAnimation");
        exceptionText.text = message;
        
        StartCoroutine("StartExceptionAnimation");
    }

    IEnumerator StartExceptionAnimation()
    {
        exceptionText.color = new Color(1,1,1,1);
        yield return new WaitForSeconds(1.0f);
        bool active = false;
        var alphaColor = 1f;
        
        while(active == false)
        {
            exceptionText.color = new Color(1, 1, 1, alphaColor -= Time.deltaTime);
            if (alphaColor <= 0)
                active = true;

            yield return null;
        }
    }

    public void UpdateReadyButton(int playerReadyCount, int maxPlayerCount)
    {
        readyCount.text = playerReadyCount + "/" + maxPlayerCount;
    }

    // THIS IS FOR THE READY UP BUTTON AT BEGINNING OF GAME
    public void ReadyUpButton()
    {
        player.ReadyUp();
    }

    public void UpdateSkipWaveButton(int playerReadyCount, int maxPlayerCount)
    {
        skipWaveCount.text = playerReadyCount + "/" + maxPlayerCount;
    }

    public void SkipWaveButton()
    {
        player.SkipWaveReady();
    }

    public void DisableReadyButtonLocally()
    {
        readyButton.SetActive(false);
    }

    public void ToggleSkipButtonLocally(bool on)
    {
        skipWaveButton.SetActive(on);
    }

    public void BuyUnitButton(int index)
    {
        player.BuyUnit(index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        MouseOnUI = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        MouseOnUI = true;
    }
}
