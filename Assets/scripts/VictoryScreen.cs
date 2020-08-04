using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryScreen : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject ball;
    private bool Victory;
    public CanvasGroup canvasGroup;
    void Start()
    {
        Victory = ball.GetComponent<gravity>().Victory;
    }

    // Update is called once per frame
    void Update()
    {
        Victory = ball.GetComponent<gravity>().Victory;
        if (Victory)
        {
            canvasGroup.alpha = 1f;
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }
}
