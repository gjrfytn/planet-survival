using UnityEngine;

public static class MoveHelper
{
    public static System.Collections.IEnumerator Fly(GameObject obj, Vector2 dest, float time)
    {
        while (time > 0 && obj != null)
        {
            float tstep = time / Time.deltaTime;
            time -= Time.deltaTime;
            float dstep = Vector2.Distance(obj.transform.position, dest) / tstep;
            obj.transform.position = Vector2.MoveTowards(obj.transform.position, dest, dstep);
            yield return null;
        }
    }
}
