using UnityEngine;
using UnityEditor;
using System.Collections;

class ActiveRagdollBuilder : ScriptableWizard
{
    public Transform pelvis;

    public Transform leftHips = null;
    public Transform leftKnee = null;
    public Transform leftFoot = null;

    public Transform rightHips = null;
    public Transform rightKnee = null;
    public Transform rightFoot = null;

    public Transform leftArm = null;
    public Transform leftElbow = null;

    public Transform rightArm = null;
    public Transform rightElbow = null;

    public Transform middleSpine = null;
    public Transform head = null;


    public float totalMass = 20;
    public float strength = 0.0F;

    Vector3 right = Vector3.right;
    Vector3 up = Vector3.up;
    Vector3 forward = Vector3.forward;

    Vector3 worldRight = Vector3.right;
    Vector3 worldUp = Vector3.up;
    Vector3 worldForward = Vector3.forward;
    public bool flipForward = false;

    class BoneInfo
    {
        public string name;

        public Transform anchor;
        public ConfigurableJoint joint;
        public BoneInfo parent;

        public float minLimit;
        public float maxLimit;
        public float swingLimit;

        public Vector3 axis;
        public Vector3 normalAxis;

        public float radiusScale;
        public System.Type colliderType;

        public ArrayList children = new ArrayList();
        public float density;
        public float summedMass;// The mass of this and all children bodies
    }

    ArrayList bones;
    BoneInfo rootBone;

    string CheckConsistency()
    {
        PrepareBones();
        Hashtable map = new Hashtable();
        foreach (BoneInfo bone in bones)
        {
            if (bone.anchor)
            {
                if (map[bone.anchor] != null)
                {
                    BoneInfo oldBone = (BoneInfo)map[bone.anchor];
                    return string.Format("{0} and {1} may not be assigned to the same bone.", bone.name, oldBone.name);
                }
                map[bone.anchor] = bone;
            }
        }

        foreach (BoneInfo bone in bones)
        {
            if (bone.anchor == null)
                return string.Format("{0} has not been assigned yet.\n", bone.name);
        }

        return "";
    }

    void OnDrawGizmos()
    {
        if (pelvis)
        {
            Gizmos.color = Color.red; Gizmos.DrawRay(pelvis.position, pelvis.TransformDirection(right));
            Gizmos.color = Color.green; Gizmos.DrawRay(pelvis.position, pelvis.TransformDirection(up));
            Gizmos.color = Color.blue; Gizmos.DrawRay(pelvis.position, pelvis.TransformDirection(forward));
        }
    }

