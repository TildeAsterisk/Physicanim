using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
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
    public bool limitHipMovement = true;
    public Material debugMat;
    public Transform physicsBodyRoot;
    public Transform staticAnimRoot;

    [SerializeField]
    PhysicanimCharacterBehaviour[] physicanimCharacterBehaviours;

    void OnValidate()
    {
        //ToggleHipLock(limitHipMovement);
        SetJointSprings();
    }

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
        HideStaticAnimMesh();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        /*
        if (jointsLerping && t <= 1)
        {
            LerpJointSprings(0.01f);
        }
        */

        UpdateJointTargets();
        //UpdateFeetTargets(); //NO IK YET
    }

    //Matching the rotation of each cj to the animated bones.
    private void UpdateJointTargets()
    {
        for (int i = 0; i < cJoints.Length; i++)    //For each joint in cJoints, set target rotation to that of anim bone
        {
            ConfigurableJointExtensions.SetTargetRotationLocal(cJoints[i], animBones[i].localRotation, initialJointRots[i]);
        }
    }

    //Match foot IK target pos to anim foot pos
    private void UpdateFeetTargets()
    {
        //update iktarget position
        //rFootTarget.position = rFootAnim.position;
        //lFootTarget.position = lFootAnim.position;

        //update IK target rotation
        //rFootTarget.transform.eulerAngles = new Vector3(0, hips.eulerAngles.y, 0);
        //lFootTarget.transform.eulerAngles = new Vector3(0, hips.eulerAngles.y, 0);
    }

    void SetJointSprings()
    {
        SetJointParams(cJoints[0], 0, 0);   //Set hip spring and damper to zero

        for (int i = 1; i < cJoints.Length; i++)    //set joints to chosen values
        {
            SetJointParams(cJoints[i], jointSpringsStrength, jointSpringDamper);
            //Debug.Log("Springs set for "+cJoints[i]);
        }
    }

    void SetJointParams(ConfigurableJoint cj, float posSpring, float posDamper)
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

    public void GoLimp()
    {
        for (int i = 0; i < cJoints.Length; i++) {
            SetJointParams(cJoints[i], 0, 0);
        }
        Debug.Log("Gone Limp!");
    }

    private bool jointsLerping;
    public void Revive()
    {
        //start lerping
        t = 0;
        jointsLerping = true;
        /*
        for (int i = 0; i < cJoints.Length; i++)
        {
            JointDrive jDrivex = cJoints[i].angularXDrive;
            JointDrive jDriveyz = cJoints[i].angularYZDrive;
            jDrivex.positionSpring = initialJointSprings[i,0];
            jDriveyz.positionSpring = initialJointSprings[i, 1];
            cJoints[i].angularXDrive = jDrivex;
            cJoints[i].angularYZDrive = jDriveyz;
        }
        */
    }
    
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

    void HideStaticAnimMesh(){
        SkinnedMeshRenderer sAnimMesh = animBones[0].parent.GetComponentInChildren<SkinnedMeshRenderer>();
        if (sAnimMesh != null){

            //sAnimMesh.sharedMesh = null;
            sAnimMesh.material = debugMat;
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

    public void ResetStaticAnimPos()    //Set static anim pos to physicsbody pos
    {
        staticAnimRoot.position = new Vector3(ragdollBones[0].position.x, staticAnimRoot.position.y, ragdollBones[0].position.y);
        animBones[0].position = ragdollBones[0].position;
    }

}