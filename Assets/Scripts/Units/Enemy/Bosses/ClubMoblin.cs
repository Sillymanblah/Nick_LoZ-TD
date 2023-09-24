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

    IEnumerator SmashAbility()
    {
        while (true)
        {
            float originalSpeed = speed;

            yield return new WaitForSeconds(abilityCooldown);

            if (healthPoints > 0.1 * maxHealthPoints)
            {
                float newHP = healthPoints - (0.20f * maxHealthPoints);

                while (healthPoints > newHP)
                {
                    yield return null;
                }
            }

            else if (healthPoints < 0.1 * maxHealthPoints)
            {

            }

            

            speed = 0;

            animManager.AttackingAnim(0.1f);


            yield return new WaitForSeconds(2.6f);

            StunAttack();

            yield return new WaitForSeconds(1);

            speed = originalSpeed;
        }
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
        if (!usedAbility)
        {
            yield return new WaitForSeconds(5.0f);
        }

        while (true)
        {
            currentCooldown
        }
    }

    [Server]
    void StunAttack()
    {
        for (int i = 0; i < unitsInRange.Count; i++)
        {
            if (unitsInRange[i] == null)
            {
                unitsInRange.RemoveAt(i);
                continue;
            }

            StartCoroutine(unitsInRange[i].StunnedEffect(10));
        }
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
