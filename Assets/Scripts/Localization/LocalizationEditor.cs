using UnityEngine; 
using System.Collections; 
using UnityEditor; 

[CustomEditor(typeof(LocalizationUI))] 
public class LocalizationEditor : Editor 
{ 
	public override void OnInspectorGUI() 
	{ 
		DrawDefaultInspector(); 
		
		LocalizationUI myScript = (LocalizationUI)target; 
		if(GUILayout.Button("Generate Keys")) 
		{ 
			myScript.generateKeys(); 
		} 
	} 
}