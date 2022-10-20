using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFootSolver : MonoBehaviour
{
    public Transform body;
    public IKFootSolver otherFootIK;
    public float footSpacing;
    public float stepDistance;
    public float stepHeight;
    public float speed;
    public LayerMask terrainLayer;
    public bool isRight;
    Vector3 oldPosition;
    Vector3 currentPosition;
    Vector3 newPosition;
    Ray ray;
    float lerp;
    public bool isStepping;
    public bool rayCasted = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = currentPosition;

        if (isRight && !rayCasted)
        { ray = new Ray(body.position + (body.right * footSpacing) + (-body.forward * footSpacing), Vector3.down); }
        else if (!isRight && !rayCasted)
        { ray = new Ray(body.position + (-body.right * footSpacing) + (body.forward * footSpacing), Vector3.down); }

        if (Physics.Raycast(ray, out RaycastHit info, 10, terrainLayer.value))
        {
            if (Vector3.Distance(newPosition, info.point) > stepDistance)
            {
                lerp = 0;
                newPosition = info.point;
                rayCasted = true;
            }
        }

        if (lerp < 1 && !otherFootIK.isStepping)
        {
            isStepping = true;
            Vector3 footPosition = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPosition.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            currentPosition = footPosition;
            lerp += Time.deltaTime * speed;
        }
        else
        {
            isStepping = false;
            oldPosition = newPosition;
            rayCasted = false;
        }
    }

private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(newPosition, 0.05f);
    }
}