    //[MenuItem("[ ~* ]/Active Ragdoll Builder", false, 2000)]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<ActiveRagdollBuilder>("Create Ragdoll");
    }

    void DecomposeVector(out Vector3 normalCompo, out Vector3 tangentCompo, Vector3 outwardDir, Vector3 outwardNormal)
    {
        outwardNormal = outwardNormal.normalized;
        normalCompo = outwardNormal * Vector3.Dot(outwardDir, outwardNormal);
        tangentCompo = outwardDir - normalCompo;
    }

    void CalculateAxes()
    {
        if (head != null && pelvis != null)
            up = CalculateDirectionAxis(pelvis.InverseTransformPoint(head.position));
        if (rightElbow != null && pelvis != null)
        {
            Vector3 removed, temp;
            DecomposeVector(out temp, out removed, pelvis.InverseTransformPoint(rightElbow.position), up);
            right = CalculateDirectionAxis(removed);
        }

        forward = Vector3.Cross(right, up);
        if (flipForward)
            forward = -forward;
    }

    public void OnWizardUpdate()
    {
        errorString = CheckConsistency();
        CalculateAxes();

        if (errorString.Length != 0)
        {
            helpString = "Drag all bones from the hierarchy into their slots.\nMake sure your character is in T-Stand.\n";
        }
        else
        {
            helpString = "Make sure your character is in T-Stand.\nMake sure the blue axis faces in the same direction the chracter is looking.\nUse flipForward to flip the direction";
        }

        isValid = errorString.Length == 0;
    }

    void PrepareBones()
    {
        if (pelvis)
        {
            worldRight = pelvis.TransformDirection(right);
            worldUp = pelvis.TransformDirection(up);
            worldForward = pelvis.TransformDirection(forward);
        }

        bones = new ArrayList();

        rootBone = new BoneInfo();
        rootBone.name = "Pelvis";
        rootBone.anchor = pelvis;
        rootBone.parent = null;
        rootBone.density = 2.5F;
        bones.Add(rootBone);

        AddMirroredJoint("Hips", leftHips, rightHips, "Pelvis", worldRight, worldForward, -20, 70, 30, typeof(CapsuleCollider), 0.2F, 1.5F);
        AddMirroredJoint("Knee", leftKnee, rightKnee, "Hips", worldRight, worldForward, -80, 0, 0, typeof(CapsuleCollider), 0.15F, 1.5F);

        AddJoint("Middle Spine", middleSpine, "Pelvis", worldRight, worldForward, -20, 20, 10, null, 1, 2.5F);

        AddMirroredJoint("Arm", leftArm, rightArm, "Middle Spine", worldUp, worldForward, -70, 10, 50, typeof(CapsuleCollider), 0.25F, 1.0F);
        AddMirroredJoint("Elbow", leftElbow, rightElbow, "Arm", worldForward, worldUp, -90, 0, 0, typeof(CapsuleCollider), 0.15F, 1.0F);

        AddJoint("Head", head, "Middle Spine", worldRight, worldForward, -40, 25, 25, null, 1, 1.0F);
    }

    //~~~~~~~~~~~~~* C R E A T E  R A G D O L L  B U T T O N *~~~~~~~~~~~~~~~~
    public void OnWizardCreate()
    {
        GameObject staticAnim = InitialiseARagObjs();

        Cleanup();
        BuildCapsules();
        AddBreastColliders();
        AddHeadCollider();

        BuildBodies();
        BuildJoints();
        CalculateMass();

        //TO DO: pass animator to JointMatch
        SetupJointMatch(staticAnim);
    }

    BoneInfo FindBone(string name)
    {
        foreach (BoneInfo bone in bones)
        {
            if (bone.name == name)
                return bone;
        }
        return null;
    }

    void AddMirroredJoint(string name, Transform leftAnchor, Transform rightAnchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, System.Type colliderType, float radiusScale, float density)
    {
        AddJoint("Left " + name, leftAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, colliderType, radiusScale, density);
        AddJoint("Right " + name, rightAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, colliderType, radiusScale, density);
    }

    void AddJoint(string name, Transform anchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, System.Type colliderType, float radiusScale, float density)
    {
        BoneInfo bone = new BoneInfo();
        bone.name = name;
        bone.anchor = anchor;
        bone.axis = worldTwistAxis;
        bone.normalAxis = worldSwingAxis;
        bone.minLimit = minLimit;
        bone.maxLimit = maxLimit;
        bone.swingLimit = swingLimit;
        bone.density = density;
        bone.colliderType = colliderType;
        bone.radiusScale = radiusScale;

        if (FindBone(parent) != null)
            bone.parent = FindBone(parent);
        else if (name.StartsWith("Left"))
            bone.parent = FindBone("Left " + parent);
        else if (name.StartsWith("Right"))
            bone.parent = FindBone("Right " + parent);


        bone.parent.children.Add(bone);
        bones.Add(bone);
    }

    void BuildCapsules()
    {
        foreach (BoneInfo bone in bones)
        {
            if (bone.colliderType != typeof(CapsuleCollider))
                continue;

            int direction;
            float distance;
            if (bone.children.Count == 1)
            {
                BoneInfo childBone = (BoneInfo)bone.children[0];
                Vector3 endPoint = childBone.anchor.position;
                CalculateDirection(bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);
            }
            else
            {
                Vector3 endPoint = (bone.anchor.position - bone.parent.anchor.position) + bone.anchor.position;
                CalculateDirection(bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);

                if (bone.anchor.GetComponentsInChildren(typeof(Transform)).Length > 1)
                {
                    Bounds bounds = new Bounds();
                    foreach (Transform child in bone.anchor.GetComponentsInChildren(typeof(Transform)))
                    {
                        bounds.Encapsulate(bone.anchor.InverseTransformPoint(child.position));
                    }

                    if (distance > 0)
                        distance = bounds.max[direction];
                    else
                        distance = bounds.min[direction];
                }
            }

            CapsuleCollider collider = Undo.AddComponent<CapsuleCollider>(bone.anchor.gameObject);
            collider.direction = direction;

            Vector3 center = Vector3.zero;
            center[direction] = distance * 0.5F;
            collider.center = center;
            collider.height = Mathf.Abs(distance);
            collider.radius = Mathf.Abs(distance * bone.radiusScale);
        }
    }

    void Cleanup()
    {
        foreach (BoneInfo bone in bones)
        {
            if (!bone.anchor)
                continue;

            Component[] joints = bone.anchor.GetComponentsInChildren(typeof(Joint));
            foreach (Joint joint in joints)
                Undo.DestroyObjectImmediate(joint);

            Component[] bodies = bone.anchor.GetComponentsInChildren(typeof(Rigidbody));
            foreach (Rigidbody body in bodies)
                Undo.DestroyObjectImmediate(body);

            Component[] colliders = bone.anchor.GetComponentsInChildren(typeof(Collider));
            foreach (Collider collider in colliders)
                Undo.DestroyObjectImmediate(collider);
        }
    }

    void BuildBodies()
    {
        foreach (BoneInfo bone in bones)
        {
            Undo.AddComponent<Rigidbody>(bone.anchor.gameObject);
            bone.anchor.GetComponent<Rigidbody>().mass = bone.density;
        }
    }

    void BuildJoints()
    {
        foreach (BoneInfo bone in bones)
        {
            if (bone.parent == null)
                continue;

            ConfigurableJoint joint = Undo.AddComponent<ConfigurableJoint>(bone.anchor.gameObject);
            bone.joint = joint;

            // Setup connection and axis
            joint.axis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.axis));
            //joint.swingAxis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.normalAxis));
            joint.anchor = Vector3.zero;
            joint.connectedBody = bone.parent.anchor.GetComponent<Rigidbody>();
            joint.enablePreprocessing = false; // turn off to handle degenerated scenarios, like spawning inside geometry.
            
            //Lock joint motion in all axes.
            joint.xMotion = ConfigurableJointMotion.Locked;
            joint.yMotion = ConfigurableJointMotion.Locked;
            joint.zMotion = ConfigurableJointMotion.Locked;

            // Setup limits
            SoftJointLimit limit = new SoftJointLimit();
            limit.contactDistance = 0; // default to zero, which automatically sets contact distance.

            limit.limit = bone.minLimit;
            //joint.lowTwistLimit = limit;

            limit.limit = bone.maxLimit;
            //joint.highTwistLimit = limit;

            limit.limit = bone.swingLimit;
            //joint.swing1Limit = limit;

            limit.limit = 0;
            //joint.swing2Limit = limit;
        }
    }

    void CalculateMassRecurse(BoneInfo bone)
    {
        float mass = bone.anchor.GetComponent<Rigidbody>().mass;
        foreach (BoneInfo child in bone.children)
        {
            CalculateMassRecurse(child);
            mass += child.summedMass;
        }
        bone.summedMass = mass;
    }

    void CalculateMass()
    {
        // Calculate allChildMass by summing all bodies
        CalculateMassRecurse(rootBone);

        // Rescale the mass so that the whole character weights totalMass
        float massScale = totalMass / rootBone.summedMass;
        foreach (BoneInfo bone in bones)
            bone.anchor.GetComponent<Rigidbody>().mass *= massScale;

        // Recalculate allChildMass by summing all bodies
        CalculateMassRecurse(rootBone);
    }

    static void CalculateDirection(Vector3 point, out int direction, out float distance)
    {
        // Calculate longest axis
        direction = 0;
        if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            direction = 1;
        if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            direction = 2;

        distance = point[direction];
    }

    static Vector3 CalculateDirectionAxis(Vector3 point)
    {
        int direction = 0;
        float distance;
        CalculateDirection(point, out direction, out distance);
        Vector3 axis = Vector3.zero;
        if (distance > 0)
            axis[direction] = 1.0F;
        else
            axis[direction] = -1.0F;
        return axis;
    }

    static int SmallestComponent(Vector3 point)
    {
        int direction = 0;
        if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
            direction = 1;
        if (Mathf.Abs(point[2]) < Mathf.Abs(point[direction]))
            direction = 2;
        return direction;
    }

    static int LargestComponent(Vector3 point)
    {
        int direction = 0;
        if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            direction = 1;
        if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            direction = 2;
        return direction;
    }

    static int SecondLargestComponent(Vector3 point)
    {
        int smallest = SmallestComponent(point);
        int largest = LargestComponent(point);
        if (smallest < largest)
        {
            int temp = largest;
            largest = smallest;
            smallest = temp;
        }

        if (smallest == 0 && largest == 1)
            return 2;
        else if (smallest == 0 && largest == 2)
            return 1;
        else
            return 0;
    }

    Bounds Clip(Bounds bounds, Transform relativeTo, Transform clipTransform, bool below)
    {
        int axis = LargestComponent(bounds.size);

        if (Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.min)) == below)
        {
            Vector3 min = bounds.min;
            min[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
            bounds.min = min;
        }
        else
        {
            Vector3 max = bounds.max;
            max[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
            bounds.max = max;
        }
        return bounds;
    }

    Bounds GetBreastBounds(Transform relativeTo)
    {
        // Pelvis bounds
        Bounds bounds = new Bounds();
        bounds.Encapsulate(relativeTo.InverseTransformPoint(leftHips.position));
        bounds.Encapsulate(relativeTo.InverseTransformPoint(rightHips.position));
        bounds.Encapsulate(relativeTo.InverseTransformPoint(leftArm.position));
        bounds.Encapsulate(relativeTo.InverseTransformPoint(rightArm.position));
        Vector3 size = bounds.size;
        size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;
        bounds.size = size;
        return bounds;
    }

    void AddBreastColliders()
    {
        // Middle spine and pelvis
        if (middleSpine != null && pelvis != null)
        {
            Bounds bounds;
            BoxCollider box;

            // Middle spine bounds
            bounds = Clip(GetBreastBounds(pelvis), pelvis, middleSpine, false);
            box = Undo.AddComponent<BoxCollider>(pelvis.gameObject);
            box.center = bounds.center;
            box.size = bounds.size;

            bounds = Clip(GetBreastBounds(middleSpine), middleSpine, middleSpine, true);
            box = Undo.AddComponent<BoxCollider>(middleSpine.gameObject);
            box.center = bounds.center;
            box.size = bounds.size;
        }
        // Only pelvis
        else
        {
            Bounds bounds = new Bounds();
            bounds.Encapsulate(pelvis.InverseTransformPoint(leftHips.position));
            bounds.Encapsulate(pelvis.InverseTransformPoint(rightHips.position));
            bounds.Encapsulate(pelvis.InverseTransformPoint(leftArm.position));
            bounds.Encapsulate(pelvis.InverseTransformPoint(rightArm.position));

            Vector3 size = bounds.size;
            size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;

            BoxCollider box = Undo.AddComponent<BoxCollider>(pelvis.gameObject);
            box.center = bounds.center;
            box.size = size;
        }
    }

    void AddHeadCollider()
    {
        if (head.GetComponent<Collider>())
            Destroy(head.GetComponent<Collider>());

        float radius = Vector3.Distance(leftArm.transform.position, rightArm.transform.position);
        radius /= 4;

        SphereCollider sphere = Undo.AddComponent<SphereCollider>(head.gameObject);
        sphere.radius = radius;
        Vector3 center = Vector3.zero;

        int direction;
        float distance;
        CalculateDirection(head.InverseTransformPoint(pelvis.position), out direction, out distance);
        if (distance > 0)
            center[direction] = -radius;
        else
            center[direction] = radius;
        sphere.center = center;
    }
    
    //Add JointMatch script to the root object of character. Update JointMatch with an array of the bones selected in Ragdoll Builder.
    void SetupJointMatch(GameObject sAnimObj)
    {
        //Called at the end of CreateWizard(), after radgoll has been built. 
        pelvis.transform.root.gameObject.AddComponent<Physicanimator>();  //Joint Match class added to root object
        Physicanimator jm = pelvis.transform.root.GetComponent<Physicanimator>();
        
        jm.cJoints[0] = pelvis.gameObject.AddComponent<ConfigurableJoint>(); //Add pelvis char joint as first joint on cJoints list. ~*

        jm.staticAnimRoot = sAnimObj.transform;

        //Set StaticAnimator object as marionette to control char movement when hipjoint movement is limited.
        //Rigidbody marionette_rb = sAnimObj.AddComponent<Rigidbody>();
        Transform sAnimHips = sAnimObj.transform.Find(pelvis.name);
        Rigidbody marionette_rb = sAnimHips.gameObject.AddComponent<Rigidbody>();
        marionette_rb.isKinematic = true;
        marionette_rb.useGravity = false;
        //Setup hip joint parameters
        jm.cJoints[0].xMotion = ConfigurableJointMotion.Limited;
        jm.cJoints[0].zMotion = ConfigurableJointMotion.Limited;
        jm.cJoints[0].yMotion = ConfigurableJointMotion.Limited;
        jm.cJoints[0].angularXMotion = ConfigurableJointMotion.Limited;
        jm.cJoints[0].angularYMotion = ConfigurableJointMotion.Limited;
        jm.cJoints[0].angularZMotion = ConfigurableJointMotion.Limited;

        jm.cJoints[0].connectedBody = marionette_rb;

        //set linear limit
        SoftJointLimit cjLinearLimit = jm.cJoints[0].linearLimit;
        cjLinearLimit.limit = 0.0001f;
        jm.cJoints[0].linearLimit = cjLinearLimit;
        SoftJointLimitSpring lmtSpring = jm.cJoints[0].linearLimitSpring;
        lmtSpring.spring = 4200;
        lmtSpring.damper = 28.0799f;
        jm.cJoints[0].linearLimitSpring = lmtSpring;

        //lmtSpr.spring = 9999;
        //lmtSpr.damper = 420;


        int bi = 0; //bi is the bone index.
        foreach (BoneInfo bone in bones)
        {
            jm.ragdollBones[bi] = bone.anchor;  //Setup RagdollBones array in JointMatch script
            
            //Add ConfigurableJoint on each bone to the cJoints array.
            ConfigurableJoint cj = bone.anchor.GetComponent<ConfigurableJoint>();
            if(cj != null){ jm.cJoints[bi] = cj; }

            //Setup animBones array in JointMatch script
            if(sAnimHips!=null)
            {
                foreach(Transform tf in sAnimHips.GetComponentsInChildren<Transform>()){
                    if (tf.name == bone.anchor.name){
                        jm.animBones[bi] = tf; //add to JointMatch.animBones
                        //Debug.Log(tf.name + " added to the animBones array in JointMatch.");
                    }
                }
            }
            else{ Debug.Log("The root of the skeleton (e.g.: pelvis/hips) could not be found."); }

            bi++;
        }
        
        jm.ragdollBones[11] = leftFoot;
        jm.ragdollBones[12] = rightFoot;
        //jm.animBones[11] = leftFoot;
        //jm.animBones[12] = rightFoot;
        bi += 2;    //Doesnt need to be done. We dont need the bone index anymore.
        
    }

    GameObject InitialiseARagObjs(){
        //Store PhysicsBody to variable.
        Transform PhysicsBodyTF = pelvis.root;
        PhysicsBodyTF.name = "PhysicsBody";

        //Create empty GameObject to serve as root, containing the PhysicsBody and StaticAnimator
        GameObject ActiveRagdollRoot = new GameObject();
        ActiveRagdollRoot.transform.position = PhysicsBodyTF.position;
        ActiveRagdollRoot.transform.rotation = PhysicsBodyTF.rotation;
        ActiveRagdollRoot.name = "~* Active-Ragdoll";
        //Create copy of character model root obj, to serve as StaticAnim.
        GameObject staticAnimator = Instantiate(PhysicsBodyTF.gameObject, pelvis.root.position ,Quaternion.identity);
        staticAnimator.name = "StaticAnimator";
        //TO DO: Give custom transparent material to static anim meshes.

        //If PhysicsBody had an animator. Delete it.
        Animator anim = PhysicsBodyTF.GetComponent<Animator>();
        if(anim == null){
            anim = PhysicsBodyTF.GetComponentInChildren<Animator>();
        }
        if (anim != null){ DestroyImmediate(anim); }
        //Debug.Log("Animator on PhysicsBody has been destroyed.");

        //If StaticAnimator doesn't have an animator component, add one.
        anim = staticAnimator.GetComponent<Animator>();
        if(anim == null){
            anim = staticAnimator.GetComponentInChildren<Animator>();
        }
        if (anim == null){ anim = staticAnimator.AddComponent<Animator>(); }
        //Debug.Log("Animator on Static Animator exists."+ anim.name);
        //Set animator variables
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        anim.applyRootMotion = true;
        anim.updateMode = AnimatorUpdateMode.AnimatePhysics;

        //Set ActiveRagdoll parent/child heirachry.
        PhysicsBodyTF.parent = ActiveRagdollRoot.transform;
        staticAnimator.transform.parent = ActiveRagdollRoot.transform;

        return staticAnimator;
    }

}

//Research windows that use EditorWindow and ScriptableWizard.
