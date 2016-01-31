using UnityEngine;

namespace Lean
{
	// This contains data about the phrases after it's been translated to a specific language
	[System.Serializable]
	public class LeanTranslation
	{
		// The language of this translation
		public string Language;
		
		// The translated text
		public string Text;
		
		// The translated object (e.g. language specific texture)
		public Object Object;
	}
}