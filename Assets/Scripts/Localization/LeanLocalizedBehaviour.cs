using UnityEngine;

namespace Lean
{
	// This component simplifies the updating process, extend it if you want to cause a specific object to get localized
	public abstract class LeanLocalizedBehaviour : MonoBehaviour
	{
		// The name of the phrase we want to use
		[LeanPhraseName]
		public string PhraseName;
		
		// This gets called every time the translation needs updating
		// NOTE: translation may be null if it can't be found
		public abstract void UpdateTranslation(LeanTranslation translation);
		
		public void SetPhraseName(string newPhraseName)
		{
			if (PhraseName != newPhraseName)
			{
				PhraseName = newPhraseName;
				
				UpdateLocalization();
			}
		}
		
		// Call this to force the behaviour to get updated
		public void UpdateLocalization()
		{
			UpdateTranslation(LeanLocalization.GetTranslation(PhraseName));
		}
		
		protected virtual void OnEnable()
		{
			LeanLocalization.OnLocalizationChanged += UpdateLocalization;
			
			UpdateLocalization();
		}
		
		protected virtual void OnDisable()
		{
			LeanLocalization.OnLocalizationChanged -= UpdateLocalization;
		}
		
#if UNITY_EDITOR
		protected virtual void OnValidate()
		{
			UpdateLocalization();
		}
#endif
	}
}