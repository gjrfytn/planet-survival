using UnityEngine;

namespace Lean
{
	// This component will update a SpriteRenderer component with a localized sprite, or use a fallback if none is found
	[ExecuteInEditMode]
	[RequireComponent(typeof(SpriteRenderer))]
	[AddComponentMenu("Lean/Localized Sprite Renderer")]
	public class LeanLocalizedSpriteRenderer : LeanLocalizedBehaviour
	{
		public bool AllowFallback;
		
		public Sprite FallbackSprite;
		
		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the SpriteRenderer component attached to this GameObject
			var spriteRenderer = GetComponent<SpriteRenderer>();
			
			// Use translation?
			if (translation != null)
			{
				spriteRenderer.sprite = translation.Object as Sprite;
			}
			// Use fallback?
			else if (AllowFallback == true)
			{
				spriteRenderer.sprite = FallbackSprite;
			}
		}
	}
}