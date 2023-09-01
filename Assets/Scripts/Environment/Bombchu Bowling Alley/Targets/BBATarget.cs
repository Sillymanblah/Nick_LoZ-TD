using System.Collections;
using UnityEngine;

public class BBATarget : MonoBehaviour
{
    [SerializeField] AudioClip explosion;
    [SerializeField] AudioSource audioSource;

    [SerializeField] bool isIndependent;

    public void ExplodeTarget()
    {
        StartCoroutine(TakeDownDelay());
    }

    IEnumerator TakeDownDelay()
    {
        for (int i = 0; i < 5; i++)
        {
            audioSource.PlayOneShot(explosion, 1f);
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
}
