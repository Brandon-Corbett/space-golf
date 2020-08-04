using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flipGravScript : MonoBehaviour
{
    public GameObject parent;
    public Vector3 Direction0;
    public Vector3 Direction1;
    public int direction = 0;
    public Renderer myRenderer;
    public GameObject ball;
    public int previousPosition = 0;

    void FixedUpdate()
    {
        ball = GameObject.Find("ball");
        if (ball.GetComponent<gravity>().mode == "start") previousPosition = direction;
        if (ball.GetComponent<gravity>().trigger == gameObject)
        {
            Debug.Log(gameObject);
            ball.GetComponent<gravity>().trigger = null;
            parent = transform.parent.gameObject;
            direction = 1 - direction;
            flip(direction);

        }
    }

    public void flip(int dir)
    {
        if (dir == 0)
        {
            parent.GetComponent<customGravity>().direction = Direction0;
            parent.GetComponent<Renderer>().material.color = new Color(1, 0, 0, 0.125f);
            myRenderer.material.color = new Color(0, 0, 1, 0.3f);
        }
        else
        {
            parent.GetComponent<customGravity>().direction = Direction1;
            parent.GetComponent<Renderer>().material.color = new Color(0, 0, 1, 0.125f);
            myRenderer.material.color = new Color(1, 0, 0, 0.3f); ;
        }
        direction = dir;
    }
}
