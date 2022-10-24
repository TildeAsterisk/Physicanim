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
        //GUILayout.Label("Physicanim Control Panel", EditorStyles.boldLabel);
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
        
        GUILayout.Label("", EditorStyles.boldLabel);

        /* //Shows Animator and Avatar component on current object if there is one.
        Animator charAnimField = (Animator)EditorGUILayout.ObjectField("Animator:",charAnim,typeof(Animator),true);
        Avatar avField = (Avatar)EditorGUILayout.ObjectField("Avatar (e.g: Rig, Skeleton):", charRig, typeof(Avatar),true);
        */

        if (GUILayout.Button("Create\nPhysicanim Character"))
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


        string winmsg;
        MessageType msgtype;
        if (charRig){
            winmsg = "Click the button to convert this character model into a Physicanim Character ~*";
            msgtype = MessageType.Info;
        }
        else{
            winmsg = "Please select a character model with a humanoid rig from the heirarchy and click the button.";
            msgtype = MessageType.Warning;
        }

        GUILayout.FlexibleSpace();  //positions the helpbox to the bottom of the editorwindow
        EditorGUILayout.HelpBox(winmsg, msgtype);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Physicanim by Tilde Asterisk ~* (2022)", EditorStyles.miniLabel);
        //GUIStyle style = new GUIStyle() { richText = true };
        //EditorGUILayout.TextField("Physicanim by Tilde Asterisk <a href='https://www.tildeasterisk.com/'>~*</a> (2022)", style);
        //GUILayout.Label("Physicanim by Tilde Asterisk <a href='https://www.tildeasterisk.com/'>~*OOOOOOOO</a> (2022)", style);
        EditorGUILayout.EndHorizontal();

        //EditorGUILayout.HelpBox("Please select a humanoid rigged character model and click the button below to create a Physicanim Character.", MessageType.None);
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
