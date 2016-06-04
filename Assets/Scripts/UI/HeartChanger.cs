using UnityEngine;
using UnityEngine.UI;

public class HeartChanger : MonoBehaviour
{
    public Sprite FullHP;
    public Sprite ThreeQuatersHP;
    public Sprite HalfHP;
    public Sprite QuaterHP;

    void OnEnable()
    {
        EventManager.CreatureHit += CheckChange;
        EventManager.CreatureHealed += CheckChange;
    }

    void OnDisable()
    {
        EventManager.CreatureHit -= CheckChange;
        EventManager.CreatureHealed -= CheckChange;
    }

    void CheckChange(LivingBeing lb, byte unused = 0)
    {
        if (lb is Player)
        {
            float percent = (float)lb.Health / lb.MaxHealth;
            if (percent > 0.75f)
                GetComponent<Image>().sprite = FullHP;
            else if (percent > 0.5f)
                GetComponent<Image>().sprite = ThreeQuatersHP;
            else if (percent > 0.25f)
                GetComponent<Image>().sprite = HalfHP;
            else
                GetComponent<Image>().sprite = QuaterHP;
        }
    }
}
