using UnityEngine;
using UnityEditor;
using System.Collections;

public class InventorySettingsEditor : EditorWindow {


    [MenuItem("Game manager/Inventory/Inventory settings")]
    static void Init()
    {

        GetWindow(typeof(InventorySettingsEditor));
    }

    void OnGUI()
    {

    }
}
