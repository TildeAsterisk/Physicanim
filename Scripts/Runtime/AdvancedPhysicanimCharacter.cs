using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AdvancedPhysicanimCharacter : MonoBehaviour
{
    Physicanimator physicanim;
    IKFootSolver rFootSolverIK;
    IKFootSolver lFootSolverIK;
    
    [SerializeField]
    [Range(0f,3f)]
    float feetSpacing, stepDistance, stepHeight, speed;

    // Start is called before the first frame update
    void Start()
    {   //ASSUMING THAT THE CHARACTER IS SET UP WITH A LOWER RIG AND A TwoBoneIKConstraint FOR EACH LEG.
        //EACH FOOT WITH A FootSolverIK SETUP
        physicanim = GetComponent<Physicanimator>();
        rFootSolverIK = physicanim.animBones[0].parent.Find("LowerRigIK").Find("RightLegIK").Find("RightLegIK_target").GetComponent<IKFootSolver>();
        lFootSolverIK = physicanim.animBones[0].parent.Find("LowerRigIK").Find("LeftLegIK").Find("LeftLegIK_target").GetComponent<IKFootSolver>();
        
        feetSpacing=0.2f;
        stepDistance=0.2f;
        stepHeight=0.3f;
        speed=1.5f;

        Debug.Log("Setting hip linear limit spring");
        physicanim.SetHipLimitSpring(500);
        Debug.Log("Setting feet solver IK variables");
        rFootSolverIK.footSpacing=feetSpacing;
        rFootSolverIK.stepDistance=stepDistance;
        rFootSolverIK.stepHeight=stepHeight;
        rFootSolverIK.speed=speed;
        lFootSolverIK.footSpacing=feetSpacing;
        lFootSolverIK.stepDistance=stepDistance;
        lFootSolverIK.stepHeight=stepHeight;
        lFootSolverIK.speed=speed;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}