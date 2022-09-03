using UnityEngine;
using UnityEditor;

public class ActiveRagdollGUI : EditorWindow
{
    [MenuItem("Example/Display simple Window")]
    static void Initialize()
    {
        EditorWindowTest window = (ActiveRagdollGUI)EditorWindow.GetWindow(typeof(EditorWindowTest), true, "My Empty Window");
    }

    void OnGUI()
    {
        //add stuff here buttons labels editable code

    }
}
