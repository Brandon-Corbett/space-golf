using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class gravity : MonoBehaviour
{
    public Transform MyTransform;
    private UnityEngine.GameObject[] GravityBois;
    private UnityEngine.GameObject[] CustomGravityBois;
    public Vector3 fGrav;
    public Rigidbody rb;
    private Vector3 OutputForce;
    public float GravConst = 0.667408f;
    private Vector3 OldPosition;
    public bool isOnLand = true;
    public float landFriction = 1f;
    public int TimeToDeath = 100;
    public string mode = "start";
    public Vector3 SwingVector;
    public float SwingForce;
    private Quaternion qTo;
    public Quaternion SwingQuaternion;
    public bool isInHole;
    public int Swings;
    public bool Victory;
    public bool inBounds = true;
    public Vector3 CustomGravity = new Vector3(0, 0, 0);
    public List<Vector3> oldSwing;
    public Transform CameraTransform;
    public bool inCustomGravity;
    public Vector3 CustomGravityVector;
    public Vector3 slowCheckPosition;
    public int hole;
    public GameObject trigger;
    public UnityEngine.GameObject[] gravityTriggers;

    void Start()
    {
        Victory = false;
        nextSwingInit();
        Swings = 0;
        GravityBois = GameObject.FindGameObjectsWithTag("HasGravity");
        CustomGravityBois = GameObject.FindGameObjectsWithTag("CustomGravity");
        rb.drag = 3.5f;
        rb.angularDrag = 3.5f;
        //rb.AddForce(new Vector3(Random.Range(-500, 500), 0, Random.Range(-500, 500)));
        Physics.IgnoreLayerCollision(8, 9, false);
        SwingQuaternion = Quaternion.Euler(SwingVector.normalized);
    }

    // Update is called once per frame
    void Update()
    {
        if (mode == "start"){
            rb.angularVelocity *= (1-rb.angularDrag);
            handleCustomGravity();
            BeInAir();
            if (Input.GetButton("Fire1"))
            {
                oldSwing.Clear();//store ball stats before making the swing, in case it lands out of bounds.
                oldSwing.Add(SwingVector);
                oldSwing.Add(new Vector3(SwingForce, 0, 0));
                oldSwing.Add(transform.position);
                oldSwing.Add(CameraTransform.position);

                TimeToDeath = 200;
                BeInAir();
                mode = "moving";
                rb.AddForce(SwingVector * SwingForce);
                Swings += 1;
            }
            if (Input.GetMouseButton(2))
            {
                SwingForce *= (1f + (0.01f * Input.GetAxis("Strength") + 0.01f * Input.GetAxis("Mouse Y")));
            }
            else
            {
                SwingForce *= (1f + (0.01f * Input.GetAxis("Strength")));
            }
            //these next 7 lines are the product of 5 hours, and a fuckton of googling as I tried and failed to understand what the fuck a quaternion is.
            //I have no idea why or how this works. Not sure how I even managed to cobble it together
            //When it finally worked, the axis were inverted and it turned 90 degrees to the left at the start for no fukin reason. What the fuck even is a quaternion?

            //First line rotates aim around the vertical axis as defined by the vector from the nearest gravity object to the ball
            //NOTE IN POST: "first line" refers to "SwingQuaternion = Quaternion.AngleAxis(Input.GetAxis("Horizontal"), (transform.position - FindClosestGravityObject().transform.position)) * SwingQuaternion;"; this was has been edited since to allow mouse movement control
            if (Input.GetMouseButton(0))
            {
                if (inCustomGravity)
                {
                    SwingQuaternion = Quaternion.AngleAxis(Input.GetAxis("Mouse X"), -CustomGravityVector) * SwingQuaternion;
                }
                else
                {
                    if (FindClosestGravityObject() != null)
                    {
                        SwingQuaternion = Quaternion.AngleAxis(Input.GetAxis("Mouse X"), (transform.position - FindClosestGravityObject().transform.position)) * SwingQuaternion;
                    }
                }
            }
            else
            {
                if (inCustomGravity)
                {
                    SwingQuaternion = Quaternion.AngleAxis(Input.GetAxis("Horizontal"), -CustomGravityVector) * SwingQuaternion;
                }
                else
                {
                    if (FindClosestGravityObject() != null)
                    {
                        SwingQuaternion = Quaternion.AngleAxis(Input.GetAxis("Horizontal"), (transform.position - FindClosestGravityObject().transform.position)) * SwingQuaternion;
                    }
                }
            }
            //look in the direction of the last SwingVector, and face up, again relative to the nearest gravity object
            //this is so I can just snag the transform.right in a sec so I can get the axis to turn around for vertical aiming.
            //it's kinda hacky, but it works, and I don't have to project anything onto any planes.
            if (inCustomGravity)
            {
                qTo = Quaternion.LookRotation(SwingVector, -CustomGravityVector);
            }
            else
            {
                if (FindClosestGravityObject() != null)
                {
                    qTo = Quaternion.LookRotation(SwingVector, (transform.position - FindClosestGravityObject().transform.position));
                }
            }
            //can't quaternion.right. gotta transform.right. don't ask me why, idfk
            transform.rotation = qTo;
            //ayy that weird shit I had mentioned earlier
            if (Input.GetMouseButton(0))
            {
                SwingQuaternion = Quaternion.AngleAxis(-Input.GetAxis("Vertical") - Input.GetAxis("Mouse Y"), transform.right) * SwingQuaternion;
            }
            else
            {
                SwingQuaternion = Quaternion.AngleAxis(-Input.GetAxis("Vertical"), transform.right) * SwingQuaternion;
            }
            //This line is just fucking magic. if I set it to euler angles, it just fucks up when changing axis (-x to +x, +y to -y, etc) and turns it 90 degrees.
            //Why? idfk. Why does this fix it? idfk. I'm too tired for this bullshit. It works. Fuck you.
            SwingVector = SwingQuaternion * new Vector3(-1, 0, 0);
            //just normalizing some vectors. def could do this more efficiently, but... fuck it, it works. Only gets called once per frame.
            SwingVector = SwingVector.normalized;
            SwingQuaternion = SwingQuaternion.normalized;
            // </magic>
        }
    }

    void nextSwingInit()
    {
        gravityTriggers = GameObject.FindGameObjectsWithTag("gravityTrigger");
        //flip custom gravity zones & switches back to initial state
        foreach (GameObject GO in gravityTriggers)
        {
            GO.GetComponent<flipGravScript>().flip(GO.GetComponent<flipGravScript>().previousPosition);
        }
        /*//find what the fuck an up is
        if (inCustomGravity)
        {
            qTo = Quaternion.LookRotation(SwingVector, -CustomGravityVector);
        }
        else
        {
            if (FindClosestGravityObject() != null)
            {
                qTo = Quaternion.LookRotation(SwingVector, (transform.position - FindClosestGravityObject().transform.position));
            }
        }
        //face.. forward
        transform.rotation = qTo;
        //move that way
        transform.position += transform.up.normalized * 0.01f;*/
        mode = "start";
    }

    private void FixedUpdate()
    {
        if (mode == "moving" || mode == "Victory")
        {
            if ((!inBounds || Input.GetButton("Force OOB")) & mode == "moving")
            {//if out of bounds (or forced)
                TimeToDeath -= 1;
                if (TimeToDeath < 1 || Input.GetButton("Force OOB"))
                {
                    Debug.Log("out of bounds");
                    nextSwingInit();
                    //go back to previous swing
                    SwingVector = oldSwing[0];
                    SwingForce = oldSwing[1].x; //had to be stored in a vector3.
                    transform.position = oldSwing[2];
                    CameraTransform.position = oldSwing[3];

                }
            }
            else
            {
                //if moving slowly (before land check bc if it's moving slowly, /for 200 frames in a row/, it's on some fucking land)
                //Debug.Log((slowCheckPosition - transform.position).magnitude);
                if ((slowCheckPosition - transform.position).magnitude < 3f & mode == "moving")
                {
                    TimeToDeath -= 1;
                    if (TimeToDeath < 1)
                    {
                        rb.angularVelocity = new Vector3(0, 0, 0);
                        rb.velocity = new Vector3(0, 0, 0);
                        TimeToDeath = 200;
                        if (isInHole)
                        {
                            Debug.Log("congrate you're did it");
                            Debug.Log("Swings: " + Swings);
                            mode = "Victory";
                            Swings = 0;
                            Victory = true;
                        }
                        else
                        {
                            nextSwingInit();
                        }

                    }
                }
                else
                {
                    slowCheckPosition = transform.position;
                    TimeToDeath = 200;
                }
            }
            //Do gravity shiz
            if (!isOnLand)
            {
                GravityBois = GameObject.FindGameObjectsWithTag("HasGravity");//don't update GravityBois if on land; this is because being on land changes how gravity works, so we don't just have the ball rolling to one spot all the time.
            }
            handleCustomGravity();
            rb.AddForce(fGrav);
            OldPosition = rb.position;
        }
        if (mode == "start")
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    void OnTriggerEnter(Collider target)
    {
        if (target.CompareTag("holeTrigger"))
        {
            Physics.IgnoreLayerCollision(8, 9, true);
            isInHole = true;
        }
        if (target.CompareTag("InBounds"))
        {
            inBounds = true;
        }
        if (target.CompareTag("gravityTrigger"))
        {
            trigger = target.gameObject;
        }
    }
    void OnTriggerExit(Collider target)
    {
        if (target.CompareTag("holeTrigger"))
        {
            Physics.IgnoreLayerCollision(8, 9, false);
            isInHole = false;
        }
        if (target.CompareTag("InBounds"))
        {
            inBounds = false;
            Debug.Log("OUT OF BOUNDS");
        }
        if (target.CompareTag("gravityTrigger"))
        {
            trigger = null;
        }
    }
    void OnCollisionEnter(Collision target)
    {
        Debug.Log("landed at: " + transform.position);
        if (target.gameObject.CompareTag("HasGravity") || target.gameObject.CompareTag("collidable"))
        {
            Debug.Log("no, really");
            isOnLand = true;
            GravityBois = new GameObject[1];
            //GravityBois[0] = (target.gameObject).transform.parent.gameObject; //hey so why the fuck do I have to go through transforms to get the actual gameObject of the parent?
            GravityBois[0] = target.gameObject; //previous version had triggers to tell the ball when to have friction and when to not.. thus the nesting needed before
            if (target.gameObject.GetComponent<Collider>() != null)
            {
                Debug.Log(1);
                Debug.Log(target.gameObject);
                landFriction = target.gameObject.GetComponent<Collider>().sharedMaterial.dynamicFriction * GetComponent<Collider>().sharedMaterial.dynamicFriction;
            }
            else
            {
                Debug.Log(2);
                Debug.Log("yo bitch, there's a collider type I ain't feelin, check the collider on this object, boi");
                landFriction = target.gameObject.GetComponent<Collider>().sharedMaterial.dynamicFriction * GetComponent<Collider>().sharedMaterial.dynamicFriction;
            }
            Debug.Log("landfriction: " + landFriction);
            rb.drag = landFriction;
            rb.angularDrag = landFriction;
        }
    }
    void OnCollisionStay(Collision target)
    {
        if (target.gameObject.CompareTag("HasGravity") || target.gameObject.CompareTag("collidable"))
        {
            isOnLand = true;
            GravityBois = new GameObject[1];
            //GravityBois[0] = (target.gameObject).transform.parent.gameObject; //hey so why the fuck do I have to go through transforms to get the actual gameObject of the parent?
            GravityBois[0] = target.gameObject; //previous version had triggers to tell the ball when to have friction and when to not.. thus the nesting needed before
            if (target.gameObject.GetComponent<Collider>() != null)
            {
                Debug.Log(1);
                Debug.Log(target.gameObject);
                landFriction = target.gameObject.GetComponent<Collider>().sharedMaterial.dynamicFriction * GetComponent<Collider>().sharedMaterial.dynamicFriction;
            }
            else
            {
                Debug.Log(2);
                Debug.Log("yo bitch, there's a collider type I ain't feelin, check the collider on this object, boi");
                landFriction = target.gameObject.GetComponent<Collider>().sharedMaterial.dynamicFriction * GetComponent<Collider>().sharedMaterial.dynamicFriction;
            }
            Debug.Log("landfriction: " + landFriction);
            rb.drag = landFriction;
            rb.angularDrag = landFriction;
        }
    }

    void OnCollisionExit(Collision target)
    {
        Debug.Log("landn't");
        if (target.gameObject.CompareTag("HasGravity") || target.gameObject.CompareTag("collidable"))
        {
            BeInAir();
        }
    }

    public void BeInAir()
    {
        rb.angularDrag = 0f;
        rb.drag = 0f;
        isOnLand = false;
        GravityBois = GameObject.FindGameObjectsWithTag("HasGravity");
    }
    public GameObject FindClosestGravityObject()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("HasGravity");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    void handleCustomGravity()
    {
        //Debug.Log(1);
        inCustomGravity = false;
        foreach (GameObject GO in CustomGravityBois)
        {
            //Debug.Log(2);
            //if (GO.GetComponent<Collider>().bounds.Contains(MyTransform.position))
            if (GO.GetComponent<Collider>().ClosestPoint(MyTransform.position) == MyTransform.position)
            {
                //Debug.Log(3);
                inCustomGravity = true;
                CustomGravityVector = GO.GetComponent<customGravity>().direction.normalized * GO.GetComponent<customGravity>().magnitude;
                fGrav = CustomGravityVector;
                break;
            }
        }
        if (!inCustomGravity)
        {
            handleGravity();
        }
    }

    void handleGravity()
    {
        fGrav = new Vector3(0, 0, 0); //reset fGrav
        Vector3 distBetweenObjects; // the difference in position on each axis between any gravityboi and the ball
        float singleFGrav; //fGrav between the ball and a single gravity object
        foreach (GameObject GO in GravityBois)//for every gravityboi
        {
            if (MyTransform.position != GO.transform.position)//only calculate the gravity between the 2 objects if they're not the same object.. simplified to just their positions, to make this calculation slightly easier (it's done <steps> times per frame)
            {
                distBetweenObjects = (GO.transform.position - MyTransform.position); //get distance between the ball and the given gravityboi.
                singleFGrav = GO.GetComponent<Rigidbody>().mass / (Mathf.Pow(distBetweenObjects.magnitude, 2)) * GravConst; //fGrav = G((m1*m2)/r^2) https://en.wikipedia.org/wiki/Gravity
                fGrav += distBetweenObjects.normalized * singleFGrav;//multiply the fGrav by the direction it should be applied in
            }
        }
    }
}
        
    

