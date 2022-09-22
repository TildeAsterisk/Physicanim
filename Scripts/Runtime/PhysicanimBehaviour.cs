using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicanimBehaviour : MonoBehaviour
{
    public Transform leftFoot, rightFoot;
    public Transform hips;
    [Range(0f, 5f)]
    public float maxDist = 0.7f;
    Physicanimator physicAnim;
    Animator charAnim;

    // Start is called before the first frame update
    void Start()
    {
        physicAnim = hips.root.gameObject.GetComponent<Physicanimator>();
        charAnim = hips.root.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //if grounded
        if (!physicAnim.limp)
        {
            Resilience();
        }
        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("Getting Up"))
        {
            //physAnim.ResetStaticAnimPos();
            //physicAnim.GetUp();
            GetUp();

        }
        /*
        if (Input.GetKeyDown(KeyCode.Space) && physAnim.limp)
        {
            physAnim.GetUp();
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !physAnim.limp)
        {
            physAnim.GoLimp();
        }

        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("Walking Left Turn"))
        {
            Resilience();
        }
        //Set hipmotion based on animation state NEED TO FIX THIS
        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("FetalPosition"))
        {
            physAnim.SetJointMotionType(null, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free);
        }
       
        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("Walking Left Turn"))
        {
            //TO DO: Before locking hip position, set anim pos to physicsbody pos
            //ONSTATEMACHINEENTER physAnim.ResetStaticAnimPos();
            physAnim.SetJointMotionType(null, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked);
        }
        */
    }

    void FallDown()
    {
        Animator anim = physicAnim.staticAnimRoot.GetComponent<Animator>();
        anim.CrossFade("Fetal Pose", 0.5f, 0, 0);
        //anim.Play("Fetal Pose", 0,0);

        physicAnim.SetJointParams(physicAnim.cJoints[0], 0.00001f, 0.00001f);
        /*
        for (int i = 0; i < cJoints.Length; i++)
        {
            SetJointParams(cJoints[i], 0, 0);
        }
        */
        //set joint limits
        //SetJointMotionType(cJoints[0],ConfigurableJointMotion.Limited, ConfigurableJointMotion.Free);

        //set linear limit for hips to 9999
        //SoftJointLimit tempLimit = cJoints[0].linearLimit;
        //tempLimit.limit = 0.1f;
        //cJoints[0].linearLimit = tempLimit;
        //SoftJointLimitSpring lmtSpring = cJoints[0].linearLimitSpring;
        //lmtSpring.spring = 0.00001f;
        //cJoints[0].linearLimitSpring = lmtSpring;

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
        physicAnim.SetJointParams(physicAnim.cJoints[0], physicAnim.jointSpringsStrength, physicAnim.jointSpringDamper);
        for (int i = 0; i < physicAnim.cJoints.Length; i++)
        {
            //SetJointParams(cJoints[i], 0, 0);
        }

        float maxAngLimit = 0;
        float lerpSpeed = 1f;
        float t = lerpSpeed * Time.fixedDeltaTime;
        //set linear limit for hips to 9999
        SoftJointLimit tempLimit = physicAnim.cJoints[0].linearLimit;

        if (physicAnim.cJoints[0].highAngularXLimit.limit > 0.34f)
        {
            //lerp hip linearlimit spring
            SoftJointLimitSpring lmtSpring = physicAnim.cJoints[0].linearLimitSpring;
            lmtSpring.spring = Mathf.Lerp(lmtSpring.spring, 9999, t);
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
            //TO DO: BEFORE SETTING LINEAR LIMIT 0, RESET ANIM HIPS POS
            //ResetStaticAnimPos();
            //setting linear limit to 0 makes physanim rootmotion
            //tempLimit = cJoints[0].linearLimit;
            //tempLimit.limit = 0;
            //cJoints[0].linearLimit = tempLimit;

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

        //limp = false;
        Debug.Log("Recovering...");
    }


    void Resilience()
    {
        Vector3 feetCentrePoint = new Vector3((leftFoot.position.x+rightFoot.position.x)/2, (leftFoot.position.y + rightFoot.position.y) / 2, (leftFoot.position.z + rightFoot.position.z) / 2);
        Vector3 hipsXZ = new Vector3(hips.position.x, feetCentrePoint.y, hips.position.z);
        //Debug.Log(Vector3.Distance(feetCentrePoint, hipsXZ));
        if (Vector3.Distance(feetCentrePoint, hipsXZ) >= maxDist)
        {
            Debug.Log("Off Balance!");
            //physAnim.SetJointMotionType(null, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free);
            //set fetal position animation
            //charAnim.CrossFade("FetalPosition",0.5f,0,0);

            //physicAnim.GoLimp();
            FallDown();
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 feetCentrePoint = new Vector3((leftFoot.position.x + rightFoot.position.x) / 2, (leftFoot.position.y + rightFoot.position.y) / 2, (leftFoot.position.z + rightFoot.position.z) / 2);
        Vector3 hipsXZ = new Vector3(hips.position.x, feetCentrePoint.y, hips.position.z);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(hipsXZ, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(feetCentrePoint, 0.05f);
    }
}

[System.Serializable]
public class PhysicanimCharacterBehaviour
{
    public GameObject PhysicanimCharBehaviour;
    [Range(0f, 5f)]
    public float maxDist = 1.1f;
}
