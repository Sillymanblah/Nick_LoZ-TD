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
    protected CharacterController controller;

    [SyncVar]
    protected Unit thisUnit;
    
    int targetWaypoint;
    float speed = 1;
    protected bool isDead;

    Transform target;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        if (!isServer) return;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (!isServer) return;

        if (isDead) return;

        GravityControl();
        WayPointControl();
    }

    [Server]
    public void Initialize(int waypointIndex, Unit unit)
    {
        thisUnit = unit;
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
            ServerEndPathDestroy();
            return;
        }

        targetWaypoint--;
        target = WayPointsManager.instance.points[targetWaypoint];

        var lookAtWaypoint = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.LookAt(lookAtWaypoint, Vector3.up);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isClientOnly) return;
        if (isDead) return;

        if (other.CompareTag("Enemy"))
        {
            ServerInteraction(other);
        }
    }

    [Server]
    protected virtual void ServerInteraction(Collider other)
    {
        Debug.Log($"ServerSide Summonable Interaction");
    }

    [Server]
    protected virtual void ServerEnemyInteract()
    {
        Debug.Log($"ServerSide Summonable Enemy Interaction");
    }

    [Client]
    protected virtual void ClientEnemyInteract()
    {
        Debug.Log($"ClientSide Summonable Enemy Interaction");
    }

    [ClientRpc]
    protected virtual void ClientRpcEnemyInteract()
    {
        Debug.Log($"ClientSide Summonable Enemy Interaction");
    }

    protected virtual void ServerEndPathDestroy()
    {
        Debug.Log($"Destroyed this summonable");
    }
}
