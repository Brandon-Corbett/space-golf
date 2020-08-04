using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goToBall : MonoBehaviour
{
    public Transform BallPosition;
    public Rigidbody BallRB;
    public Transform MyPosition;
    public Rigidbody MyRB;
    private Quaternion q = new Quaternion();
    public GameObject ball;
    private Vector3 BallGrav;
    
    // Start is called before the first frame update
    void Start()
    {
        BallGrav = ball.GetComponent<gravity>().fGrav;
        MyPosition.position = BallPosition.position;
    }

    // Update is called once per frame
    private void FixedUpdate() {
        MyPosition.position = (MyPosition.position * 9 + BallPosition.position) / 10;
        if (BallRB.velocity.sqrMagnitude > 4f)
        {
            MyPosition.LookAt(BallPosition.position);
            q.SetLookRotation((BallPosition.position - MyPosition.position), BallGrav);
            MyPosition.localRotation = q;
            /*q.SetLookRotation((BallRB.velocity), BallGrav);
            MyPosition.localRotation = q;*/
        }
        //Quaternion.Inverse(MyPosition.localRotation);
    }
}
