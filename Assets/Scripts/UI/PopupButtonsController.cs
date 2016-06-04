using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PopupButtonsController : MonoBehaviour
{
    public GameObject Button;

    const float Radius = 1.5f;
    const float Angle = 15 * Mathf.Deg2Rad;
    const float FadeInTime = 0.7f;
    const float FlyTime = 0.5f;

    Transform CameraCanvas;
    List<PopupButton> Buttons = new List<PopupButton>();

    bool ButtonsShowed;

    void OnEnable()
    {
        EventManager.PopupButtonsCalled += ShowButtons;
        EventManager.PopupButtonClicked += ButtonClick;
        EventManager.PopupButtonsExpelled += HideButtons;
    }

    void OnDisable()
    {
        EventManager.PopupButtonsCalled -= ShowButtons;
        EventManager.PopupButtonClicked -= ButtonClick;
        EventManager.PopupButtonsExpelled -= HideButtons;
    }

    void Start()
    {
        CameraCanvas = GameObject.Find("CameraCanvas").transform;
    }

    void ShowButtons(Vector2 origin, Action[] actions)
    {
        if (ButtonsShowed)
        {
            Buttons.ForEach(Destroy);
            Buttons.Clear();
        }
        ButtonsShowed = true;

        Vector2 screenPos = Camera.main.WorldToScreenPoint(transform.position);
        Vector2 screenCenter = new Vector2(Screen.width >> 1, Screen.height >> 1);

        Vector2 axis = -(screenCenter - screenPos).normalized * Radius;

        Vector2 check = (Vector2)Camera.main.WorldToScreenPoint(axis) + screenPos;
        if (check.x < 0 || check.x > Screen.width || check.y < 0 || check.y > Screen.height)
            axis = -axis;

        float radAngle = Angle;
        for (byte i = 0; i < actions.Length; ++i)
        {
            Buttons.Add((Instantiate(Button, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<PopupButton>());
            Buttons[i].Action = actions[i];
            Buttons[i].GetComponent<Image>().sprite = actions[i].Sprite;
            Buttons[i].transform.SetParent(CameraCanvas);
            Buttons[i].transform.localScale = Vector3.one;
            float angle = radAngle * (i + 1);
            Buttons[i].transform.position = origin;
            StartCoroutine(MoveHelper.Fly(Buttons[i].gameObject, new Vector2(origin.x + axis.x * Mathf.Cos(angle) - axis.y * Mathf.Sin(angle), origin.y + axis.x * Mathf.Sin(angle) + axis.y * Mathf.Cos(angle)), FlyTime));
            Buttons[i].GetComponent<Fader>().FadeIn(FadeInTime);
            radAngle *= -1;
        }
    }

    void HideButtons()
    {
        ButtonsShowed = false;
        Buttons.ForEach(b => Destroy(b.gameObject));
        Buttons.Clear();
    }

    public void ButtonClick(PopupButton btn)
    {
        Buttons.Remove(btn);
        Buttons.ForEach(b => Destroy(b.gameObject));
        Buttons.Clear();
        btn.GetComponent<Button>().transition = UnityEngine.UI.Selectable.Transition.None;///
        btn.GetComponent<Button>().interactable = false;///
        btn.GetComponent<Fader>().FadeAndDestroyObject(0.3f);
        ButtonsShowed = false;
    }
}
