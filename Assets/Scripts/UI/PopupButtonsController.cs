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

    void OnEnable()
    {
        EventManager.PopupButtonsCalled += ShowButtons;
        //EventManager.PopupButtonClicked += ButtonClick;
        //EventManager.PopupButtonsExpelled += HideButtons;
    }

    void OnDisable()
    {
        EventManager.PopupButtonsCalled -= ShowButtons;
        //EventManager.PopupButtonClicked -= ButtonClick;
        //EventManager.PopupButtonsExpelled -= HideButtons;
    }

    void Start()
    {
        Angle *= Mathf.Deg2Rad;
    }

    void ShowButtons(Vector2 origin, Action[] actions)
    {
        if (ButtonsShowed)
        {
            //Buttons.ForEach(Destroy);
            foreach (PopupButton pb in Buttons.Keys)
                Destroy(pb.gameObject);
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
            PopupButton button = (Instantiate(Button, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<PopupButton>();
            button.Controller = this;
            button.GetComponent<Image>().sprite = actions[i].Sprite;
            button.transform.SetParent(CameraCanvas);
            button.transform.localScale = Vector3.one;
            float angle = radAngle * (i + 1);
            button.transform.position = origin;
            StartCoroutine(MoveHelper.Fly(button.gameObject, new Vector2(origin.x + axis.x * Mathf.Cos(angle) - axis.y * Mathf.Sin(angle), origin.y + axis.x * Mathf.Sin(angle) + axis.y * Mathf.Cos(angle)), FlyTime));
            button.GetComponent<Fader>().FadeIn(FadeInTime);
            Buttons.Add(button, actions[i]);
            radAngle *= -1;
        }
    }

    void HideButtons()
    {
        ButtonsShowed = false;
        //Buttons.ForEach(b => Destroy(b.gameObject));
        foreach (PopupButton pb in Buttons.Keys)
            Destroy(pb.gameObject);
        Buttons.Clear();
    }

    public void ButtonClick(PopupButton btn)
    {
        EventManager.OnActionChoose(Buttons[btn]);
        Buttons.Remove(btn);
        //Buttons.ForEach(b => Destroy(b.gameObject));
        foreach (PopupButton pb in Buttons.Keys)
            Destroy(pb.gameObject);
        Buttons.Clear();
        btn.GetComponent<Button>().transition = UnityEngine.UI.Selectable.Transition.None;///
        btn.GetComponent<Button>().interactable = false;///
        btn.GetComponent<Fader>().FadeAndDestroyObject(0.3f);
        ButtonsShowed = false;
    }
}
