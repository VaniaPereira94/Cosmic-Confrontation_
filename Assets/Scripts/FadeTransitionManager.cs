using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransitionController : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private float _fadeDuration = 3f;

    IEnumerator FadeIn()
    {
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, timer / _fadeDuration);

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);

            yield return null;

            timer += Time.deltaTime;
        }

        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1f);
    }

    IEnumerator FadeOut()
    {
        float timer = 0f;

        while (timer < _fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / _fadeDuration);

            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, alpha);

            yield return null;

            timer += Time.deltaTime;
        }

        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0f);
    }

    public void PlayFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    public void PlayFadeOut()
    {
        StartCoroutine(FadeOut());
    }
}