using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIController : MonoBehaviour 
{
	bool SoundMuted;

	public void TogglePanel (GameObject panel) 
	{
		panel.SetActive (!panel.activeSelf);
	}

	public void OnMuteSoundButtonClick (GameObject soundScrollbar) //TODO Целесообразно ли таким образом передавать скроллбар? Может привести к путанице.
	{
		SoundMuted = ! SoundMuted;
		AudioListener.volume =  SoundMuted ? 0 : soundScrollbar.GetComponent<Scrollbar>().value;
	}

	public void OnStartGameButtonClick (GameObject button) 
	{	
		Application.LoadLevel(1);	
	}

	public void OnSoundScrollbarScroll (GameObject soundScrollbar)
	{
		if(!SoundMuted)
			AudioListener.volume =  soundScrollbar.GetComponent<Scrollbar>().value;
	}
}
