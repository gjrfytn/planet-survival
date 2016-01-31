using UnityEngine;
using UnityEngine.UI;

namespace Lean
{
	// This component will update an AudioSource component with localized text, or use a fallback if none is found
	[ExecuteInEditMode]
	[RequireComponent(typeof(AudioSource))]
	[AddComponentMenu("Lean/Localized Audio Source")]
	public class LeanLocalizedAudioSource : LeanLocalizedBehaviour
	{
		public bool AllowFallback;
		
		public AudioClip FallbackAudioClip;
		
		// This gets called every time the translation needs updating
		public override void UpdateTranslation(LeanTranslation translation)
		{
			// Get the AudioSource component attached to this GameObject
			var audioSource = GetComponent<AudioSource>();
			
			// Use translation?
			if (translation != null)
			{
				audioSource.clip = translation.Object as AudioClip;
			}
			// Use fallback?
			else if (AllowFallback == true)
			{
				audioSource.clip = FallbackAudioClip;
			}
		}
	}
}