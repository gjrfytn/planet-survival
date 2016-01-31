using UnityEngine;
using UnityEngine.UI;

namespace Lean
{
	// This component will update a Text component with localized text, or use a fallback if none is found
	[ExecuteInEditMode]
	[RequireComponent(typeof(Text))]
	[AddComponentMenu("Lean/Localized Text")]
	public class LeanLocalizedText : LeanLocalizedBehaviour
	{
		public bool AllowFallback;
		
		public string FallbackText;
		
		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the Text component attached to this GameObject
			var text = GetComponent<Text>();
			
			// Use translation?
			if (translation != null)
			{
				text.text = translation.Text;
			}
			// Use fallback?
			else if (AllowFallback == true)
			{
				text.text = FallbackText;
			}
		}
	}
}