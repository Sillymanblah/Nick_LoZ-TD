using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ClubMoblin : EnemyUnit
{
    [SerializeField] List<Unit> unitsInRange = new List<Unit>();

    [SerializeField] int abilityCooldown;
                     float currentCooldown;
    AudioSource audioSource;
    [SerializeField] AudioClip attackingClip;
    bool beingAttacked;
    bool usedAbility;

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        if (!isServer) return;

        StartCoroutine(SmashAbility());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        currentCooldown += Time.deltaTime;
    }

    

    void GotAttacked()
    {
        if (healthPoints > 0.1 * maxHealthPoints)
        {
            abilityCooldown = 20;
        }
        else if (healthPoints < 0.1 * maxHealthPoints)
        {
            abilityCooldown = 15;
        }
        
        if (!beingAttacked)
        {
            beingAttacked = true;
            currentCooldown = 0;
            StartCoroutine(AbilityAlgorithm());
        }

    }

    IEnumerator AbilityAlgorithm()
    {
        float oldHealth = healthPoints;

        while (true)
        {
            if (healthPoints <= (oldHealth - (0.2 * maxHealthPoints)))
            {
                StartCoroutine(SmashAbility());
            }
        }
    }

    IEnumerator SmashAbility()
    {
        float originalSpeed = speed;
        speed = 0;
        animManager.AttackingAnim(0.1f);

        yield return new WaitForSeconds(2.6f);

        for (int i = 0; i < unitsInRange.Count; i++)
        {
            if (unitsInRange[i] == null)
            {
                unitsInRange.RemoveAt(i);
                continue;
            }

            StartCoroutine(unitsInRange[i].StunnedEffect(10));
        }

        yield return new WaitForSeconds(1);

        speed = originalSpeed;
    }

    [Server]
    public override void DealDamage(float points)
    {
        healthPoints -= Mathf.CeilToInt(points);

        if (healthPoints <= 0)
        {
            isDead = true;

            foreach (NetworkIdentity player in CSNetworkManager.instance.players)
            {
                player.GetComponent<PlayerUnitManager>().SetMoney(dropMoney);
            }
            
            WaveManager.instance.EnemyKilled();
            NetworkServer.Destroy(gameObject); 
        }
    }

    public void PlayAttackSound()
    {
        audioSource.PlayOneShot(attackingClip, 2f);
    }

    /// <summary>
    /// OnTriggerEnter is called when the Collider other enters the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Unit"))
        {
            unitsInRange.Add(other.GetComponent<Unit>());
        }
    }

    /// <summary>
    /// OnTriggerExit is called when the Collider other has stopped touching the trigger.
    /// </summary>
    /// <param name="other">The other Collider involved in this collision.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Unit"))
        {
            unitsInRange.Add(other.GetComponent<Unit>());
        }
    }
}
