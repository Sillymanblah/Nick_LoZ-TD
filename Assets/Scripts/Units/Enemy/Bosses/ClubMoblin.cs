using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClubMoblin : EnemyUnit
{
    [SerializeField] List<Unit> unitsInRange = new List<Unit>();

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        StartCoroutine(SmashAbility());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    IEnumerator SmashAbility()
    {
        while (true)
        {
            float originalSpeed = speed;

            yield return new WaitForSeconds(20);

            speed = 0;

            yield return new WaitForSeconds(1);

            Debug.Log($"just stunned lel");

            for (int i = 0; i < unitsInRange.Count; i++)
            {
                if (unitsInRange[i] == null) unitsInRange.RemoveAt(i);

                StartCoroutine(unitsInRange[i].StunnedEffect(10));
            }

            yield return new WaitForSeconds(1);

            speed = originalSpeed;
        }
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
