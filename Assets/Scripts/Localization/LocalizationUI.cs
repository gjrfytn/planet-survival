using UnityEngine; 
using UnityEngine.UI; 
using System.Collections; 
using System.Xml; 

public class LocalizationUI : MonoBehaviour { 
	
	public GameObject[] goArray; 
	
	void Start () { 
		foreach (GameObject ui in goArray) { 
			if (ui.GetComponent<Text> () != null) { 
				ui.GetComponent<Text>().text = Local.getInstance ().getText (ui.name);             
				
			} else{ 
				ui.GetComponentInChildren<Text>().text = Local.getInstance().getText(ui.name); 
			} 
		} 
		
		//goArray [0].GetComponent<Text>().text = Local.getInstance ().getText (Keys.back); 
	} 
	
	
	public void generateKeys(){ 
		Local.getInstance ().generateKeys (); 
	} 
} 