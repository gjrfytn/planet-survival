using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FrameRate : MonoBehaviour {

    public Text FPSText;

    float UpdateInterval = 0.5f;

    float Accum = 0.0f;

    ushort Frames = 0;

    float TimeLeft;

    private void Start()
    {
        TimeLeft = UpdateInterval;
    }

    private void Update()
    {
        TimeLeft -= Time.deltaTime;
        Accum += Time.timeScale / Time.deltaTime;
        ++Frames;

        if (TimeLeft <= 0)
        {
            FPSText.text = "Frame rate: " + (Accum / Frames).ToString("f2");
            TimeLeft = UpdateInterval;
            Accum = 0.0f;
            Frames = 0;
        }
    }


}
