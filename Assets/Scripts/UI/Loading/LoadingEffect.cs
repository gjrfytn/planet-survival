using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingEffect : MonoBehaviour
{
	
    public CanvasGroup Alpha = null;
    public float Speed = 500;
    public bool LoadingAnimation = false;

    private Image Image = null;

    public Texture2D LoadingTexture = null;
    public float Size = 50f;


    void Awake()
    {
		Image = this.GetComponent<Image>();
    }

    void Update()
    {
		Loading();
    }

    void Loading()
    {
        if (LoadingAnimation)
        {
            this.transform.Rotate(((Vector3.forward * this.Speed) * Time.deltaTime), Space.World);
            Color alpha = new Color(1, 1, 1, 1);
            if (Alpha == null) { alpha = Image.color; } else { alpha.a = Alpha.alpha; }
            if (alpha.a < 1f)
            {
                alpha.a = Mathf.Lerp(alpha.a, 1f, Time.deltaTime * 2);
            }
            if (Alpha == null) { Image.color = alpha; } else { Alpha.alpha = alpha.a; }
        }
        else
        {
            Color alpha = new Color(1, 1, 1, 1);
            if (Alpha == null) { alpha = Image.color; } else { alpha.a = Alpha.alpha; }
            if (alpha.a > 0f)
            {
                alpha.a = Mathf.Lerp(alpha.a, 0f, Time.deltaTime * 2);
            }
            if (Alpha == null) { Image.color = alpha; } else { Alpha.alpha = alpha.a; }

        }
    }
}