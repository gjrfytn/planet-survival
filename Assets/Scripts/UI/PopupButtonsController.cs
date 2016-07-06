using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class PopupButtonsController : MonoBehaviour
{
    [SerializeField]
    GameObject Button;

    [SerializeField]
    float Radius;
    [SerializeField]
    float Angle;
    [SerializeField]
    float FadeInTime;
    [SerializeField]
    float FlyTime;

    [SerializeField]
    Transform CameraCanvas;

    Dictionary<PopupButton, Action> Buttons = new Dictionary<PopupButton, Action>();
    bool ButtonsShowed;
    bool Modal;

    void OnEnable()
    {
        EventManager.PopupButtonsCalled += ShowButtons;
    }

    void OnDisable()
    {
        EventManager.PopupButtonsCalled -= ShowButtons;
    }

    void Start()
    {
        Angle *= Mathf.Deg2Rad;
    }

    void ShowButtons(Vector2 origin, Action[] actions, bool modal)
    {
        if (ButtonsShowed)
        {
            if (Modal)
                return;

            foreach (PopupButton pb in Buttons.Keys)
                Destroy(pb.gameObject);
            Buttons.Clear();
        }
        Modal = modal;
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
            PopupButton button = (Instantiate(Button, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<PopupButton>();
            button.Controller = this;
            button.GetComponent<Image>().sprite = actions[i].Sprite;
            button.transform.SetParent(CameraCanvas);
            button.transform.localScale = Vector3.one;
            if (actions[i].AdditionalSprite != null)
            {
                GameObject additionalSprite = new GameObject("AdditionalSprite");
                additionalSprite.AddComponent<Image>().sprite = actions[i].AdditionalSprite;
                additionalSprite.GetComponent<Image>().rectTransform.sizeDelta = button.GetComponent<Image>().rectTransform.sizeDelta;
                additionalSprite.transform.SetParent(button.transform, false);
            }
            float angle = radAngle * (i + 1);
            button.transform.position = origin;
            StartCoroutine(MoveHelper.Fly(button.gameObject, new Vector2(origin.x + axis.x * Mathf.Cos(angle) - axis.y * Mathf.Sin(angle), origin.y + axis.x * Mathf.Sin(angle) + axis.y * Mathf.Cos(angle)), FlyTime));
            button.GetComponent<Fader>().FadeIn(FadeInTime);
            Buttons.Add(button, actions[i]);
            radAngle *= -1;
        }
    }

    public void ButtonClick(PopupButton btn)
    {
        Action buf = Buttons[btn];
        Buttons.Remove(btn);
        foreach (PopupButton pb in Buttons.Keys)
            Destroy(pb.gameObject);
        Buttons.Clear();
        btn.GetComponent<Button>().transition = UnityEngine.UI.Selectable.Transition.None;///
        btn.GetComponent<Button>().interactable = false;///
        btn.GetComponent<Fader>().FadeAndDestroyObject(0.3f);
        ButtonsShowed = false;

        EventManager.OnActionChoose(buf);
    }
}
