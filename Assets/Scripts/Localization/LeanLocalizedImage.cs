using UnityEngine;
using UnityEngine.UI;

namespace Lean
{
	// This component will update an Image component with a localized sprite, or use a fallback if none is found
	[ExecuteInEditMode]
	[RequireComponent(typeof(Image))]
	[AddComponentMenu("Lean/Localized Image")]
	public class LeanLocalizedImage : LeanLocalizedBehaviour
	{
		public bool AllowFallback;
		
		public Sprite FallbackSprite;
		
		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the Image component attached to this GameObject
			var image = GetComponent<Image>();
			
			// Use translation?
			if (translation != null)
			{
				image.sprite = translation.Object as Sprite;
			}
			// Use fallback?
			else if (AllowFallback == true)
			{
				image.sprite = FallbackSprite;
			}
		}
	}
}