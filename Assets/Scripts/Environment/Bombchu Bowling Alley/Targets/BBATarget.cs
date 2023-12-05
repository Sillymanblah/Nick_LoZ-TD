using System.Collections;
using UnityEngine;

public class BBATarget : MonoBehaviour
{
    [SerializeField] AudioClip explosion;
    [SerializeField] AudioSource audioSource;
    [SerializeField] GameObject particlesObject;
    [SerializeField] Transform particlesParent;

    [SerializeField] bool isIndependent;

    public void ExplodeTarget()
    {
        StartCoroutine(TakeDownDelay());
    }

    IEnumerator TakeDownDelay()
    {
        for (int i = 0; i < 7; i++)
        {
            audioSource.PlayOneShot(explosion, 1f);
            
            Vector3 var = RandomVector3(new Vector3 (-0.54f,0.13f,-0.23f), new Vector3(0.54f,0.13f,0.5f));
            GameObject newObject = Instantiate(particlesObject, particlesParent);
            newObject.transform.localPosition = var;

            yield return new WaitForSeconds(0.2f);
        }

        if (isIndependent) yield break;

        Destroy(GetComponent<BoxCollider>());

        float Yposition = this.transform.position.y;

        while (this.transform.position.y > (Yposition - 5))
        {
            transform.Translate(Vector3.down * Time.deltaTime * 3f, Space.World);
            yield return null;
        }
    }

    Vector3 RandomVector3(Vector3 min, Vector3 max)
    {
        return new Vector3(UnityEngine.Random.Range(min.x, max.x), UnityEngine.Random.Range(min.y, max.y), UnityEngine.Random.Range(min.z, max.z));
    }
}


