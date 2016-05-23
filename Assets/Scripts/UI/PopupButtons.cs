using UnityEngine;
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

    void Start()
    {
        CameraCanvas = GameObject.Find("CameraCanvas");
    }

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
            ButtonsShowed = true;

            Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            Vector2 screenCenter = new Vector2(Screen.width >> 1, Screen.height >> 1);

            Vector2 axis = -(screenCenter - screenPos).normalized * Radius;

            Vector2 check = (Vector2)Camera.main.WorldToScreenPoint(axis) + screenPos;
            if (check.x < 0 || check.x > Screen.width || check.y < 0 || check.y > Screen.height)
                axis = -axis;

            float radAngle = Angle;
            for (byte i = 0; i < 4; ++i)///TODO
            {
                Buttons.Add(Instantiate(Button, Vector3.zero, Quaternion.identity) as GameObject);
                Buttons[i].GetComponent<ActionButton>().Entity = gameObject.GetComponent<Entity>();
                Buttons[i].transform.SetParent(CameraCanvas.transform);
                Buttons[i].transform.localScale = Vector3.one;
                float angle = radAngle * (i + 1);
                Buttons[i].transform.position = transform.position;
                StartCoroutine(MoveHelper.Fly(Buttons[i], new Vector2(transform.position.x + axis.x * Mathf.Cos(angle) - axis.y * Mathf.Sin(angle), transform.position.y + axis.x * Mathf.Sin(angle) + axis.y * Mathf.Cos(angle)), FlyTime));
                Buttons[i].GetComponent<Fader>().FadeIn(FadeInTime);
                radAngle *= -1;
            }
        }
    }

    public void ButtonClick(GameObject btn)
    {
        Buttons.Remove(btn);
        Buttons.ForEach(b => Destroy(b));
        Buttons.Clear();
        btn.GetComponent<UnityEngine.UI.Button>().transition = UnityEngine.UI.Selectable.Transition.None;///
        btn.GetComponent<UnityEngine.UI.Button>().interactable = false;///
        btn.GetComponent<Fader>().FadeAndDestroyObject(0.3f);
        ButtonsShowed = false;
    }
}
