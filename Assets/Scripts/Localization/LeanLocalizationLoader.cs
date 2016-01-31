using UnityEngine;
using UnityEngine.UI;

namespace Lean
{
	// This component will load localizations from a file
	// They must be in the format:
	// Phrase Name Here = Translation Here // Optional Comment Here
	[AddComponentMenu("Lean/Localization Loader")]
	public class LeanLocalizationLoader : MonoBehaviour
	{
		[LeanLanguageNameAttribute]
		public string SourceLanguage;
		
		public TextAsset Source;
		
		private static readonly char[] newlineCharacters = new char[] { '\r', '\n' };
		
		private static readonly string newlineString = "\\n";
		
		protected virtual void Start()
		{
			LoadFromSource();
		}
		
		[ContextMenu("Load From Source")]
		public void LoadFromSource()
		{
			if (Source != null && string.IsNullOrEmpty(SourceLanguage) == false)
			{
				var localization = LeanLocalization.GetInstance();
				var lines        = Source.text.Split(newlineCharacters, System.StringSplitOptions.RemoveEmptyEntries);
				
				foreach (var line in lines)
				{
					var equalsIndex = line.IndexOf('=');
					
					if (equalsIndex != -1)
					{
						var phraseName        = line.Substring(0, equalsIndex).Trim();
						var phraseTranslation = line.Substring(equalsIndex + 1).Trim();
						
						// Replace line markers with actual newlines
						phraseTranslation = phraseTranslation.Replace(newlineString, System.Environment.NewLine);
						
						// Does this entry have a comment?
						var commentIndex = phraseTranslation.IndexOf("//");
						
						if (commentIndex != -1)
						{
							phraseTranslation = phraseTranslation.Substring(0, commentIndex).Trim();
						}
						
						// Find or add the translation for this phrase
						var translation = localization.AddTranslation(SourceLanguage, phraseName);
						
						// Set the translation text for this phrase
						translation.Text = phraseTranslation;
					}
				}
				
				// Update translations?
				if (localization.CurrentLanguage == SourceLanguage)
				{
					LeanLocalization.UpdateTranslations();
				}
			}
		}
		
		// NOTE: Saving text files like this doesn't work with webplayer build set
		// If this doesn't work for other platforms then add them to the list
#if UNITY_EDITOR && !UNITY_WEBPLAYER
		[ContextMenu("Export Text Asset")]
		public void ExportTextAsset()
		{
			var localization = LeanLocalization.GetInstance();
			
			if (string.IsNullOrEmpty(SourceLanguage) == false)
			{
				if (localization.Languages.Contains(SourceLanguage) == true)
				{
					// Find where we want to save the file
					var path = UnityEditor.EditorUtility.SaveFilePanelInProject("Export Text Asset", SourceLanguage, "txt", "");
					
					// Make sure we didn't cancel the panel
					if (string.IsNullOrEmpty(path) == false)
					{
						var data        = "";
						var oldLanguage = localization.CurrentLanguage;
						
						localization.SetLanguage(SourceLanguage);
						
						// Add all phrase names and existing translations to lines
						for (var i = localization.Phrases.Count - 1; i >= 0; i--)
						{
							var phrase      = localization.Phrases[i];
							var translation = phrase.FindTranslation(SourceLanguage);
							
							data += phrase.Name + " = ";
							
							if (translation != null)
							{
								var text = translation.Text;
								
								// Replace all new line permutations with the new line token
								text = text.Replace("\r\n", newlineString);
								text = text.Replace("\n\r", newlineString);
								text = text.Replace("\n", newlineString);
								text = text.Replace("\r", newlineString);
								
								data += text;
							}
							
							if (i > 0)
							{
								data += "\r\n";
							}
						}
						
						// Revert language
						localization.SetLanguage(oldLanguage);
						
						// Write text to file
						System.IO.File.WriteAllText(path, data);
						
						// Import asset into project
						UnityEditor.AssetDatabase.ImportAsset(path);
						
						// Replace Soure with new Text Asset?
						var textAsset = (TextAsset)UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(TextAsset));
						
						if (textAsset != null)
						{
							Source = textAsset;
							
							UnityEditor.EditorGUIUtility.PingObject(textAsset);
							
							UnityEditor.EditorUtility.SetDirty(this);
						}
					}
				}
			}
			else
			{
				Debug.LogError("Can't export Text Asset for language that doesn't exist in the current localization");
			}
		}
#endif
	}
}