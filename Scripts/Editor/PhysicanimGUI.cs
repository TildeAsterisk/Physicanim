using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PhysicanimGUI : EditorWindow
{
    //Object selectedObj;
    GameObject charModelObj;
    Avatar charRig;

    [MenuItem("[ ~* ]/Physicanim")]
    public static void ShowWindow()
    {
        GetWindow<PhysicanimGUI>("Physicanim by ~*");
    }

    void OnGUI()
    {
        //Selected Object Fields
        GUILayout.Label("Convert character into Physanim~* active ragdoll.", EditorStyles.boldLabel);
        //selectedObj = EditorGUILayout.ObjectField("Selected Object",Selection.activeObject, typeof(Object),true);
        charModelObj = (GameObject)EditorGUILayout.ObjectField("Character Model:",Selection.activeGameObject, typeof(GameObject),true);
        //Check to show GUI
        if (charRig != null)
        { GUI.enabled = true; }
        else { GUI.enabled = false; }
        charRig = GetRig(charModelObj);
        charRig = (Avatar)EditorGUILayout.ObjectField("Avatar (e.g: Rig, Skeleton):", charRig, typeof(Avatar),true);

        if (GUILayout.Button("Create Active Ragdoll"))
        {
            Debug.Log(charRig.name);
            
        }
        GUI.enabled = true;
    }

    //Get the avatar from any charactermodel. in scene or assets
    Avatar GetRig(GameObject characterModel)
    {
        Animator anim = characterModel.GetComponent<Animator>();
        if (anim != null) 
        {
            return anim.avatar;
        }
        else 
        {
            Avatar av = characterModel.GetComponent<Avatar>();
            if (av) {return av;}
            else {return null;}
        }
    }
}


//Research windows that use EditorWindow and ScriptableWizard.