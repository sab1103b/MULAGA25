using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 2f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeOut(string sceneName)
    {
        Color color = fadeImage.color;

        while (color.a < 1)
        {
            color.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = color;
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    public IEnumerator FadeIn()
    {
        Color color = fadeImage.color;

        while (color.a > 0)
        {
            color.a -= Time.deltaTime * fadeSpeed;
            fadeImage.color = color;
            yield return null;
        }
    }
}