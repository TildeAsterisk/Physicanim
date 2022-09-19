using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicanimBehaviour : MonoBehaviour
{
    public Transform leftFoot, rightFoot;
    public Transform hips;
    [Range(0f, 5f)]
    public float maxDist = 1.1f;
    Physicanimator physAnim;
    Animator charAnim;

    // Start is called before the first frame update
    void Start()
    {
        physAnim = hips.root.gameObject.GetComponent<Physicanimator>();
        charAnim = hips.root.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("Walking Left Turn"))
        {
            Resilience();
        }

        //Set hipmotion based on animation state NEED TO FIX THIS
        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("FetalPosition"))
        {
            physAnim.SetJointMotionType(null, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free);
        }
        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("Getting Up"))
        {
            //physAnim.ResetStaticAnimPos();
            physAnim.SetJointMotionType(null, ConfigurableJointMotion.Free, ConfigurableJointMotion.Locked);

        }
        if (charAnim.GetCurrentAnimatorStateInfo(0).IsName("Walking Left Turn"))
        {
            //TO DO: Before locking hip position, set anim pos to physicsbody pos
            //ONSTATEMACHINEENTER physAnim.ResetStaticAnimPos();
            physAnim.SetJointMotionType(null, ConfigurableJointMotion.Locked, ConfigurableJointMotion.Locked);
        }
    }

    void Resilience()
    {
        Vector3 feetCentrePoint = new Vector3((leftFoot.position.x+rightFoot.position.x)/2, (leftFoot.position.y + rightFoot.position.y) / 2, (leftFoot.position.z + rightFoot.position.z) / 2);
        Vector3 hipsXZ = new Vector3(hips.position.x, feetCentrePoint.y, hips.position.z);
        if (Vector3.Distance(feetCentrePoint, hips.position) > maxDist)
        {
            Debug.Log("Off Balance!");
            physAnim.SetJointMotionType(null, ConfigurableJointMotion.Free, ConfigurableJointMotion.Free);
            //set fetal position animation
            charAnim.CrossFade("FetalPosition",0.5f,0,0);
            
        }
    }
}

[System.Serializable]
public class PhysicanimCharacterBehaviour
{
    public GameObject PhysicanimCharBehaviour;
    [Range(0f, 5f)]
    public float maxDist = 1.1f;
}
