using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraScript : MonoBehaviour
{
    public Transform cameraTransform; //position and rotation of the camera
    public Transform target; //ball followey boi
    public Transform ball; //ball
    private Vector3 upVector;
    public float upDistance = 7.0f; //the distance the camera should be above the ball (given its up vector, not world up)
    public float backDistance = 10.0f; //the distance the camera should be behind the ball
    public float trackingSpeed = 3.0f; //how quickly the camera should be able to move
    public float rotationSpeed = 9.0f; //how quickly the camera should be able to rotate
    public float zoomSpeed; //how much zooming in and out moves the camera
    private Vector3 Oldposition; //the position the ball was in the last time the camera was updated. this exists to keep the camera from flipping out when changing directions a lot
    private Quaternion OldRotation; //the rotation of the camera the last time it was updated. this exists to keeo the camera from flipping out when moving along the up vector.
    private Vector3 OldUp;
    private bool isOnLand; //I think you can figure out what this one is for
    private bool inCustomGravity;
    private string mode;

    public Vector3 Debug1;
    public Vector3 Debug2;
    public float Debug3;

    private Vector3 targetCameraPosition;
    private Quaternion targetCameraRotation;

    private void Start()
    {
        Oldposition = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);//generate an impossible position at the start, so the camera is forced to update the first frame it can
    }
    void LateUpdate()
    {
        //define some things
        mode = GameObject.Find("ball").GetComponent<gravity>().mode;
        isOnLand = GameObject.Find("ball").GetComponent<gravity>().isOnLand;
        inCustomGravity = GameObject.Find("ball").GetComponent<gravity>().inCustomGravity;
        upVector = transform.up; ;
        if (inCustomGravity)
        {
            upVector = -GameObject.Find("ball").GetComponent<gravity>().CustomGravityVector;
        }
        if (GameObject.Find("ball").GetComponent<gravity>().mode == "moving" || GameObject.Find("ball").GetComponent<gravity>().mode == "Victory")
        {//if not in the "start" state (so.. if the ball is moving)
            if (Input.GetMouseButton(1))//player controlled camera
            {
                transform.RotateAround(target.position, upVector, -300 * Time.deltaTime * -Input.GetAxis("Mouse X"));
                transform.RotateAround(target.position, transform.right, 300 * Time.deltaTime * -Input.GetAxis("Mouse Y"));
                targetCameraPosition = cameraTransform.position;
            }
            else
            {
                if ((Oldposition - target.position).magnitude > 20 || !isOnLand)//also that "5" value is incredibly arbitrary, and should definitely be a serialized or public variable, but fuck you I'm too lazy rn
                {
                    //construct target camera position (behind and above the ball)
                    /*if (isOnLand)
                    {
                        targetCameraPosition = target.position - target.forward * backDistance + upVector * upDistance;
                    }
                    else*/
                    /*if (!isOnLand)
                    {
                        targetCameraPosition = target.position - target.forward * backDistance;
                    }*/
                    //note: it will keep this position next time, so it only needs to be written when it needs to be changed, not every frame
                    //reset old position, so we have a new position to read from to tell if the ball is /really/ moving
                    Oldposition = target.position;
                }
                // s l e r m p on over to that position (it is kept from the last time it was updated in the above if statement)
                transform.position = Vector3.Slerp(transform.position, targetCameraPosition, trackingSpeed * Time.deltaTime);
                // set distance from ball to a constant value... well, slerp to it, because otherwise jitter happens
                transform.position = Vector3.Slerp(transform.position, ball.position + ((transform.position - ball.position).normalized * Mathf.Sqrt(upDistance * upDistance + backDistance * backDistance)), trackingSpeed * Time.deltaTime * 2);
                transform.RotateAround(target.position, upVector, -300 * Time.deltaTime * Input.GetAxis("CameraHorizontal"));
                transform.RotateAround(target.position, transform.right, 300 * Time.deltaTime * Input.GetAxis("CameraVertical"));
                if (Input.GetAxis("CameraHorizontal") != 0 || Input.GetAxis("CameraVertical") != 0)
                {
                    targetCameraPosition = cameraTransform.position;
                }
            }
            //where the fuck should I point the camera (at the ball,) and facing up
            targetCameraRotation = Quaternion.LookRotation((target.position * 5 + ball.position) / 6 - transform.position, (-GameObject.Find("ball").GetComponent<gravity>().fGrav * ((upVector.normalized - (targetCameraRotation.normalized * Vector3.up)).sqrMagnitude)) + (OldUp * (1 - (upVector.normalized - (targetCameraRotation.normalized * Vector3.up)).sqrMagnitude)));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetCameraRotation, rotationSpeed * Time.deltaTime);
            OldUp = upVector;
            OldRotation = transform.rotation;
        }
        //if (GameObject.Find("ball").GetComponent<gravity>().mode != "start")
        //{//if not in the "start" state (so.. if the ball is moving)
        //    if (Input.GetMouseButton(1))//player controlled camera
        //    {
        //        transform.RotateAround(target.position, upVector, -300 * Time.deltaTime * -Input.GetAxis("Mouse X"));
        //        transform.RotateAround(target.position, transform.right, 300 * Time.deltaTime * -Input.GetAxis("Mouse Y"));
        //    }
        //    else
        //    {
        //        if (!GameObject.Find("ball").GetComponent<gravity>().isInHole)
        //        {
        //            //only move the camera if the ball has moved significantly (if on land), or always move it when not on land, because...
        //            //well when it's flying through the air, it's not going to change direction too much, so it won't fuck with the camera.
        //            //this if statement just keeps it from spazzing out when the ball is changing direction a lot (in the hole)
        //            if ((Oldposition - target.position).magnitude > 2 || !isOnLand)//also that "5" value is incredibly arbitrary, and should definitely be a serialized or public variable, but fuck you I'm too lazy rn
        //            {
        //                //construct target camera position (behind and above the ball)
        //                if (isOnLand)
        //                {
        //                    targetCameraPosition = target.position - target.forward * backDistance + upVector * upDistance;
        //                }
        //                else
        //                {
        //                    targetCameraPosition = target.position - target.forward * backDistance;
        //                }
        //                //note: it will keep this position next time, so it only needs to be written when it needs to be changed, not every frame
        //                //reset old position, so we have a new position to read from to tell if the ball is /really/ moving
        //                Oldposition = target.position;
        //            }
        //            // s l e r m p on over to that position (it is kept from the last time it was updated in the above if statement)
        //            transform.position = Vector3.Slerp(transform.position, targetCameraPosition, trackingSpeed * Time.deltaTime);
        //            // set distance from ball to a constant value
        //            transform.position = ball.position + ((transform.position - ball.position).normalized * Mathf.Sqrt(upDistance * upDistance + backDistance * backDistance));
        //            transform.RotateAround(target.position, upVector, -300 * Time.deltaTime * Input.GetAxis("CameraHorizontal"));
        //            transform.RotateAround(target.position, transform.right, 300 * Time.deltaTime * Input.GetAxis("CameraVertical"));
        //        }
        //    }
        //    //where the fuck should I point the camera (at the ball,) and facing up
        //    targetCameraRotation = Quaternion.LookRotation((target.position * 5 + ball.position) / 6 - transform.position, (-GameObject.Find("ball").GetComponent<gravity>().fGrav * ((upVector.normalized - (targetCameraRotation.normalized * Vector3.up)).sqrMagnitude)) + (OldUp * (1 - (upVector.normalized - (targetCameraRotation.normalized * Vector3.up)).sqrMagnitude)));
        //    transform.rotation = Quaternion.Slerp(transform.rotation, targetCameraRotation, rotationSpeed * Time.deltaTime);
        //    OldUp = upVector;
        //    OldRotation = transform.rotation;
        //}
        else
        {//if we're waiting on the player to make their move
            //don't fuck with the camera while the ball isn't moving.
            //player input
            //zoom
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                transform.position = Vector3.Lerp(transform.position, transform.position + ((transform.position - ball.position) * -Input.GetAxis("Mouse ScrollWheel") * zoomSpeed), 0.1f);
            }
            //rotate camera around ball
            transform.RotateAround(target.position, upVector, -300 * Time.deltaTime * Input.GetAxis("CameraHorizontal"));
            transform.RotateAround(target.position, transform.right, 300 * Time.deltaTime * Input.GetAxis("CameraVertical"));
            if (Input.GetAxis("CameraHorizontal") != 0 || Input.GetAxis("CameraVertical") != 0)
            {
                targetCameraPosition = cameraTransform.position;
            }
            if (Input.GetMouseButton(1))
            {
                transform.RotateAround(target.position, upVector, -300 * Time.deltaTime * -Input.GetAxis("Mouse X"));
                transform.RotateAround(target.position, transform.right, 300 * Time.deltaTime * -Input.GetAxis("Mouse Y"));
                targetCameraPosition = cameraTransform.position;
            }
            if (inCustomGravity)
            {
                targetCameraRotation = Quaternion.LookRotation(ball.position - transform.position, upVector);
            }
            else
            {
                if (FindClosestGravityObject() != null)
                {
                    targetCameraRotation = Quaternion.LookRotation(ball.position - transform.position, (ball.position - FindClosestGravityObject().transform.position));
                }
            }
            //correct the camera's up vector
            transform.rotation = targetCameraRotation;
        }
    }
    public GameObject FindClosestGravityObject()//you can figure out how this works;
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("HasGravity");
        GameObject closest = null;
        float distance = Mathf.Infinity;
        Vector3 position = target.transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            if (diff.sqrMagnitude < distance)
            {
                closest = go;
                distance = diff.sqrMagnitude;
            }
        }
        return closest;
    }
}
