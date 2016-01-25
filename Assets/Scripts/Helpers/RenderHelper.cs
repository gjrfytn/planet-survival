using UnityEngine;
using System.Collections;

public static class RenderHelper
{
	/// <summary>
	/// Постепенно отображает объект (корутина).
	/// </summary>
	/// <returns>(Корутина).</returns>
	/// <param name="renderer">Renderer объекта.</param>
	/// <param name="time">Время появления.</param>
	public static IEnumerator FadeIn(Renderer renderer,float time)
	{
		Color cbuf = renderer.material.color;
		cbuf.a = 0;
		renderer.material.color = cbuf;
		do
		{
			Color buf = renderer.material.color;
			buf.a += Time.deltaTime/time;
			renderer.material.color = buf;
 			yield return null;
		}
		while (renderer != null && renderer.material.color.a < 1);
	}

	/// <summary>
	/// Постепенно отображает объект UI (корутина).
	/// </summary>
	/// <returns>(Корутина).</returns>
	/// <param name="renderer">CanvasRenderer объекта.</param>
	/// <param name="time">Время появления.</param>
	public static IEnumerator FadeIn(CanvasRenderer renderer,float time)
	{
		renderer.SetAlpha(0);
		do
		{
			renderer.SetAlpha(renderer.GetAlpha()+Time.deltaTime/time);
			yield return null;
		}
		while (renderer != null && renderer.GetAlpha() < 1);
	}

	/// <summary>
	/// Постепенно скрывает, затем уничтожает объект (корутина).
	/// </summary>
	/// <returns>(Корутина).</returns>
	/// <param name="renderer">Renderer объекта.</param>
	/// <param name="time">Время скрытия.</param>
	public static IEnumerator FadeAndDestroyObject(Renderer renderer, float time)
	{
		//Renderer renderer = obj.GetComponent<Renderer>();
		float a=renderer.material.color.a;

		while (renderer.material.color.a > 0)
		{
			Color buf = renderer.material.color;
			buf.a -= (Time.deltaTime/time)*a;
			renderer.material.color = buf;
			yield return null;
		}
		Object.Destroy(renderer.gameObject);
	}

	/// <summary>
	/// Постепенно скрывает, затем уничтожает объект UI (корутина).
	/// </summary>
	/// <returns>(Корутина).</returns>
	/// <param name="renderer">CanvasRenderer объекта.</param>
	/// <param name="time">Время скрытия.</param>
	public static IEnumerator FadeAndDestroyObject(CanvasRenderer renderer, float time)
	{
		float a=renderer.GetAlpha();
		
		while (renderer.GetAlpha() > 0)
		{
			renderer.SetAlpha(renderer.GetAlpha()-(Time.deltaTime/time)*a);
			yield return null;
		}
		Object.Destroy(renderer.gameObject);
	}
}
