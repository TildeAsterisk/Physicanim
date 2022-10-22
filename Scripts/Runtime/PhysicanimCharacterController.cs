using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PhysicanimCharacterController : MonoBehaviour
{
    Physicanimator physicanim;
    Vector3 rawInputMovement;
    Animator staticAnimator;
    public float movSpeed;
    public float rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        physicanim = GetComponent<Physicanimator>();
        staticAnimator = physicanim.staticAnimRoot.GetComponent<Animator>();
    }

    // Update is called once per frame
    //void Update(){}

    //FixedUpdate is called ???
    void FixedUpdate()
    {
        //If there is movement from input then move staticanim position and rotate
        if(rawInputMovement.magnitude >0){
            staticAnimator.SetBool("Walk",true);
            physicanim.staticAnimRoot.position = physicanim.staticAnimRoot.position + rawInputMovement * Time.deltaTime * movSpeed;
            Quaternion toRotation = Quaternion.LookRotation(rawInputMovement, Vector3.up);
            physicanim.staticAnimRoot.rotation = Quaternion.RotateTowards(physicanim.staticAnimRoot.rotation, toRotation, rotationSpeed*Time.deltaTime);
        }
        else{
            staticAnimator.SetBool("Walk",false);
        }
    }

    public void Fire(InputAction.CallbackContext context){
        Debug.Log("Fire!");
    }
    public void Move(InputAction.CallbackContext value){
        Vector2 inputMovement = value.ReadValue<Vector2>();
        rawInputMovement=new Vector3(inputMovement.x,0,inputMovement.y);
        //rawInputMovement.Normalize();

    }
    public void Jump(InputAction.CallbackContext context){
        Debug.Log("Jump!");
    }
}
