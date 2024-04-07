using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ClubMoblin : EnemyUnit
{

    [SerializeField] int abilityCooldown;
                     float currentCooldown;
    AudioSource audioSource;
    [SerializeField] AudioClip attackingClip;
    bool beingAttacked;
    bool usedAbility;
    [SerializeField] LayerMask unitLayer;
    [SerializeField] ParticleSystem shockwaveParticle;
    [SerializeField] Transform clubPos;
    [SerializeField] int shockwaveRange;

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    [Server]
    public override void DealDamage(float points)
    {
        GotAttacked();

        base.DealDamage(points);   
    }

    [Server]
    void GotAttacked()
    {
        if (healthPoints > 0.1 * maxHealthPoints)
        {
            abilityCooldown = 17;
        }
        else if (healthPoints < 0.1 * maxHealthPoints)
        {
            abilityCooldown = 12;
            Debug.Log($"bitch wtf");
        }
        
        if (!beingAttacked)
        {
            StartCoroutine(nameof(BeingAttackedTracker));

            Debug.Log($"got attacked");
            currentCooldown = abilityCooldown;
            StartCoroutine(AbilityAlgorithm());
        }
        else
        {
            StopCoroutine(nameof(BeingAttackedTracker));
            StartCoroutine(nameof(BeingAttackedTracker));
        }

    }

    IEnumerator AbilityAlgorithm()
    {
        float oldHealth = healthPoints;

        while (beingAttacked)
        {
            if (usedAbility)
            {
                yield return null;
                continue;
            }

            if (currentCooldown < abilityCooldown)
            {
                yield return null;
                continue;
            }

            if (healthPoints <= 0.1 * maxHealthPoints)
            {
                StartCoroutine(SmashAbility());
            }

            else if (healthPoints <= (oldHealth - (0.2 * maxHealthPoints)))
            {
                StartCoroutine(SmashAbility());
            }

            yield return null;
        }
    }

    IEnumerator SmashAbility()
    {
        usedAbility = true;

        float originalSpeed = speed;
        speed = 0;
        animManager.AttackingAnim(0.1f);

        yield return new WaitForSeconds(2.6f);

        foreach (Collider collider in Physics.OverlapSphere(clubPos.position, shockwaveRange, unitLayer))
        {
            Unit newUnit = collider.GetComponent<Unit>();
            if (newUnit != null) StartCoroutine(newUnit.StunnedEffect(10));
        }
        
        RpcSmashAbility(shockwaveRange);
        StartCoroutine(CooldownAffect());

        yield return new WaitForSeconds(1);

        usedAbility = false;
        speed = originalSpeed;
    }

    [ClientRpc]
    void RpcSmashAbility(int range)
    {
        shockwaveParticle.transform.position = clubPos.position;
        shockwaveParticle.transform.localScale = Vector3.one * (0.34f * range);
        shockwaveParticle.Play();
    }

    IEnumerator CooldownAffect()
    {
        currentCooldown = 0;

        while (currentCooldown < abilityCooldown)
        {
            currentCooldown += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator BeingAttackedTracker()
    {
        beingAttacked = true;
        float thisTime = 0;

        while (thisTime < 10)
        {
            thisTime += Time.deltaTime;
            yield return null;
        }

        beingAttacked = false;
    }

    public void PlayAttackSound()
    {
        audioSource.PlayOneShot(attackingClip, 2f);
    }
}
