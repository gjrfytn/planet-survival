using UnityEngine;
using System.Collections.Generic;

namespace Lean
{
	// This contains data about each phrase, which is then translated into different languages
	[System.Serializable]
	public class LeanPhrase
	{
		// The name/description of this phrase
		public string Name;
		
		// All the translations for this phrase
		public List<LeanTranslation> Translations = new List<LeanTranslation>();
		
		// Find the translation using this language, or return null
		public LeanTranslation FindTranslation(string language)
		{
			return Translations.Find(t => t.Language == language);
		}
		
		// Add a new translation to this phrase, or return the current one
		public LeanTranslation AddTranslation(string language)
		{
			var translation = FindTranslation(language);
			
			// Add it?
			if (translation == null)
			{
				translation = new LeanTranslation();
				
				translation.Language = language;
				
				Translations.Add(translation);
			}
			
			return translation;
		}
	}
}