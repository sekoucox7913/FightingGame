using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    [SerializeField]
    private Image fadeImage = null;

    [SerializeField]
    private Color startColor = default;
    [SerializeField]
    private Color finishColor = default;

    private float fadePercent = 0f;

    public void Fade()
    {
        fadeImage.enabled = true;
        StartCoroutine(ColourChanging());
    }

    IEnumerator ColourChanging()
    {
        while (fadePercent < 1f)
        {
            fadePercent += Time.deltaTime;

            fadeImage.color = Color.Lerp(startColor, finishColor, fadePercent);
            yield return null;
        }
        fadeImage.enabled = false;
    }
}
