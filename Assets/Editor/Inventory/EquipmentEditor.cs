using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(Equipment))]
public class EquipmentEditor : Editor {

    Equipment Equipment;

    static void Init()
    {

    }
    void OnEnable()
    {
        Equipment = target as Equipment;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        /*EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        CreateEquipmentSlots();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        RemoveEquipmentSlots();
        EditorGUILayout.EndHorizontal();*/
    }

    void CreateEquipmentSlots()
    {
        if(GUILayout.Button("Create equipment slots"))
        {
            Equipment.CreateEquipmentSlots();
        }
    }
    void RemoveEquipmentSlots()
    {
        GUI.color = Color.red;
        if(GUILayout.Button("Remove equipment slots"))
        {
            Equipment.RemoveEquipmentSlots();
        }
        GUI.color = Color.white;
    }

}
