using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointMatch : MonoBehaviour
{
    public Transform[] ragdollBones = new Transform[13];
    public ConfigurableJoint[] cJoints = new ConfigurableJoint[11];
    public Transform[] animBones = new Transform[11];
    private Quaternion[] initialJointRots;
    public float jointSpringsStrength = 1000;
    public float[,] initialJointSprings;

    public Transform rFootTarget;
    public Transform lFootTarget;
    public Transform rFootAnim;
    public Transform lFootAnim;

    public Transform hips;

    // Start is called before the first frame update
    void Start()
    {
        //Assign references on startup using ragdollBones array initialised on RagdollBuilder Create.
        lFootAnim = ragdollBones[11];
        rFootAnim = ragdollBones[12];
        lFootTarget = ragdollBones[11];
        rFootTarget = ragdollBones[12];
        hips = ragdollBones[0];


        Transform[] initialaj = animBones;
        ConfigurableJoint[] initialcj = cJoints;

        initialJointRots = new Quaternion[cJoints.Length];
        for (int i = 0; i < cJoints.Length; i++)
        {
            initialJointRots[i] = cJoints[i].transform.localRotation;
        }

        initialJointSprings = new float[cJoints.Length,2];
        StoreInitialSprings();

        //UpdateFeetTargets(); //NO IK YET
        
    }

    void Update()
    {
        if (jointsLerping && t <= 1)
        {
            LerpJointSprings(0.01f);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateJointTargets(); //WHEN ENABLED, NO ANIM COPY

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
        rFootTarget.position = rFootAnim.position;
        lFootTarget.position = lFootAnim.position;

        //update IK target rotation
        rFootTarget.transform.eulerAngles = new Vector3(0, hips.eulerAngles.y, 0);
        lFootTarget.transform.eulerAngles = new Vector3(0, hips.eulerAngles.y, 0);
    }

    void StoreInitialSprings()
    {
        /*
        for (int i = 0; i < cJoints.Length; i++)
        {
            JointDrive jDrivex = cJoints[i].angularXDrive;
            JointDrive jDriveyz = cJoints[i].angularYZDrive;
            initialJointSprings[i, 0] = jDrivex.positionSpring;
            initialJointSprings[i, 1] = jDriveyz.positionSpring;
        }
        */
        for (int i = 0; i < cJoints.Length; i++)
        {
            JointDrive jDrivex = cJoints[i].angularXDrive;
            JointDrive jDriveyz = cJoints[i].angularYZDrive;
            jDrivex.positionSpring = jointSpringsStrength;
            jDriveyz.positionSpring = jointSpringsStrength;
            cJoints[i].angularXDrive = jDrivex;
            cJoints[i].angularYZDrive = jDriveyz;
            Debug.Log("Springs set for "+cJoints[i]);
        }
    }

    public void GoLimp()
    {
        for (int i = 0; i < cJoints.Length; i++) {
            JointDrive jDrivex = cJoints[i].angularXDrive;
            JointDrive jDriveyz = cJoints[i].angularYZDrive;
            jDrivex.positionSpring = 0;
            jDriveyz.positionSpring = 0;
            cJoints[i].angularXDrive = jDrivex;
            cJoints[i].angularYZDrive = jDriveyz;
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
}