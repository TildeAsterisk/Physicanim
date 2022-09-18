using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class PhysicanimGUI : EditorWindow
{
    //GameObject selectedObj;
    GameObject charModelObj;
    Animator charAnim;
    Avatar charRig;

    [MenuItem("[ ~* ]/Physicanim ~*")]
    public static void ShowWindow()
    {
        GetWindow<PhysicanimGUI>("Physicanim");
    }

    void OnGUI()
    {
        //Selected Object Fields
        GUILayout.Label("Physicanim Control Panel", EditorStyles.boldLabel);
        GUILayout.Box("Please select a rigged character model to be converted into an Physicanim active ragdoll.");
        //selectedObj = EditorGUILayout.ObjectField("Selected Object",Selection.activeObject, typeof(Object),true);
        charModelObj = (GameObject)EditorGUILayout.ObjectField("Character Model:",Selection.activeGameObject, typeof(GameObject),true);

        if (charModelObj) { charAnim = charModelObj.GetComponent<Animator>(); }
        else { charAnim = null; }
        if (charModelObj) { charRig = GetRig(charModelObj); }   //if selected GameObject has rig set charRig
        else { charRig = null; }

        //Check to show GUI
        if (charRig)
        { GUI.enabled = true; }
        else { GUI.enabled = false; }
        
        /* //Shows Animator and Avatar component on current object if there is one.
        Animator charAnimField = (Animator)EditorGUILayout.ObjectField("Animator:",charAnim,typeof(Animator),true);
        Avatar avField = (Avatar)EditorGUILayout.ObjectField("Avatar (e.g: Rig, Skeleton):", charRig, typeof(Avatar),true);
        */

        if (GUILayout.Button("Create Active Ragdoll"))
        {
            //Depending on ARagBuilder reference, could open window or not.
            //ActiveRagdollBuilder rdBuilder = ScriptableWizard.DisplayWizard<ActiveRagdollBuilder>("~* Active Ragdoll Builder");
            //ActiveRagdollBuilder rdBuilder = EditorWindow.GetWindow<ActiveRagdollBuilder>("~* Active Ragdoll Builder");
            ActiveRagdollBuilder rdBuilder = new ActiveRagdollBuilder();    //Doesn't display window

            //set each parameter in ragdoll builder from animator avatar
            rdBuilder.pelvis = charAnim.GetBoneTransform(HumanBodyBones.Hips);
            rdBuilder.leftHips = charAnim.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rdBuilder.leftKnee = charAnim.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rdBuilder.leftFoot = charAnim.GetBoneTransform(HumanBodyBones.LeftFoot);
            rdBuilder.rightHips = charAnim.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            rdBuilder.rightKnee = charAnim.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            rdBuilder.rightFoot = charAnim.GetBoneTransform(HumanBodyBones.RightFoot);
            rdBuilder.leftArm = charAnim.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rdBuilder.leftElbow = charAnim.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rdBuilder.rightArm = charAnim.GetBoneTransform(HumanBodyBones.RightUpperArm);
            rdBuilder.rightElbow = charAnim.GetBoneTransform(HumanBodyBones.RightLowerArm);
            rdBuilder.middleSpine = charAnim.GetBoneTransform(HumanBodyBones.Chest);
            rdBuilder.head = charAnim.GetBoneTransform(HumanBodyBones.Head);

            rdBuilder.OnWizardUpdate();
            rdBuilder.OnWizardCreate(); //Create ragdoll using wizard
        }
        GUI.enabled = true;
    }

    //Get the avatar from any charactermodel. in scene or assets
    Avatar GetRig(GameObject characterModel)
    {
        Animator anim = characterModel.GetComponent<Animator>();
        if (anim) 
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
