using UnityEngine;
using System.Collections;

public class Fader : MonoBehaviour
{
    bool UseCanvas = false;
    bool Fading = false;

    void Awake()
    {
        if (GetComponent<Renderer>() == null)
        {
            if (GetComponent<CanvasRenderer>() == null)
                throw new System.NullReferenceException("Fader cannot be used without Renderer or CanvasRenderer component.");
            else
                UseCanvas = true;
        }
    }

    /// <summary>
    /// Постепенно отображает объект.
    /// </summary>
    /// <param name="time">Время появления.</param>
    public void FadeIn(float time)
    {
        if (UseCanvas)
            StartCoroutine(FadeIn(GetComponent<CanvasRenderer>(), time));
        else
            StartCoroutine(FadeIn(GetComponent<Renderer>(), time));
    }

    /// <summary>
    /// Постепенно скрывает, затем уничтожает объект.
    /// </summary>
    /// <param name="time">Время скрытия.</param>
    public void FadeAndDestroyObject(float time)
    {
        Fading = true;
        if (UseCanvas)
            StartCoroutine(FadeAndDestroyObject(GetComponent<CanvasRenderer>(), time));
        else
            StartCoroutine(FadeAndDestroyObject(GetComponent<Renderer>(), time));
    }

    IEnumerator FadeIn(Renderer renderer, float time)
    {
        Color buf = renderer.material.color;
        buf.a = 0;
        renderer.material.color = buf;
        do
        {
            buf.a += Time.deltaTime / time;
            renderer.material.color = buf;
            yield return null;
        }
        while (renderer.material.color.a < 1 && !Fading);
    }

    IEnumerator FadeIn(CanvasRenderer renderer, float time)
    {
        renderer.SetAlpha(0);
        do
        {
            renderer.SetAlpha(renderer.GetAlpha() + Time.deltaTime / time);
            yield return null;
        }
        while (renderer.GetAlpha() < 1 && !Fading);
    }

    IEnumerator FadeAndDestroyObject(Renderer renderer, float time)
    {
        float a = renderer.material.color.a;
        Color buf = renderer.material.color;
        while (renderer.material.color.a > 0)
        {
            buf.a -= (Time.deltaTime / time) * a;
            renderer.material.color = buf;
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator FadeAndDestroyObject(CanvasRenderer renderer, float time)
    {
        float a = renderer.GetAlpha();

        while (renderer.GetAlpha() > 0)
        {
            renderer.SetAlpha(renderer.GetAlpha() - (Time.deltaTime / time) * a);
            yield return null;
        }
        Destroy(gameObject);
    }
}
