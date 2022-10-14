using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicanimBehaviour : MonoBehaviour
{
    Physicanimator physicAnim;
    Transform leftFoot, rightFoot;
    Transform hips;
    [Range(0f, 1.5f)]
    public float maxDist = 0.7f;
    Animator charAnim;

    void Start()
    {
        physicAnim = gameObject.GetComponent<Physicanimator>();
        charAnim = gameObject.GetComponentInChildren<Animator>();
        //Set bone tfs
        leftFoot = physicAnim.ragdollBones[11];
        rightFoot = physicAnim.ragdollBones[12];
        hips = physicAnim.ragdollBones[0];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if grounded
        if (!physicAnim.limp && !charAnim.GetCurrentAnimatorStateInfo(0).IsName("Getting Up"))
        {
            Resilience();
        }
        else if (physicAnim.limp && charAnim.GetCurrentAnimatorStateInfo(0).IsName("Getting Up"))
        {
            GetUp();
        }


        //TESTING!!!! RESET WHEN GETS TO FAR
        if (physicAnim.ragdollBones[0].position.z > 20)
        {
            Debug.Log("RELOADING SCENE");
            physicAnim.animBones[0].parent.position = Vector3.zero;
            physicAnim.animBones[0].position = Vector3.zero;
            physicAnim.ragdollBones[0].position = Vector3.zero;
        }
    }

        void FallDown()
        {
            Animator anim = physicAnim.staticAnimRoot.GetComponent<Animator>();
            anim.CrossFade("Fetal Pose", 0.5f, 0, 0);   //anim.Play("Fetal Pose", 0,0);

            physicAnim.SetJointParams(physicAnim.cJoints[0], 0.00001f, 0.00001f);
            /*// Set each joint spring and damper to 0
            for (int i = 0; i < cJoints.Length; i++)
            {
                SetJointParams(cJoints[i], 0, 0);
            }
            */

            //Set hip linear limit to HIGH
            physicAnim.lockHipsToAnim = false;
            physicAnim.LockPhysicsHipsToAnimHips(physicAnim.lockHipsToAnim);

            //set hip linearlimit spring
            SoftJointLimitSpring lmtSpring = physicAnim.cJoints[0].linearLimitSpring;
            lmtSpring.spring = 0.0001f;
            physicAnim.cJoints[0].linearLimitSpring = lmtSpring;

            //set angularlimits
            float maxlimit = 177f;
            SoftJointLimit tempLimit = physicAnim.cJoints[0].lowAngularXLimit;
            tempLimit.limit = -maxlimit;
            physicAnim.cJoints[0].lowAngularXLimit = tempLimit;

            tempLimit = physicAnim.cJoints[0].highAngularXLimit;
            tempLimit.limit = maxlimit;
            physicAnim.cJoints[0].highAngularXLimit = tempLimit;

            tempLimit = physicAnim.cJoints[0].angularYLimit;
            tempLimit.limit = maxlimit;
            physicAnim.cJoints[0].angularYLimit = tempLimit;

            tempLimit = physicAnim.cJoints[0].angularZLimit;
            tempLimit.limit = maxlimit;
            physicAnim.cJoints[0].angularZLimit = tempLimit;

            physicAnim.limp = true;
            Debug.Log("Gone Limp!");
        }

    public void GetUp()
    {
        /*
        for (int i = 0; i < physicAnim.cJoints.Length; i++)
        {
            //SetJointParams(cJoints[i], 0, 0);
        }
        */
        float maxAngLimit = 0;
        float lerpSpeed = 1f;
        float t = lerpSpeed * Time.fixedDeltaTime;
        //set linear limit for hips to 9999
        SoftJointLimit tempLimit;
        float slerpTarget = 2f;
        Debug.Log("Slerping:" + physicAnim.cJoints[0].highAngularXLimit.limit + "To:" + slerpTarget);
        if (physicAnim.cJoints[0].highAngularXLimit.limit > slerpTarget)
        {
            //Lerp joint X-YZ drive spring and
            float lerpedDriveSpring = Mathf.Lerp(physicAnim.cJoints[0].angularXDrive.positionSpring, physicAnim.jointSpringsStrength, t / 10);
            physicAnim.SetJointParams(physicAnim.cJoints[0], lerpedDriveSpring, physicAnim.jointSpringDamper);

            //lerp hip linearlimit spring [HIGH value brings physicHips to animHips]
            SoftJointLimitSpring lmtSpring = physicAnim.cJoints[0].linearLimitSpring;
            lmtSpring.spring = Mathf.Lerp(lmtSpring.spring, 9999, t / 10);
            physicAnim.cJoints[0].linearLimitSpring = lmtSpring;

            //lerp angularlimits
            tempLimit = physicAnim.cJoints[0].lowAngularXLimit;
            tempLimit.limit = Mathf.Lerp(tempLimit.limit, -maxAngLimit, t);
            physicAnim.cJoints[0].lowAngularXLimit = tempLimit;
            tempLimit = physicAnim.cJoints[0].highAngularXLimit;
            tempLimit.limit = Mathf.Lerp(tempLimit.limit, maxAngLimit, t);
            physicAnim.cJoints[0].highAngularXLimit = tempLimit;
            tempLimit = physicAnim.cJoints[0].angularYLimit;
            tempLimit.limit = Mathf.Lerp(tempLimit.limit, maxAngLimit, t);
            physicAnim.cJoints[0].angularYLimit = tempLimit;
            tempLimit = physicAnim.cJoints[0].angularZLimit;
            tempLimit.limit = Mathf.Lerp(tempLimit.limit, maxAngLimit, t);
            physicAnim.cJoints[0].angularZLimit = tempLimit;
        }
        else
        {
            physicAnim.SetJointParams(physicAnim.cJoints[0], physicAnim.jointSpringsStrength, physicAnim.jointSpringDamper);

            //TO DO: BEFORE SETTING LINEAR LIMIT 0, RESET ANIM HIPS POS
            physicAnim.ResetStaticAnimPos();

            //Set hip linear limit to HIGH
            physicAnim.lockHipsToAnim = true;
            physicAnim.LockPhysicsHipsToAnimHips(physicAnim.lockHipsToAnim);

            //set hip linearlimit spring
            SoftJointLimitSpring lmtSpring = physicAnim.cJoints[0].linearLimitSpring;
            lmtSpring.spring = 0.0001f;
            physicAnim.cJoints[0].linearLimitSpring = lmtSpring;

            //set angularlimits
            tempLimit = physicAnim.cJoints[0].lowAngularXLimit;
            tempLimit.limit = -maxAngLimit;
            physicAnim.cJoints[0].lowAngularXLimit = tempLimit;
            tempLimit = physicAnim.cJoints[0].highAngularXLimit;
            tempLimit.limit = maxAngLimit;
            physicAnim.cJoints[0].highAngularXLimit = tempLimit;
            tempLimit = physicAnim.cJoints[0].angularYLimit;
            tempLimit.limit = maxAngLimit;
            physicAnim.cJoints[0].angularYLimit = tempLimit;
            tempLimit = physicAnim.cJoints[0].angularZLimit;
            tempLimit.limit = maxAngLimit;
            physicAnim.cJoints[0].angularZLimit = tempLimit;
            physicAnim.limp = false;
            Debug.Log("Finished Getting Up!");
        }
    }


    void Resilience()
    {
        Vector3 feetCentrePoint = new Vector3((leftFoot.position.x + rightFoot.position.x) / 2, (leftFoot.position.y + rightFoot.position.y) / 2, (leftFoot.position.z + rightFoot.position.z) / 2);
        Vector3 hipsXZ = new Vector3(hips.position.x, feetCentrePoint.y, hips.position.z);
        if (Vector3.Distance(feetCentrePoint, hipsXZ) >= maxDist)
        {
            Debug.Log("Off Balance!");
            FallDown();
        }
    }

    /* 
    void OnDrawGizmosSelected()
    {
        Vector3 feetCentrePoint = new Vector3((leftFoot.position.x + rightFoot.position.x) / 2, (leftFoot.position.y + rightFoot.position.y) / 2, (leftFoot.position.z + rightFoot.position.z) / 2);
        Vector3 hipsXZ = new Vector3(hips.position.x, feetCentrePoint.y, hips.position.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hipsXZ, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(feetCentrePoint, 0.05f);
    }
    */
}

[System.Serializable]
public class PhysicanimCharacterBehaviour
{
    public GameObject PhysicanimCharBehaviour;
    [Range(0f, 5f)]
    public float maxDist = 1.1f;
}
