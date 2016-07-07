using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    bool SoundMuted;
    private AudioClip Clip;

    [SerializeField]
    GameObject DebugUI;

    //=====================
    CursorMode CursorMode = CursorMode.Auto;
    public Texture2D CursorImage;
    void Awake()
    {
        Cursor.SetCursor(CursorImage, Vector2.zero, CursorMode);
    }
    //=====================

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
            DebugUI.SetActive(!DebugUI.activeSelf);
    }

    public void TogglePanel(GameObject panel)
    {
        panel.SetActive(!panel.activeSelf);
    }

    public void OnMuteSoundButtonClick(GameObject soundScrollbar) //TODO Целесообразно ли таким образом передавать скроллбар? Может привести к путанице.
    {
        SoundMuted = !SoundMuted;
        AudioListener.volume = SoundMuted ? 0 : soundScrollbar.GetComponent<Scrollbar>().value;
    }

    public void OnExitButtonClick()
    {
        Application.Quit();
    }

    public void LoadLevel(string levelName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
    public void OnSoundScrollbarScroll(GameObject soundScrollbar)
    {
        if (!SoundMuted)
            AudioListener.volume = soundScrollbar.GetComponent<Scrollbar>().value;
    }
}



