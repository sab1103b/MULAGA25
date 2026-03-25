using UnityEngine;
using System.Collections;

public class CoverSystem : MonoBehaviour
{
    public Transform[] covers; // las 4 coberturas

    public float raisedHeight = 1.5f;
    public float loweredHeight = -2f;

    public float moveSpeed = 3f;

    public float activeTime = 15f;
    public float cooldownTime = 5f;

    void Start()
    {
        StartCoroutine(CoverLoop());
    }

    IEnumerator CoverLoop()
    {
        while (true)
        {
            int first = Random.Range(0, covers.Length);
            int second;

            do
            {
                second = Random.Range(0, covers.Length);
            }
            while (second == first);

            yield return StartCoroutine(RaiseCover(covers[first]));
            yield return StartCoroutine(RaiseCover(covers[second]));

            yield return new WaitForSeconds(activeTime);

            yield return StartCoroutine(LowerCover(covers[first]));
            yield return StartCoroutine(LowerCover(covers[second]));

            yield return new WaitForSeconds(cooldownTime);
        }
    }

    IEnumerator RaiseCover(Transform cover)
    {
        while (cover.position.y < raisedHeight)
        {
            cover.position += Vector3.up * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator LowerCover(Transform cover)
    {
        while (cover.position.y > loweredHeight)
        {
            cover.position += Vector3.down * moveSpeed * Time.deltaTime;
            yield return null;
        }
    }
}