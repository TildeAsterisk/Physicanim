using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class Physicanimator : MonoBehaviour
{
    public Transform[] ragdollBones = new Transform[13];
    public ConfigurableJoint[] cJoints = new ConfigurableJoint[11];
    public Transform[] animBones = new Transform[11];
    private Quaternion[] initialJointRots;
    [Range(0.0f,9999.0f)]
    public float jointSpringsStrength = 420;
    [Range(0.0f, 1000.0f)]
    public float jointSpringDamper = 1;
    public float[,] initialJointSprings;
    public Material debugMat;
    //public Transform physicsBodyRoot;
    public Transform staticAnimRoot;
    public bool limp;
    bool showDEBUG = false;
    public bool lockHipsToAnim;

    //[SerializeField]
    //PhysicanimCharacterBehaviour[] physicanimCharacterBehaviours;

    // Start is called before the first frame update
    void Start()
    {
        //Set initial joint springs
        Transform[] initialaj = animBones;
        ConfigurableJoint[] initialcj = cJoints;
        initialJointRots = new Quaternion[cJoints.Length];
        for (int i = 0; i < cJoints.Length; i++)
        {
            initialJointRots[i] = cJoints[i].transform.localRotation;
        }

        initialJointSprings = new float[cJoints.Length,2];
        SetJointSprings();

        //UpdateFeetTargets(); //NO IK YET
        ShowStaticAnimMesh(showDEBUG);
        LockPhysicsHipsToAnimHips(lockHipsToAnim);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateJointTargets();
        //ResetStaticAnimPos();   //Set position of static animator to physicsbody position.
    }

    //Matching the rotation of each cj to the animated bones.
    private void UpdateJointTargets()
    {
        //set indx to skip hips or no based on if limp
        int indx;
        if (limp) { indx = 1; }
        else      { indx = 0; }
        for (int i = indx; i < cJoints.Length; i++)    //For each joint in cJoints, set target rotation to that of anim bone
        {
            ConfigurableJointExtensions.SetTargetRotationLocal(cJoints[i], animBones[i].localRotation, initialJointRots[i]);
        }
    }

    void SetJointSprings()
    {
        //SetJointParams(cJoints[0], 0, 0);   //Set hip spring and damper to zero

        for (int i = 0; i < cJoints.Length; i++)    //set joints to chosen values
        {
            SetJointParams(cJoints[i], jointSpringsStrength, jointSpringDamper);
            //Debug.Log("Springs set for "+cJoints[i]);
        }
    }

    public void SetJointParams(ConfigurableJoint cj, float posSpring, float posDamper)
    {
        JointDrive jDrivex = cj.angularXDrive;
        JointDrive jDriveyz = cj.angularYZDrive;
        jDrivex.positionSpring = posSpring;
        jDriveyz.positionSpring = posSpring;
        jDrivex.positionDamper = posDamper;
        jDriveyz.positionDamper = posDamper;
        cj.angularXDrive = jDrivex;
        cj.angularYZDrive = jDriveyz;
    }

    #region LerpJoints [Unused]
    /*
    float t = 1;
    public void LerpJointSprings(float spd)
    {
        if (t <= 1)
        {
            t += spd * Time.deltaTime;
            for (int i = 0; i < cJoints.Length; i++)
            {
                JointDrive xjd = cJoints[i].angularXDrive;
                JointDrive yzjd = cJoints[i].angularYZDrive;
                xjd.positionSpring = Mathf.Lerp(cJoints[i].angularXDrive.positionSpring, initialJointSprings[i,0], t);
                yzjd.positionSpring = Mathf.Lerp(cJoints[i].angularYZDrive.positionSpring, initialJointSprings[i,1], t);
                cJoints[i].angularXDrive = xjd;
                cJoints[i].angularYZDrive = yzjd;

                if ((initialJointSprings[i, 0] - cJoints[i].angularXDrive.positionSpring) <= 0.1f)
                {
                    t = 1;
                    jointsLerping = false;
                }
            }
        }
    }
    */
#endregion

    void ShowStaticAnimMesh(bool enabled){
        SkinnedMeshRenderer sAnimMesh = animBones[0].parent.GetComponentInChildren<SkinnedMeshRenderer>();
        if (sAnimMesh != null && !enabled){

            //sAnimMesh.sharedMesh = null;
            //sAnimMesh.material = debugMat;
            sAnimMesh.enabled = false;
        }
        else if(sAnimMesh != null && enabled)
        {
            sAnimMesh.enabled = true;
        }
        else{
            Debug.Log("Mesh on static animator could not be found.");
        }
    }

    public void SetJointMotionType(ConfigurableJoint cj, ConfigurableJointMotion motionType, ConfigurableJointMotion angularMotionType)
    {
        if (cj == null) { cj = cJoints[0]; }    //if no cj give set to hips
        cj.xMotion = motionType;
        cj.zMotion = motionType;
        cj.yMotion = motionType;
        cj.angularXMotion = angularMotionType;
        cj.angularYMotion = angularMotionType;
        cj.angularZMotion = angularMotionType;
    }

    //CAUSING PROBLEMS
    public void ResetStaticAnimPos()    //Set static anim pos to physicsbody pos
    {
        animBones[0].parent.localPosition = new Vector3(
            ragdollBones[0].localPosition.x,
            0.9648402f,
            ragdollBones[0].localPosition.z);

        //Debug.Log("Reset anim pos to physbody pos");
    }

    public void SetHipLimitSpring(int hipSpring)
    {
        SoftJointLimitSpring tmplmt = cJoints[0].linearLimitSpring;
        tmplmt.spring = hipSpring;
        cJoints[0].linearLimitSpring = tmplmt;
    }

    public void LockPhysicsHipsToAnimHips(bool hipLock)
    {
        if (hipLock)
        {
            SoftJointLimit tmplmt = cJoints[0].linearLimit;
            tmplmt.limit = 0;
            cJoints[0].linearLimit = tmplmt;
        }
        else
        {
            SoftJointLimit tmplmt = cJoints[0].linearLimit;
            tmplmt.limit = 0.001f;
            cJoints[0].linearLimit = tmplmt;
        }
    }

}