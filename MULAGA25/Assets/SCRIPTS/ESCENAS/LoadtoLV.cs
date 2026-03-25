using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class LoadtoLV : MonoBehaviour
{
    public FadeController fade;
    void Start()
    {
        StartCoroutine(LoadLevel());
    }

    IEnumerator LoadLevel()
    {
        yield return new WaitForSeconds(10f);
        StartCoroutine(fade.FadeOut("Level_01"));
    }
}
