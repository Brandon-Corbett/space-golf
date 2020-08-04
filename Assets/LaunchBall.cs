using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchBall : MonoBehaviour
{
    public Vector3 LaunchVector;
    public GameObject ball;
    public Vector3 finalPosition;
    public int holeNumber;
    private bool hasLaunched = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (ball.GetComponent<gravity>().Victory & ball.GetComponent<gravity>().hole == holeNumber)
        {
            if (!hasLaunched)
            {
                ball.GetComponent<Transform>().position = transform.position + transform.up*0.2f;
                ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                ball.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                ball.GetComponent<Rigidbody>().AddForce(LaunchVector * ball.GetComponent<Rigidbody>().mass);
                hasLaunched = true;
                ball.GetComponent<gravity>().isOnLand = false;
            }
            else
            {
                if (ball.GetComponent<gravity>().isOnLand & (ball.GetComponent<Transform>().position - finalPosition).magnitude < 1f)//if touching the ground.. and close enough to where it's supposed to be
                {
                    ball.GetComponent<gravity>().hole += 1;
                    ball.GetComponent<gravity>().Victory = false;
                    //no motion pls
                    ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                    ball.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 0, 0);
                    //ball.GetComponent<Transform>().position = finalPosition;
                    //next swing
                    ball.GetComponent<gravity>().mode = "start";
                }
            }
        }
    }
}
