using UnityEngine;
using UnityEngine.UI;

public class FlyingText : MonoBehaviour
{
    [SerializeField]
    GameObject Text;
    [SerializeField]
    float Offset;
    [SerializeField]
    float FlyHeight;
    [SerializeField]
    float FlyTime;
    [SerializeField]
    float FadeTime;

    [SerializeField]
    Transform CameraCanvas;

    void OnEnable()
    {
        EventManager.CreatureHit += ShowDamageText;
        EventManager.AttackMissed += ShowMissText;
    }

    void OnDisable()
    {
        EventManager.CreatureHit -= ShowDamageText;
        EventManager.AttackMissed -= ShowMissText;
    }

    void ShowDamageText(LivingBeing obj, byte damage)
    {
        GameObject textObj = Instantiate(Text);
        textObj.GetComponent<Text>().text = damage.ToString();
        textObj.GetComponent<Text>().color = new Color(1, 1 - damage / obj.MaxHealth, 0);
        textObj.transform.SetParent(CameraCanvas);
        textObj.transform.localScale = Vector3.one;
        textObj.transform.position = new Vector2(obj.transform.position.x, obj.transform.position.y + Offset);
        StartCoroutine(MoveHelper.Fly(textObj, new Vector2(obj.transform.position.x, obj.transform.position.y + FlyHeight), FlyTime));
        textObj.AddComponent<Fader>().FadeAndDestroyObject(FadeTime);
    }

    void ShowMissText(Vector2 pos)
    {
        GameObject textObj = Instantiate(Text);
        textObj.GetComponent<Text>().text = "Miss";
        textObj.GetComponent<Text>().color = Color.red;
        textObj.transform.SetParent(CameraCanvas);
        textObj.transform.localScale = Vector3.one;
        textObj.transform.position = new Vector2(pos.x, pos.y + Offset);
        StartCoroutine(MoveHelper.Fly(textObj, new Vector2(pos.x, pos.y + FlyHeight), FlyTime));
        textObj.AddComponent<Fader>().FadeAndDestroyObject(FadeTime);
    }
}