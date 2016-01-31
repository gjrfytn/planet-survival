using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PopupButtons : MonoBehaviour
{
    public GameObject Button;

    const float Radius = 1;
    const float Angle = 15 * Mathf.Deg2Rad;
    const float FadeInTime = 0.7f;
    const float FlyTime = 0.5f;

    GameObject CameraCanvas;
    List<GameObject> Buttons = new List<GameObject>();

    bool ButtonsShowed;

    void OnMouseUpAsButton()
    {
        if (ButtonsShowed)
        {
            ButtonsShowed = false;
            Buttons.ForEach(b => Destroy(b));
            Buttons.Clear();
        }
        else
        {
            CameraCanvas = GameObject.Find("CameraCanvas");
            ButtonsShowed = true;

            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

            Vector2 axis = -(screenCenter - screenPos).normalized * Radius;

            Vector2 check = (Vector2)Camera.main.WorldToScreenPoint(axis) + screenPos;
            if (check.x < 0 || check.x > Screen.width || check.y < 0 || check.y > Screen.height)
                axis = -axis;

            float radAngle = Angle;
            for (byte i = 0; i < 4; ++i)///TODO
            {
                Buttons.Add(Instantiate(Button, new Vector3(), Quaternion.identity) as GameObject);
                Buttons[i].GetComponent<ActionButton>().Entity = gameObject;
                Buttons[i].transform.SetParent(CameraCanvas.transform);
                Buttons[i].transform.localScale = Vector3.one;
                float angle = radAngle * ((i / 2) * 2 + 1);
                StartCoroutine(Fly(Buttons[i], transform.position, new Vector2(transform.position.x + axis.x * Mathf.Cos(angle) - axis.y * Mathf.Sin(angle), transform.position.y + axis.x * Mathf.Sin(angle) + axis.y * Mathf.Cos(angle))));
                StartCoroutine(RenderHelper.FadeIn(Buttons[i].GetComponent<CanvasRenderer>(), FadeInTime));
                radAngle *= -1;
            }
        }
    }

    IEnumerator Fly(GameObject obj, Vector2 from, Vector2 to)
    {
        float flyTime = FlyTime;
        obj.transform.position = from;
        while (flyTime > 0 && obj != null)
        {
            float tstep = flyTime / Time.deltaTime;
            flyTime -= Time.deltaTime;
            float dstep = Vector2.Distance(obj.transform.position, to) / tstep;
            obj.transform.position = Vector2.MoveTowards(obj.transform.position, to, dstep);
            yield return null;
        }
    }

    public void ButtonClick(GameObject btn)
    {
        Buttons.Remove(btn);
        Buttons.ForEach(b => Destroy(b));
        Buttons.Clear();
        StartCoroutine(RenderHelper.FadeAndDestroyObject(btn.GetComponent<CanvasRenderer>(), 0.3f));
        ButtonsShowed = false;
    }
}
