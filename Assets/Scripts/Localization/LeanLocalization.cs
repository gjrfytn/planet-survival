using UnityEngine;
using System.Collections.Generic;

namespace Lean
{
	// This component stores a list of phrases, their translations, as well as manage a global list of translations for easy access
	[ExecuteInEditMode]
	[AddComponentMenu("Lean/Localization")]
	public class LeanLocalization : MonoBehaviour
	{
		// This contains the current LeanLocalization instance
		public static LeanLocalization Instance;
		
		// Dictionary of all the phrase names mapped to their current translations
		public static Dictionary<string, LeanTranslation> Translations = new Dictionary<string, LeanTranslation>();
		
		// Called when the language or something changes
		public static System.Action OnLocalizationChanged;
		
		// The list of all supported languages
		public List<string> Languages = new List<string>();
		
		// The list of all supported phrases
		public List<LeanPhrase> Phrases = new List<LeanPhrase>();
		
		// The current language
		[LeanLanguageName]
		public string CurrentLanguage;
		
		// This method will return the current instance, or create one
		public static LeanLocalization GetInstance()
		{
			if (Instance == null)
			{
				Instance = new GameObject(typeof(LeanLocalization).Name).AddComponent<LeanLocalization>();
			}
			
			return Instance;
		}
		
		// Change the current language of this instance?
		public void SetLanguage(string newLanguage)
		{
			if (CurrentLanguage != newLanguage)
			{
				CurrentLanguage = newLanguage;
				
				UpdateTranslations();
			}
		}
		
		// Get the current translation for this phrase, or return null
		public static LeanTranslation GetTranslation(string phraseName)
		{
			var translation = default(LeanTranslation);
			
			if (phraseName != null)
			{
				Translations.TryGetValue(phraseName, out translation);
			}
			
			return translation;
		}
		
		// Get the current text for this phrase, or return null
		public static string GetTranslationText(string phraseName)
		{
			var translation = GetTranslation(phraseName);
			
			if (translation != null)
			{
				return translation.Text;
			}
			
			return null;
		}
		
		// Get the current Object for this phrase, or return null
		public static Object GetTranslationObject(string phraseName)
		{
			var translation = GetTranslation(phraseName);
			
			if (translation != null)
			{
				return translation.Object;
			}
			
			return null;
		}
		
		// Add a new language to this localization
		public void AddLanguage(string language)
		{
			if (Languages.Contains(language) == false)
			{
				Languages.Add(language);
			}
		}
		
		// Add a new phrase to this localization, or return the current one
		public LeanPhrase AddPhrase(string phraseName)
		{
			var phrase = Phrases.Find(p => p.Name == phraseName);
			
			if (phrase == null)
			{
				phrase = new LeanPhrase();
				
				phrase.Name = phraseName;
				
				Phrases.Add(phrase);
			}
			
			return phrase;
		}
		
		// Add a new translation to this localization, or return the current one
		public LeanTranslation AddTranslation(string language, string phraseName)
		{
			AddLanguage(language);
			
			return AddPhrase(phraseName).AddTranslation(language);
		}
		
		// This rebuilds the dictionary used to quickly map phrase names to translations for the current language
		public static void UpdateTranslations()
		{
			// Clear existing translation mapping
			Translations.Clear();
			
			// Does a localization instance exist?
			if (Instance != null)
			{
				// Go through all phrases
				for (var j = Instance.Phrases.Count - 1; j >= 0; j--)
				{
					var phrase = Instance.Phrases[j];
					
					// Make sure this phrase hasn't already been added
					if (Translations.ContainsKey(phrase.Name) == false)
					{
						// Find the translation for this phrase
						var translation = phrase.FindTranslation(Instance.CurrentLanguage);
						
						// If it exists, add it
						if (translation != null)
						{
							Translations.Add(phrase.Name, translation);
						}
					}
				}
			}
			
			// Notify changes?
			if (OnLocalizationChanged != null)
			{
				OnLocalizationChanged();
			}
		}
		
		// Set the instance, merge old instance, and update translations
		protected virtual void OnEnable()
		{
			// Make sure there's only one LeanLocalization active in the scene
			if (Instance != null && Instance != this)
			{
				MergeLocalizations(Instance, this);
				
				Debug.LogWarning("Your scene already contains a " + typeof(LeanLocalization).Name + ", merging & destroying the old one...");
				
				DestroyImmediate(Instance.gameObject);
			}
			
			// Set the instance
			Instance = this;
			
			// Update translations
			UpdateTranslations();
		}
		
		// Unset instance?
		protected virtual void OnDisable()
		{
			if (Instance == this)
			{
				Instance = null;
			}
			
			UpdateTranslations();
		}
		
#if UNITY_EDITOR
		// Inspector modified?
		protected virtual void OnValidate()
		{
			UpdateTranslations();
		}
#endif
		
		private static void MergeLocalizations(LeanLocalization oldLocalization, LeanLocalization newLocalization)
		{
			// Go through all old phrases
			for (var i = oldLocalization.Phrases.Count - 1; i >= 0; i--)
			{
				var oldPhrase = oldLocalization.Phrases[i];
				var newPhrase = newLocalization.AddPhrase(oldPhrase.Name);
				
				// Go through all old translations
				for (var j = oldPhrase.Translations.Count - 1; j >= 0; j--)
				{
					var oldTranslation = oldPhrase.Translations[j];
					var newTranslation = newPhrase.AddTranslation(oldTranslation.Language);
					
					newTranslation.Text   = oldTranslation.Text;
					newTranslation.Object = oldTranslation.Object;
				}
			}
		}
	}
}