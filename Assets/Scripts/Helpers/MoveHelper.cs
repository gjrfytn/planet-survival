using UnityEngine;
using System.Collections;

public static class MoveHelper
{
	public static IEnumerator Fly(GameObject obj, Vector2 from, Vector2 to, float time)
	{
		obj.transform.position = from;
		while (time > 0 && obj != null)
		{
			float tstep = time / Time.deltaTime;
			time -= Time.deltaTime;
			float dstep = Vector2.Distance(obj.transform.position, to) / tstep;
			obj.transform.position = Vector2.MoveTowards(obj.transform.position, to, dstep);
			yield return null;
		}
	}
}
