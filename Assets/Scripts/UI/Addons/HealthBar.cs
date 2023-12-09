using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Transform cam;
    Slider healthSlider;
    [SerializeField] Text hpText;
    [SerializeField] bool isWorldSpace;
    EnemyUnit enemy;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        healthSlider = transform.GetChild(1).GetChild(1).GetComponent<Slider>();
        enemy = transform.parent.GetComponent<EnemyUnit>();
        cam = Camera.main.transform;
    }
    // Start is called before the first frame update
    void Start()
    {
        transform.GetChild(1).gameObject.SetActive(false);
    }
    

    // Update is called once per frame
    void Update()
    {
    }

    public void UpdateBarValue(int healthValue)
    {
        healthSlider.value = healthValue;
        hpText.text = FormatNumber((int)healthSlider.value) + "/" +  FormatNumber((int)healthSlider.maxValue);
    }

    public void BarValueOnStart(float maxValue)
    {
        healthSlider.maxValue = maxValue;
        UpdateBarValue((int)maxValue);
    }

    private void LateUpdate() 
    {
        if (isWorldSpace)
            transform.LookAt(transform.position + cam.forward);    
    }

    string FormatNumber(int num) {
    if (num >= 100000)
        return FormatNumber(num / 1000) + "K";

    if (num >= 10000)
        return (num / 1000D).ToString("0.#") + "K";

    return num.ToString("#,0");
    }
}
