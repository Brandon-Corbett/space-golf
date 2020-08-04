using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trail : MonoBehaviour
{
    public GameObject ball; //ball.
    public Transform ballPosition; //position of ball. not sure why I didn't just use ball.GetComponent<transform> instead of making this bullshit happen, but whatever
    private UnityEngine.GameObject[] GravityBois2; //list of gameobjects that have the HasGravity tag.. has to be separate from the one used for the ball, bc that one is a lil broken (the ball sets it to the object it's colliding with, if it can.)
    private UnityEngine.GameObject[] Collidable;
    private Vector3 fGrav; //force of gravity on the simulated ball
    private float GravConst; //gravitational "constant", defined by that of the ball to avoid any differences
    public Transform MyTransform; //position / rotation of the trail object
    private Vector3 SwingVector; //swing vector as imported from ball (the direction the ball will travel in when hit)
    private float SwingForce; //swing force as imported from ball
    public int steps; //how many fixedUpdates the trail should calculate ahead
    private Vector3 velocity; //can't directly use rigidbody when updating multiple times within a single frame, because it gets pissy. So.. gotta do this manually
    private string mode; //the current game mode, as imported from ball
    public LineRenderer lineRenderer; //the lineRenderer with which to draw the ball's trail
    private Vector3[] points; //all the points along said trail, which will be imported into the lineRenderer
    private UnityEngine.GameObject[] CustomGravityBois;
    public bool inCustomGravity;
    private bool inCollision;
    public Vector3 CustomGravityVector;
    void Start()
    {
        ball = GameObject.Find("ball");//what in the everliving fuck is a ball
        GravConst = ball.GetComponent<gravity>().GravConst;//import GravConst from ball
        SwingVector = ball.GetComponent<gravity>().SwingVector;//import SwingVector from ball
        SwingForce = ball.GetComponent<gravity>().SwingForce;//import SwingForce from ball
        mode = ball.GetComponent<gravity>().mode;//import gamemode from ball
        lineRenderer.positionCount = steps;//get the lineRenderer ready

        CustomGravityBois = GameObject.FindGameObjectsWithTag("CustomGravity");
    }

    // Update is called once per frame
    void Update()
    {
        mode = ball.GetComponent<gravity>().mode;//check if gamemode has been updated
        if (mode == "start")
        {
            GravConst = ball.GetComponent<gravity>().GravConst;//check if GravConst has been updated (it shouldn't)
            SwingVector = ball.GetComponent<gravity>().SwingVector;//check if SwingVector has been updated (the player changed the arc they were going for)
            SwingForce = ball.GetComponent<gravity>().SwingForce;//check if SwingForce has been updated (again, by the player)
            MyTransform.position = ballPosition.position;//start the trail and physics at the actual ball's position..
            velocity = SwingVector * SwingForce / 50;//set velocity on a per-FixedUpdate basis; there are 50 in a second
            points = new Vector3[steps];//clear the points list
            inCollision = false;
            for (int i = 0; i < steps; i++)//for every physics step
            {
                if (!inCollision)
                {
                GravityBois2 = GameObject.FindGameObjectsWithTag("HasGravity"); //get those gravitybois
                Collidable = GameObject.FindGameObjectsWithTag("collidable");
                handleCollision();
                handleCustomGravity();//see below
                velocity += fGrav / 50;//divide by 50 because of that whole 50 fixed updates in a second thing
                MyTransform.position += (velocity) / 50;//divide by 50 because of that whole 50 fixed updates in a second thing
                }
                points[i] = MyTransform.position;//add the calculated point to the list
            }
            lineRenderer.SetPositions(points);//import the points list into the lineRenderer, for line rendering things
        }
    }

    void handleCollision()
    {
        inCollision = false;
        foreach (GameObject GO in Collidable)
        {
            if (GO.GetComponent<Collider>().ClosestPoint(MyTransform.position) == MyTransform.position)
            {
                inCollision = true;
                break;
            }
        }
        foreach (GameObject GO in GravityBois2)
        {
            if (GO.GetComponent<Collider>().ClosestPoint(MyTransform.position) == MyTransform.position)
            {
                inCollision = true;
                break;
            }
        }
    }
    void handleCustomGravity()
    {
        inCustomGravity = false;
        foreach (GameObject GO in CustomGravityBois)
        {
            if (GO.GetComponent<Collider>().ClosestPoint(MyTransform.position) == MyTransform.position)
            {
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
        foreach (GameObject GO in GravityBois2)//for every gravityboi
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
