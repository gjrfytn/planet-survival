using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Lean
{
	[CustomEditor(typeof(LeanLocalization))]
	public class LeanLocalization_Editor : Editor
	{
		// Languages
		private string[] newLanguages = new string[] { "Chinese", "English", "French", "German", "Japanese", "Korean", "Portuguese", "Russian", "Spanish", "Other" };
		
		// Currently expanded language
		private int languageIndex = -1;
		
		// Currently expanded language phrase
		private int reverseIndex = -1;
		
		// Currently expanded phrase
		private int phraseIndex = -1;
		
		// Currently expanded translation
		private int translationIndex = -1;
		
		private bool dirty;
		
		private List<string> existingLanguages = new List<string>();
		
		private List<string> existingPhrases = new List<string>();
		
		private GUIStyle labelStyle;
		
		private GUIStyle LabelStyle
		{
			get
			{
				if (labelStyle == null)
				{
					labelStyle = new GUIStyle(EditorStyles.label);
					labelStyle.clipping = TextClipping.Overflow;
				}
				
				return labelStyle;
			}
		}
		
		[MenuItem("Game Modules/Lean Localiztion/Create/Localization", false, 1)]
		public static void CreateLocalization()
		{
			var gameObject = new GameObject(typeof(LeanLocalization).Name);
			
			UnityEditor.Undo.RegisterCreatedObjectUndo(gameObject, "Create Localization");
			
			gameObject.AddComponent<LeanLocalization>();
			
			Selection.activeGameObject = gameObject;
		}
		
		// Draw the whole inspector
		public override void OnInspectorGUI()
		{
			var localization = (LeanLocalization)target;
			
			EditorGUILayout.Separator();
			
			EditorGUILayout.PropertyField(serializedObject.FindProperty("CurrentLanguage"));
			
			EditorGUILayout.Separator();
			
			DrawLanguages(localization);
			
			EditorGUILayout.Separator();
			EditorGUILayout.Separator();
			
			DrawPhrases(localization);
			
			EditorGUILayout.Separator();
			
			// Update if dirty?
			if (serializedObject.ApplyModifiedProperties() == true || dirty == true)
			{
				dirty = false;
				
				LeanLocalization.UpdateTranslations();
				
				EditorUtility.SetDirty(localization);
				EditorApplication.MarkSceneDirty();
			}
		}
		
		private void MarkAsModified()
		{
			dirty = true;
		}
		
		private void BeginModifications()
		{
			EditorGUI.BeginChangeCheck();
		}
		
		private void EndModifications()
		{
			if (EditorGUI.EndChangeCheck() == true)
			{
				dirty = true;
			}
		}
		
		private void DrawLanguages(LeanLocalization localization)
		{
			var labelA = Reserve();
			var valueA = Reserve(ref labelA, 35.0f);
			
			EditorGUI.LabelField(labelA, "Languages", EditorStyles.boldLabel);
			
			// Add a new language?
			if (GUI.Button(valueA, "Add") == true)
			{
				MarkAsModified();
				
				var menu = new GenericMenu();
				
				for (var i = 0; i < newLanguages.Length; i++)
				{
					var newLanguage = newLanguages[i];
					
					menu.AddItem(new GUIContent(newLanguage), false, () => localization.AddLanguage(newLanguage));
				}
				
				menu.DropDown(valueA);
			}
			
			existingLanguages.Clear();
			
			// Draw all added languages
			for (var i = 0; i < localization.Languages.Count; i++)
			{
				var labelB   = Reserve();
				var valueB   = Reserve(ref labelB, 20.0f);
				var language = localization.Languages[i];
				
				// Edit language name or remove
				if (languageIndex == i)
				{
					BeginModifications();
					{
						localization.Languages[i] = EditorGUI.TextField(labelB, language);
					}
					EndModifications();
					
					if (GUI.Button(valueB, "X") == true)
					{
						MarkAsModified();
						
						localization.Languages.RemoveAt(i); languageIndex = -1;
					}
				}
				
				// Expand language?
				if (EditorGUI.Foldout(labelB, languageIndex == i, languageIndex == i ? "" : language) == true)
				{
					if (languageIndex != i)
					{
						languageIndex = i;
						reverseIndex  = -1;
					}
					
					EditorGUI.indentLevel += 1;
					{
						DrawReverse(localization, language);
					}
					EditorGUI.indentLevel -= 1;
					
					EditorGUILayout.Separator();
				}
				else if (languageIndex == i)
				{
					languageIndex = -1;
					reverseIndex  = -1;
				}
				
				// Already added?
				if (existingLanguages.Contains(language) == true)
				{
					EditorGUILayout.HelpBox("This language already exists in the list!", MessageType.Warning);
				}
				else
				{
					existingLanguages.Add(language);
				}
			}
		}
		
		// Reverse lookup the phrases for this language
		private void DrawReverse(LeanLocalization localization, string language)
		{
			for (var i = 0; i < localization.Phrases.Count; i++)
			{
				var labelA      = Reserve();
				var phrase      = localization.Phrases[i];
				var translation = phrase.Translations.Find(t => t.Language == language);
				
				BeginModifications();
				{
					EditorGUI.LabelField(labelA, phrase.Name);
				}
				EndModifications();
				
				if (translation != null)
				{
					if (reverseIndex == i)
					{
						BeginModifications();
						{
							phrase.Name = EditorGUI.TextField(labelA, "", phrase.Name);
						}
						EndModifications();
					}
					
					if (EditorGUI.Foldout(labelA, reverseIndex == i, reverseIndex == i ? "" : phrase.Name) == true)
					{
						reverseIndex = i;
						
						EditorGUI.indentLevel += 1;
						{
							DrawTranslation(translation);
						}
						EditorGUI.indentLevel -= 1;
						
						EditorGUILayout.Separator();
					}
					else if (reverseIndex == i)
					{
						reverseIndex = -1;
					}
					
				}
				else
				{
					var valueA = Reserve(ref labelA, 120.0f);
					
					if (GUI.Button(valueA, "Create Translation") == true)
					{
						MarkAsModified();
						
						var newTranslation = new LeanTranslation();
						
						newTranslation.Language = language;
						newTranslation.Text     = phrase.Name;
						
						phrase.Translations.Add(newTranslation);
					}
				}
			}
		}
		
		private void DrawPhrases(LeanLocalization localization)
		{
			var labelA = Reserve();
			var valueA = Reserve(ref labelA, 35.0f);
			
			EditorGUI.LabelField(labelA, "Phrases", EditorStyles.boldLabel);
			
			if (GUI.Button(valueA, "Add") == true)
			{
				MarkAsModified();
				
				var newPhrase = new LeanPhrase();
				
				newPhrase.Name = "New Phrase";
				
				phraseIndex = localization.Phrases.Count;
				
				localization.Phrases.Add(newPhrase);
			}
			
			existingPhrases.Clear();
			
			for (var i = 0; i < localization.Phrases.Count; i++)
			{
				var labelB = Reserve();
				var valueB = Reserve(ref labelB, 20.0f);
				var phrase = localization.Phrases[i];
				
				if (phraseIndex == i)
				{
					BeginModifications();
					{
						phrase.Name = EditorGUI.TextField(labelB, "", phrase.Name);
					}
					EndModifications();
					
					if (GUI.Button(valueB, "X") == true)
					{
						MarkAsModified();
						
						localization.Phrases.RemoveAt(i); phraseIndex = -1;
					}
				}
				
				if (EditorGUI.Foldout(labelB, phraseIndex == i, phraseIndex == i ? "" : phrase.Name) == true)
				{
					if (phraseIndex != i)
					{
						phraseIndex      = i;
						translationIndex = -1;
					}
					
					EditorGUI.indentLevel += 1;
					{
						DrawTranslations(localization, phrase);
					}
					EditorGUI.indentLevel -= 1;
					
					EditorGUILayout.Separator();
				}
				else if (phraseIndex == i)
				{
					phraseIndex      = -1;
					translationIndex = -1;
				}
				
				if (existingPhrases.Contains(phrase.Name) == true)
				{
					EditorGUILayout.HelpBox("This phrase already exists in the list!", MessageType.Warning);
				}
				else
				{
					existingPhrases.Add(phrase.Name);
				}
			}
		}
		
		private void DrawTranslations(LeanLocalization localization, LeanPhrase phrase)
		{
			existingLanguages.Clear();
			
			for (var i = 0; i < phrase.Translations.Count; i++)
			{
				var labelA      = Reserve();
				var valueA      = Reserve(ref labelA, 20.0f);
				var translation = phrase.Translations[i];
				
				if (translationIndex == i)
				{
					BeginModifications();
					{
						translation.Language = EditorGUI.TextField(labelA, "", translation.Language);
					}
					EndModifications();
					
					if (GUI.Button(valueA, "X") == true)
					{
						MarkAsModified();
						
						phrase.Translations.RemoveAt(i); translationIndex = -1;
					}
				}
				
				if (EditorGUI.Foldout(labelA, translationIndex == i, translationIndex == i ? "" : translation.Language) == true)
				{
					translationIndex = i;
					
					EditorGUI.indentLevel += 1;
					{
						DrawTranslation(translation);
					}
					EditorGUI.indentLevel -= 1;
					
					EditorGUILayout.Separator();
				}
				else if (translationIndex == i)
				{
					translationIndex = -1;
				}
				
				if (existingLanguages.Contains(translation.Language) == true)
				{
					EditorGUILayout.HelpBox("This phrase has already been translated to this language!", MessageType.Warning);
				}
				else
				{
					existingLanguages.Add(translation.Language);
				}
				
				if (localization.Languages.Contains(translation.Language) == false)
				{
					EditorGUILayout.HelpBox("This translation uses a language that hasn't been set in the localization!", MessageType.Warning);
				}
			}
			
			for (var i = 0; i < localization.Languages.Count; i++)
			{
				var language = localization.Languages[i];
				
				if (phrase.Translations.Exists(t => t.Language == language) == false)
				{
					var labelA = Reserve();
					var valueA = Reserve(ref labelA, 120.0f);
					
					EditorGUI.LabelField(labelA, language);
					
					if (GUI.Button(valueA, "Create Translation") == true)
					{
						MarkAsModified();
						
						var newTranslation = new LeanTranslation();
						
						newTranslation.Language = language;
						newTranslation.Text     = phrase.Name;
						
						translationIndex = phrase.Translations.Count;
						
						phrase.Translations.Add(newTranslation);
					}
				}
			}
		}
		
		private void DrawTranslation(LeanTranslation translation)
		{
			BeginModifications();
			{
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Text", LabelStyle, GUILayout.Width(50.0f));
					
					translation.Text = EditorGUILayout.TextArea(translation.Text);
				}
				EditorGUILayout.EndHorizontal();
				
				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Object", LabelStyle, GUILayout.Width(50.0f));
					
					
					translation.Object = EditorGUILayout.ObjectField(translation.Object, typeof(Object), true);
				}
				EditorGUILayout.EndHorizontal();
			}
			EndModifications();
		}
		
		private static Rect Reserve(ref Rect rect, float rightWidth = 0.0f, float padding = 2.0f)
		{
			if (rightWidth == 0.0f)
			{
				rightWidth = rect.width - EditorGUIUtility.labelWidth;
			}
			
			var left  = rect; left.xMax -= rightWidth;
			var right = rect; right.xMin = left.xMax;
			
			left.xMax -= padding;
			
			rect = left;
			
			return right;
		}
		
		private static Rect Reserve(float height = 16.0f)
		{
			var rect = EditorGUILayout.BeginVertical();
			{
				EditorGUILayout.LabelField("", GUILayout.Height(height));
			}
			EditorGUILayout.EndVertical();
			
			return rect;
		}
	}
}