using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lean
{
	// This attribute allows you to select a phrase from all the localizations in the scene
	public class LeanPhraseNameAttribute : PropertyAttribute
	{
	}

#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(LeanPhraseNameAttribute))]
	public class LeanPhraseNameDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var left  = position; left.xMax -= 40;
			var right = position; right.xMin = left.xMax + 2;
			
			EditorGUI.PropertyField(left, property);
			
			if (GUI.Button(right, "List") == true)
			{
				var menu = new GenericMenu();
				
				if (LeanLocalization.Instance != null)
				{
					for (var j = 0; j < LeanLocalization.Instance.Phrases.Count; j++)
					{
						var phraseName = LeanLocalization.Instance.Phrases[j].Name;
						
						menu.AddItem(new GUIContent(phraseName), property.stringValue == phraseName, () => { property.stringValue = phraseName; property.serializedObject.ApplyModifiedProperties(); });
					}
				}
				
				if (menu.GetItemCount() > 0)
				{
					menu.DropDown(right);
				}
				else
				{
					Debug.LogWarning("Your scene doesn't contain any phrases, so the phrase name list couldn't be created.");
				}
			}
		}
	}
#endif
}