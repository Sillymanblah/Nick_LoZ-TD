using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grotto : MonoBehaviour
{
    public static Grotto instance;

    [SerializeField] Transform spawnTransform;
    public Vector3 GetSpawnPosition() { return spawnTransform.position; }


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
