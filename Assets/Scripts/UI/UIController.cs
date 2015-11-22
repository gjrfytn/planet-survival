using UnityEngine;
using System.Collections;

public class UIController : MonoBehaviour {

	bool isMute;

	public void TogglePanelButton (GameObject panel) {
		panel.SetActive (!panel.activeSelf);
	}
	public void MuteSoundButton (){
		isMute = ! isMute;
		AudioListener.volume =  isMute ? 0 : 1;
	}
	public void StartGameButton (GameObject button) {
		
		Application.LoadLevel(1);
		
	}
  
}
