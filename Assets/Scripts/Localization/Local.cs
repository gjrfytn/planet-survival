using UnityEngine; 
using System.Collections; 
using System.Xml; 
using System.IO; 
using System; 
using System.Text; 
using System.Collections.Generic; 

public class Local { 
	
	private static Local instance; 
	
	private Dictionary<string, string> d_text = new Dictionary<string, string>(); 
	
	
	private Local(){ 
		instance = this; 
		initLocal (); 
	} 
	
	public static Local getInstance(){ 
		if (instance == null) { 
			new Local(); 
		} 
		return instance; 
	} 
	
	private void initLocal(string language){ 
		TextAsset textAsset = (TextAsset) Resources.Load("Localization/" + language + "/strings");   
		Debug.Log ("Localization/" + language + "/strings");
		XmlDocument xmldoc = new XmlDocument (); 
		xmldoc.LoadXml ( textAsset.text ); 
		
		XmlElement myElement = xmldoc.DocumentElement; 
		XmlNodeList date = myElement.GetElementsByTagName("string");   
		d_text.Clear (); 
		for (int i = 0; i < date.Count; i++) { 
			d_text.Add(date [i].Attributes [0].InnerText.ToLower(), date [i].InnerText); 
		} 
	} 
	
	private void initLocal(){ 
		initLocal (getSystemLanguage ()); 
	} 
	
	private string getSystemLanguage(){ 
		string local = ""; 
		string lang = Application.systemLanguage.ToString (); 
		if (lang.Equals ("English")) { 
			local = "en"; 
		} else if (lang.Equals ("Russian")) { 
			local = "ru"; 
		} else if (lang.Equals ("Ukrainian")) { 
			local = "ru"; 
		} else if (lang.Equals ("Belarusian")) { 
			local = "ru"; 
		} else { 
			local = "en"; 
		} 
		return local; 
	} 
	
	public void generateKeys(){ 
		initLocal ("ru"); 
		string path = Directory.GetCurrentDirectory() + "Assets/Scripts/Localization/LocalKeys.cs"; 
		Debug.Log (path); 
		if (File.Exists(path)) 
		{ 
			File.Delete(path); 
		} 
		
		string textFile = "public enum Keys{"; 
		foreach (string key in d_text.Keys) { 
			textFile += key +", "; 
		} 
		textFile = textFile.Substring (0, textFile.Length - 2); 
		
		textFile += "}"; 
		using (FileStream fs = File.Create(path)) 
		{ 
			Byte[] info = new UTF8Encoding(true).GetBytes(textFile); 
			fs.Write(info, 0, info.Length); 
		} 
		Debug.Log ("generate Keys finish"); 
	} 
	
	public string getText(string key){ 
		string result = ""; 
		try{ 
			result = d_text [key.ToLower ()]; 
		} catch(Exception /*e*/){ //TODO
			throw new NullReferenceException("not found this key - " + key); 
		} 
		return result; 
	} 
	
	//public string getText(Keys key){ 
		//return getText(key.ToString()); 
	//} 
} 
