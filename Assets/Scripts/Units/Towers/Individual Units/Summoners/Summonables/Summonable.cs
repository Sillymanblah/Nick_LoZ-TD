using System.Collections;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Xml.XPath;
using kcp2k;
using Mirror;
using UnityEngine;

public class Summonable : NetworkBehaviour
{
    Vector3 velocity = Vector3.zero;
    float gravity = -9.81f;
    [SerializeField] bool isGrounded = false;
    CharacterController controller;
    [SerializeField] ParticleSystem thisParticleSystem;
    int targetWaypoint;
    float speed = 1;

    Transform target;

    // Start is called before the first frame update
    void Start()
    {
        if (!isServer) return;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer) return;

        GravityControl();
        WayPointControl();
    }

    [Server]
    public void Initialize(int waypointIndex)
    {
        targetWaypoint = waypointIndex;

        controller = GetComponent<CharacterController>();

        target = WayPointsManager.instance.points[targetWaypoint];
        var lookAtWaypoint = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.LookAt(lookAtWaypoint, Vector3.up);
    }

    [Server]
    void GravityControl()
    {
        velocity.y += Mathf.Clamp(gravity * Time.deltaTime, -20, 20);

        controller.Move(velocity * Time.deltaTime);

        if (!isGrounded) { return; }

        if (velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    [Server]
    void WayPointControl()
    {
        Vector3 thisTarget = new Vector3(target.position.x, transform.position.y, target.position.z);

        Vector3 direction = thisTarget - transform.position;
        controller.Move(direction.normalized * speed * Time.deltaTime);

        Vector3 targetDistance = new Vector3(target.position.x, 0, target.position.z);
        Vector3 positionDistance = new Vector3(transform.position.x, 0, transform.position.z);

        if (Vector3.Distance(targetDistance, positionDistance) <= 0.05f)
        {
            GetNextWayPoint();
        }
    }

    [Server]
    void GetNextWayPoint()
    {
        if (targetWaypoint == 0)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }

        targetWaypoint--;
        target = WayPointsManager.instance.points[targetWaypoint];

        var lookAtWaypoint = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.LookAt(lookAtWaypoint, Vector3.up);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        if (other.CompareTag("Enemy"))
        {
            MyMainGoalIsToBlowUp();
        }
    }

    protected virtual void MyMainGoalIsToBlowUp()
    {
        Debug.Log($"My main goal is to blow up 🤯");
        StartCoroutine(ExplosionAndDelay());
    }

    // this isnt multiplayer logic, only for demonstration purposes
    // please work on it and make sure THIS IS NOT IT
    IEnumerator ExplosionAndDelay()
    {
        thisParticleSystem.Play();
        transform.GetChild(0).gameObject.SetActive(false);
        yield return new WaitForSeconds(2.0f);
        NetworkServer.Destroy(this.gameObject);
    }
}
